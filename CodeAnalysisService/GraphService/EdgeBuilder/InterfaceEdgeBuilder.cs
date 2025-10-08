using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="InterfaceNode"/>
    /// </summary>
    public class InterfaceEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Interface;

        public IEnumerable<EdgeNode> BuildEdges( INode node, NodeRegistry registry, Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            if (node is not InterfaceNode interfaceNode) return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();
            var symbol = interfaceNode.Symbol;

            foreach (var baseIface in symbol.Interfaces)
            {
                if (registry.GetNode<InterfaceNode>(baseIface) is InterfaceNode baseIfaceNode)
                {
                    edges.Add(new EdgeNode
                    {
                        Target = baseIfaceNode,
                        Type = EdgeType.Inherits
                    });
                }
            }

            return edges;
        }
    }
}
