using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphBuildingService.Nodes;

namespace CodeAnalysisService.GraphBuildingService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="MethodNode"/> instances for method declarations 
    /// in classes using Roslyn symbols.
    /// </summary>
    public class MethodNodeBuilder : INodeBuilder
    {
        public IReadOnlyList<Type> SyntaxTypes =>
        [
            typeof(MethodDeclarationSyntax)
        ];

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNode(SyntaxNode node, SemanticModel model)
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
