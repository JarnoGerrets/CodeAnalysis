using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.NodeBuilder;
using CodeAnalysisService.GraphService.EdgeBuilder;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.GraphService.Helpers;
using System.Collections.Concurrent;

namespace CodeAnalysisService.GraphService
{
    /// <summary>
    /// Builds a graph from Roslyn models.
    /// </summary>
    public class GraphBuilder
    {
        private readonly Compilation _compilation;
        private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels;
        private CallResolver? _callResolver;

        private readonly List<INodeBuilder> _nodeBuilders;
        private readonly List<IEdgeBuilder> _edgeBuilders;

        public NodeRegistry Registry { get; } = new();
        
        public GraphBuilder(Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            _compilation = compilation;
            _semanticModels = semanticModels;

            _nodeBuilders = new List<INodeBuilder>
            {
                new ClassNodeBuilder(),
                new InterfaceNodeBuilder(),
                new ConstructorNodeBuilder(),
                new EventNodeBuilder(),
                new MethodNodeBuilder(),
                new PropertyNodeBuilder(),
                new FieldNodeBuilder()
            };

            _edgeBuilders = new List<IEdgeBuilder>
            {
                new ClassEdgeBuilder(),
                new InterfaceEdgeBuilder(),
                new ConstructorEdgeBuilder(),
                new EventEdgeBuilder(),
                new MethodEdgeBuilder(),
                new PropertyEdgeBuilder(),
                new FieldEdgeBuilder()
            };
        }

        public void BuildGraph()
        {
            BuildNodes();
            BuildEdges();
        }

        private void BuildNodes()
        {
            var entries = new ConcurrentBag<(ISymbol Symbol, INode Node)>();
            var ctx = new GraphContext(_semanticModels, Registry);

            Parallel.ForEach(_semanticModels, kvp =>
            {
                var (tree, model) = (kvp.Key, kvp.Value);

                foreach (var builder in _nodeBuilders)
                {
                    foreach (var entry in builder.BuildNodes(ctx, tree, model))
                    {
                        entries.Add(entry);
                    }
                }
            });
            AddNodesSafe(entries);
        }



        private void BuildEdges()
        {
            _callResolver = new CallResolver(
                Registry.GetAll<ClassNode>(),
                Registry.GetAll<MethodNode>()
            );

            foreach (var builder in _edgeBuilders)
            {
                builder.WithCallResolver(_callResolver);
            }

            Parallel.ForEach(Registry.GetAll<INode>(), node =>
            {
                var builder = _edgeBuilders.FirstOrDefault(b => b.NodeType == node.NodeType);
                if (builder == null) return;
                
                var edges = builder.BuildEdges(node, Registry, _compilation, _semanticModels);
                if (edges == null) return;

                lock (node.SyncRoot)
                {
                    var set = new HashSet<EdgeNode>(node.Edges, Comparers.Edge);
                    foreach (var e in edges)
                    {
                        set.Add(e);
                    }

                    node.Edges.Clear();
                    node.Edges.AddRange(set);
                }
            });
        }


        private void AddNodesSafe(IEnumerable<(ISymbol Symbol, INode Node)> entries)
        {
            foreach (var (symbol, node) in entries)
            {
                Registry.AddNode(symbol, node);
            }
        }
    }
}
