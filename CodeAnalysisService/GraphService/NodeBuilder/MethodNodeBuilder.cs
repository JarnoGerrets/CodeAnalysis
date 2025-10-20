using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="MethodNode"/> instances for method declarations 
    /// in classes using Roslyn symbols.
    /// </summary>
    public class MethodNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Method;

        public Type SyntaxType => typeof(MethodDeclarationSyntax);

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is not MethodDeclarationSyntax method)
                yield break;

            if (model.GetDeclaredSymbol(method) is IMethodSymbol symbol)
            {
                yield return (symbol, new MethodNode
                {
                    MethodSyntax = method,
                    Symbol = symbol,
                });
            }

        }
    }
}
