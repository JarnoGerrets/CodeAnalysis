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

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = context.GetRoot(tree);
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(method) is IMethodSymbol symbol)
                {
                    yield return (symbol, new MethodNode
                    {
                        MethodSyntax = method,
                        Symbol = symbol,
                        IsAbstract = symbol.IsAbstract
                    });
                }
            }
        }
    }
}
