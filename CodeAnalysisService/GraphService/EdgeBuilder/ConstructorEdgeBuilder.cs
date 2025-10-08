using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="ConstructorNode"/>
    /// </summary>
    public class ConstructorEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Constructor;

        public IEnumerable<EdgeNode> BuildEdges(
            INode node,
            NodeRegistry registry,
            Compilation compilation,
            Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            if (node is not ConstructorNode ctorNode) return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();
            // HasConstructor
            var containingType = ctorNode.Symbol?.ContainingType;
            if (containingType != null)
            {
                var cNode = registry.GetNode<ClassNode>(containingType);
                if (cNode != null)
                {
                    edges.Add(new EdgeNode
                    {
                        Target = ctorNode,
                        Type = EdgeType.HasConstructor
                    });
                }
            }
            // Creates
            var model = semanticModels[ctorNode.ConstructorSyntax.SyntaxTree];
            var createdTypes = ctorNode.ConstructorSyntax.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                .Select(expr => model.GetTypeInfo(expr).Type as INamedTypeSymbol).Where(t => t != null);

            foreach (var createdType in createdTypes!)
            {
                if (createdType == null)
                continue;

                var created = registry.GetNode<ClassNode>(createdType);
                if (created != null)
                {
                    edges.Add(new EdgeNode
                    {
                        Target = created,
                        Type = EdgeType.Creates
                    });
                }
            }

            return edges;
        }
    }
}

