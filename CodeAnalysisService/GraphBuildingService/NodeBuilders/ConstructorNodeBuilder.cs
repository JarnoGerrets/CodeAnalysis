using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphBuildingService.Nodes;

namespace CodeAnalysisService.GraphBuildingService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="ConstructorNode"/> instances for constructor declarations 
    /// in a syntax tree using Roslyn symbols.
    /// </summary>
    public class ConstructorNodeBuilder : INodeBuilder
    {
        public IReadOnlyList<Type> SyntaxTypes =>
        [
            typeof(ConstructorDeclarationSyntax)
        ];

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNode(SyntaxNode node, SemanticModel model)
        {
            if (node is not ConstructorDeclarationSyntax ctorDecl)
                yield break;

            if (model.GetDeclaredSymbol(ctorDecl) is IMethodSymbol symbol)
            {
                yield return (symbol, new ConstructorNode
                {
                    ConstructorSyntax = ctorDecl,
                    Symbol = symbol
                });
            }

        }
    }
}
