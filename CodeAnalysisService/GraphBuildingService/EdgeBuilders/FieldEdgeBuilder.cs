using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.Helpers;
using CodeAnalysisService.GraphBuildingService.Registry;

namespace CodeAnalysisService.GraphBuildingService.EdgeBuilder
{
    //// <summary>
    /// Builds edges for <see cref="FieldNode"/>
    /// </summary>
    public class FieldEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Field;

        public IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, SemanticModel model)
        {
            if (node is not FieldNode fieldNode) return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();
            var fieldType = fieldNode.Symbol.Type as INamedTypeSymbol;
            if (fieldType == null) return edges;

            // Uses
            if (registry.GetNode<ClassNode>(fieldType) is ClassNode classNode)
            {
                edges.Add(new EdgeNode { Target = classNode, Type = EdgeType.Uses });
            }

            // Has Field Element
            if (TypeHelper.GetElementType(fieldType) is INamedTypeSymbol elemNamed)
            {
                var elemNode = registry.GetNode<ClassNode>(elemNamed);
                if (elemNode != null)
                {
                    edges.Add(new EdgeNode { Target = elemNode, Type = EdgeType.HasFieldElement });
                }
            }

            return edges;
        }
    }

}
