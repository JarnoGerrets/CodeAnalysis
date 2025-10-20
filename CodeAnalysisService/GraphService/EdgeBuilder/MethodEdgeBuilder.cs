using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Helpers;
using CodeAnalysisService.GraphService.Registry;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="MethodNode"/>
    /// </summary>
    public class MethodEdgeBuilder : IEdgeBuilder
    {
        private CallResolver? _callResolver;

        public NodeType NodeType => NodeType.Method;

        public IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, SemanticModel model)
        {
            if (node is not MethodNode methodNode) return Enumerable.Empty<EdgeNode>();
            if (_callResolver == null) _callResolver = new CallResolver(registry.GetAll<ClassNode>(), registry.GetAll<MethodNode>());

            var edges = new List<EdgeNode>();
            var symbol = methodNode.Symbol;
            // Returns
            if (symbol.ReturnType is INamedTypeSymbol returnType && registry.GetNode<ClassNode>(returnType) is { } returnNode)
            {
                edges.Add(new EdgeNode
                {
                    Target = returnNode,
                    Type = EdgeType.Returns
                });
            }
            // Uses
            foreach (var paramType in symbol.Parameters.Select(p => p.Type).OfType<INamedTypeSymbol>())
            {
                if (registry.GetNode<ClassNode>(paramType) is { } paramNode)
                {
                    edges.Add(new EdgeNode
                    {
                        Target = paramNode,
                        Type = EdgeType.Uses
                    });
                }
            }

            var collectionVariables = new Dictionary<string, ITypeSymbol>();
            foreach (var foreachStmt in methodNode.MethodSyntax.DescendantNodes().OfType<ForEachStatementSyntax>())
            {
                var collectionType = model.GetTypeInfo(foreachStmt.Expression).Type;
                if (collectionType == null) continue;

                var elementType = TypeHelper.GetElementType(collectionType) ?? collectionType;
                if (elementType != null) collectionVariables[foreachStmt.Identifier.Text] = elementType;
            }

            foreach (var descendant in methodNode.MethodSyntax.DescendantNodes())
            {
                switch (descendant)
                {
                    case InvocationExpressionSyntax invocation:
                        HandleInvocation(invocation, model, registry, collectionVariables, edges);
                        break;
                    // Creates
                    case ObjectCreationExpressionSyntax objCreation:
                        if (model.GetTypeInfo(objCreation).Type is INamedTypeSymbol createdType &&
                            registry.GetNode<ClassNode>(createdType) is { } createdNode)
                        {
                            edges.Add(new EdgeNode
                            {
                                Target = createdNode,
                                Type = EdgeType.Creates
                            });
                        }
                        break;
                    // Uses
                    case IdentifierNameSyntax id:
                        var sym = model.GetSymbolInfo(id).Symbol;
                        switch (sym)
                        {
                            case IFieldSymbol fs when registry.GetNode<FieldNode>(fs) is { } fieldNode:
                                edges.Add(new EdgeNode
                                {
                                    Target = fieldNode,
                                    Type = EdgeType.ReferencesField
                                });
                                break;
                            case IPropertySymbol ps when registry.GetNode<PropertyNode>(ps) is { } propNode:
                                edges.Add(new EdgeNode
                                {
                                    Target = propNode,
                                    Type = EdgeType.Uses
                                });
                                break;
                        }
                        break;
                }
            }

            if (symbol.IsOverride && symbol.OverriddenMethod != null &&
                registry.GetNode<MethodNode>(symbol.OverriddenMethod) is { } overriddenNode)
            {
                edges.Add(new EdgeNode
                {
                    Target = overriddenNode,
                    Type = EdgeType.Overrides
                });
            }

            if (symbol.ContainingType.TypeKind == TypeKind.Interface && _callResolver != null)
            {
                foreach (var implNode in _callResolver.GetImplementations(symbol))
                    edges.Add(new EdgeNode
                    {
                        Target = implNode,
                        Type = EdgeType.ImplementedBy
                    });
            }

            return edges;
        }

        // ---------- Helpers ----------

        private void HandleInvocation(InvocationExpressionSyntax invocation, SemanticModel model, NodeRegistry registry,
            Dictionary<string, ITypeSymbol> collectionVariables, List<EdgeNode> edges)
        {
            IMethodSymbol? calledSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            if (calledSymbol == null && invocation.Expression is MemberAccessExpressionSyntax maes && maes.Expression is IdentifierNameSyntax id &&
                collectionVariables.TryGetValue(id.Identifier.Text, out var elemType) &&
                elemType is INamedTypeSymbol namedType)
            {
                calledSymbol = namedType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == maes.Name.Identifier.Text);
            }
            // Calls
            if (calledSymbol != null)
            {
                if (registry.GetNode<MethodNode>(calledSymbol) is { } targetMethod)
                {
                    edges.Add(new EdgeNode
                    {
                        Target = targetMethod,
                        Type = EdgeType.Calls
                    });
                }
                else if (calledSymbol.ContainingType.TypeKind == TypeKind.Interface && _callResolver != null)
                {
                    foreach (var implNode in _callResolver.GetImplementations(calledSymbol))
                        edges.Add(new EdgeNode
                        {
                            Target = implNode,
                            Type = EdgeType.Calls
                        });
                }
            }
        }
    }
}
