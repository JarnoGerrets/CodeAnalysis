using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Helpers;
using CodeAnalysisService.GraphService.Context;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Builds edges for <see cref="ClassNode"/>s, including inheritance,
    /// implemented interfaces, fields, properties, methods, and constructors.
    /// </summary>
    public class ClassEdgeBuilder : IEdgeBuilder
    {
        public NodeType NodeType => NodeType.Class;

        public IEnumerable<EdgeNode> BuildEdges( INode node, NodeRegistry registry, Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            if (node is not ClassNode classNode) return Enumerable.Empty<EdgeNode>();

            var edges = new List<EdgeNode>();
            var model = semanticModels[classNode.ClassSyntax.SyntaxTree];
            var symbol = classNode.Symbol;

            // Inherits
            if (symbol.BaseType != null && symbol.BaseType.Name != "Object")
            {
                var baseNode = registry.GetNode<ClassNode>(symbol.BaseType);
                if (baseNode != null)
                    edges.Add(new EdgeNode
                    {
                        Target = baseNode,
                        Type = EdgeType.Inherits
                    });
            }

            // Implements
            foreach (var iface in symbol.Interfaces)
            {
                var ifaceNode = registry.GetNode<InterfaceNode>(iface);
                if (ifaceNode != null)
                    edges.Add(new EdgeNode
                    {
                        Target = ifaceNode,
                        Type = EdgeType.Implements
                    });
            }

            foreach (var member in classNode.ClassSyntax.Members)
            {
                switch (member)
                {
                    // HasFieldElement
                    case FieldDeclarationSyntax fDecl:
                        foreach (var fieldSymbol in fDecl.Declaration.Variables.Select(v => model.GetDeclaredSymbol(v)).OfType<IFieldSymbol>())
                        {
                            var fnode = registry.GetNode<FieldNode>(fieldSymbol);
                            if (fnode != null)
                                edges.Add(new EdgeNode
                                {
                                    Target = fnode,
                                    Type = EdgeType.HasField
                                });

                            var eType = TypeHelper.GetElementType(fieldSymbol.Type);
                            if (eType is INamedTypeSymbol namedElem)
                            {
                                var eNode = registry.GetNode<ClassNode>(namedElem);
                                if (eNode != null)
                                    edges.Add(new EdgeNode
                                    {
                                        Target = eNode,
                                        Type = EdgeType.HasFieldElement
                                    });
                            }
                        }
                        break;
                    // HasPropertyElement
                    case PropertyDeclarationSyntax pDecl:
                        if (model.GetDeclaredSymbol(pDecl) is IPropertySymbol propSymbol)
                        {
                            var pNode = registry.GetNode<PropertyNode>(propSymbol);
                            if (pNode != null)
                                edges.Add(new EdgeNode
                                {
                                    Target = pNode,
                                    Type = EdgeType.HasProperty
                                });

                            var eType = TypeHelper.GetElementType(propSymbol.Type);
                            if (eType is INamedTypeSymbol namedElem)
                            {
                                var eNode = registry.GetNode<ClassNode>(namedElem);
                                if (eNode != null)
                                    edges.Add(new EdgeNode
                                    {
                                        Target = eNode,
                                        Type = EdgeType.HasPropertyElement
                                    });
                            }
                        }
                        break;
                    // HasMethod
                    case MethodDeclarationSyntax mDecl:
                        if (model.GetDeclaredSymbol(mDecl) is IMethodSymbol methodSymbol)
                        {
                            var mNode = registry.GetNode<MethodNode>(methodSymbol);
                            if (mNode != null)
                                edges.Add(new EdgeNode
                                {
                                    Target = mNode,
                                    Type = EdgeType.HasMethod
                                });
                        }
                        break;
                    // HasConstructor
                    case ConstructorDeclarationSyntax cDecl:
                        if (model.GetDeclaredSymbol(cDecl) is IMethodSymbol ctorSymbol)
                        {
                            var cNode = registry.GetNode<ConstructorNode>(ctorSymbol);
                            if (cNode != null)
                                edges.Add(new EdgeNode
                                {
                                    Target = cNode,
                                    Type = EdgeType.HasConstructor
                                });
                        }
                        break;
                        // HasEvent
                    case EventDeclarationSyntax eDecl:
                    {
                        if (model.GetDeclaredSymbol(eDecl) is IEventSymbol evtSymbol)
                        {
                            var eNode = registry.GetNode<EventNode>(evtSymbol);
                            if (eNode != null)
                                edges.Add(new EdgeNode
                                {
                                    Target = eNode,
                                    Type = EdgeType.HasEvent
                                });
                        }
                        break;
                    }
                    // HasEvent
                    case EventFieldDeclarationSyntax efDecl:
                    {
                        foreach (var variable in efDecl.Declaration.Variables)
                        {
                            if (model.GetDeclaredSymbol(variable) is IEventSymbol evtSymbol)
                            {
                                var eNode = registry.GetNode<EventNode>(evtSymbol);
                                if (eNode != null)
                                    edges.Add(new EdgeNode
                                    {
                                        Target = eNode,
                                        Type = EdgeType.HasEvent
                                    });
                            }
                        }
                        break;
                    }

                }
            }
            return edges;
        }
    }
}
