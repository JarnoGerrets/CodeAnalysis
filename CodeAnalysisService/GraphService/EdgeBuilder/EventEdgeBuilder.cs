using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="EventNode"/>
    /// </summary>
    public class EventEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Event;

        public IEnumerable<EdgeNode> BuildEdges( INode node, NodeRegistry registry, Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            if (node is not EventNode eventNode)
                return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();
            var symbol = eventNode.Symbol;

            // Uses event
            if (symbol.Type is INamedTypeSymbol delegateType && registry.GetNode<ClassNode>(delegateType) is { } delegateNode)
            {
                edges.Add(new EdgeNode
                {
                    Target = delegateNode,
                    Type = EdgeType.Uses
                });
            }

            // HasEvent
            if (symbol.ContainingType is INamedTypeSymbol containingType && registry.GetNode<ClassNode>(containingType) is { } classNode)
            {
                edges.Add(new EdgeNode
                {
                    Target = classNode,
                    Type = EdgeType.HasEvent
                });
            }
            
            return edges;
        }
    }
}
