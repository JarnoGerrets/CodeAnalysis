using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="ConstructorNode"/> instances for constructor declarations 
    /// in a syntax tree using Roslyn symbols.
    /// </summary>
    public class ConstructorNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Constructor;
        public Type SyntaxType => typeof(ConstructorDeclarationSyntax);

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
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
