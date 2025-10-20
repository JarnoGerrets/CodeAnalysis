using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="PropertyNode"/> instances for property declarations
    /// using Roslyn symbols and captures referenced symbols inside property bodies.
    /// </summary>
    public class PropertyNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Property;
        public Type SyntaxType => typeof(PropertyDeclarationSyntax);
        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is not PropertyDeclarationSyntax prop)
                yield break;

            if (model.GetDeclaredSymbol(prop) is not IPropertySymbol symbol)
                yield break;

            var nodeObj = new PropertyNode
            {
                PropertySyntax = prop,
                Symbol = symbol,
            };

            if (prop.AccessorList != null)
            {
                foreach (var id in prop.AccessorList.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    var refSymbol = model.GetSymbolInfo(id).Symbol;
                    if (refSymbol != null)
                        nodeObj.ReferencedSymbols.Add(refSymbol);
                }
            }

            if (prop.ExpressionBody != null)
            {
                foreach (var id in prop.ExpressionBody.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    var refSymbol = model.GetSymbolInfo(id).Symbol;
                    if (refSymbol != null)
                        nodeObj.ReferencedSymbols.Add(refSymbol);
                }
            }

            yield return (symbol, nodeObj);
        }

    }
}
