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

        public Type SyntaxType => typeof(FieldDeclarationSyntax);

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is not FieldDeclarationSyntax field)
                yield break;

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

