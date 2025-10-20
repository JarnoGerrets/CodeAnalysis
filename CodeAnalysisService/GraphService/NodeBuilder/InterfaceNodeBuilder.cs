using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="InterfaceNode"/> instances for interface declarations 
    /// in a syntax tree using Roslyn symbols.
    /// </summary>
    public class InterfaceNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Interface;
        public Type SyntaxType => typeof(InterfaceDeclarationSyntax);
        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is not InterfaceDeclarationSyntax iface)
                yield break;

            if (model.GetDeclaredSymbol(iface) is INamedTypeSymbol symbol)
            {
                yield return (symbol, new InterfaceNode
                {
                    InterfaceSyntax = iface,
                    Symbol = symbol
                });
            }
        }
    }
}
