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

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = context.GetRoot(tree);
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol symbol)
                {
                    yield return (symbol, new ClassNode 
                    { 
                        ClassSyntax = classDecl, 
                        Symbol = symbol, 
                        IsAbstract = symbol.IsAbstract  
                    });
                }
            }
        }
    }
}
