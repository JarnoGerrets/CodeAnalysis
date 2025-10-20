using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Helpers;
using CodeAnalysisService.GraphService.Registry;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="PropertyNode"/>
    /// </summary>

    public class PropertyEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Property;

        public IEnumerable<EdgeNode> BuildEdges( INode node, NodeRegistry registry, SemanticModel model)
        {
            if (node is not PropertyNode propertyNode) return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();

            foreach (var symbol in propertyNode.ReferencedSymbols)
            {
                switch (symbol)
                {
                    case IFieldSymbol fieldSymbol
                        when registry.GetNode<FieldNode>(fieldSymbol) is { } fieldNode:
                        edges.Add(new EdgeNode
                        {
                            Target = fieldNode,
                            Type = EdgeType.ReferencesField
                        });
                        break;

                    case INamedTypeSymbol classSymbol:
                        if (registry.GetNode<ClassNode>(classSymbol) is { } classNode)
                        {
                            edges.Add(new EdgeNode
                            {
                                Target = classNode,
                                Type = EdgeType.Uses
                            });
                        }

                        if (TypeHelper.GetElementType(classSymbol) is INamedTypeSymbol named && registry.GetNode<ClassNode>(named) is { } elemNode)
                        {
                            edges.Add(new EdgeNode
                            {
                                Target = elemNode,
                                Type = EdgeType.HasPropertyElement
                            });
                        }
                        break;
                }
            }

            return edges;
        }
    }
}
