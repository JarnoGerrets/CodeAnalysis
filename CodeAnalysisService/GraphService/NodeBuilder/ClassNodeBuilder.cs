using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="ClassNode"/> instances for class declarations 
    /// in a syntax tree using Roslyn symbols.
    /// </summary>
    public class ClassNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Class;
        public Type SyntaxType => typeof(ClassDeclarationSyntax);

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is not ClassDeclarationSyntax classDecl)
                yield break;

            if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol symbol)
            {
                yield return (symbol, new ClassNode
                {
                    ClassSyntax = classDecl,
                    Symbol = symbol,
                });
            }

        }
    }
}
