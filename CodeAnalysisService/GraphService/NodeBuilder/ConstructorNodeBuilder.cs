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

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = context.GetRoot(tree);
            foreach (var ctorDecl in root.DescendantNodes().OfType<ConstructorDeclarationSyntax>())
            {
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
}
