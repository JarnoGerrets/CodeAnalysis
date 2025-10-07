using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="FieldNode"/> instances for field declarations 
    /// in classes using Roslyn symbols.
    /// </summary>
    public class FieldNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Field;

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = context.GetRoot(tree);
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                foreach (var field in classDecl.Members.OfType<FieldDeclarationSyntax>())
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        if (model.GetDeclaredSymbol(variable) is IFieldSymbol fieldSymbol)
                        {
                            yield return (fieldSymbol, new FieldNode
                            {
                                DeclarationSyntax = field,
                                VariableSyntax = variable,
                                Symbol = fieldSymbol
                            });
                        }
                    }
                }
            }
        }
    }
}
