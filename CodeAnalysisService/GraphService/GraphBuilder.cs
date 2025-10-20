using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.NodeBuilder;
using CodeAnalysisService.GraphService.EdgeBuilder;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Helpers;
using System.Collections.Concurrent;
using CodeAnalysisService.GraphService.Registry;

namespace CodeAnalysisService.GraphService
{
    /// <summary>
    /// Builds a graph from Roslyn models.
    /// </summary>
    public class GraphBuilder
    {
        private readonly Compilation _compilation;
        private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels;

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
                new MethodNodeBuilder(),
                new PropertyNodeBuilder(),
                new FieldNodeBuilder(),
                new EventNodeBuilder()
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

            Parallel.ForEach(_semanticModels, semanticModel =>
            {
                var root = semanticModel.Value.SyntaxTree.GetRoot();

                foreach (var node in root.DescendantNodes())
                {
                    var nodeType = node.GetType();
                    var builder = _nodeBuilders.FirstOrDefault(b =>
                        b.SyntaxTypes.Any(t => t.IsAssignableFrom(nodeType)));

                    if (builder == null) continue;

                    foreach (var entry in builder.BuildNode(node, semanticModel.Value))
                        entries.Add(entry);
                }
            });

            foreach (var (symbol, node) in entries)
            {
                Registry.AddNode(symbol, node);
            }
        }

        private void BuildEdges()
        {
            Parallel.ForEach(Registry.GetAll<INode>(), node =>
            {
                var builder = _edgeBuilders.FirstOrDefault(b => b.NodeType == node.NodeType);
                if (builder == null) return;

                var model = _semanticModels[node.Syntax.SyntaxTree];
    
                var edges = builder.BuildEdges(node, Registry, model);
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
    }
}
