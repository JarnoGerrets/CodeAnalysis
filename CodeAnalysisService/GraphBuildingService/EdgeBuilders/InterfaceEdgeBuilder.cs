using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.GraphBuildingService.Registry;

namespace CodeAnalysisService.GraphBuildingService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="InterfaceNode"/>
    /// </summary>
    public class InterfaceEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Interface;

        public IEnumerable<EdgeNode> BuildEdges( INode node, NodeRegistry registry, SemanticModel model)
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
