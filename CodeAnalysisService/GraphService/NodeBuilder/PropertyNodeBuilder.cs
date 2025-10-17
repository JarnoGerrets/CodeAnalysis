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

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = tree.GetRoot();
            foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(prop) is IPropertySymbol symbol)
                {
                    var node = new PropertyNode
                    {
                        PropertySyntax = prop,
                        Symbol = symbol,
                        IsVirtual = symbol.IsVirtual
                    };

                    foreach (var id in prop.DescendantNodes().OfType<IdentifierNameSyntax>())
                    {
                        var refSymbol = model.GetSymbolInfo(id).Symbol;
                        if (refSymbol != null)
                            node.ReferencedSymbols.Add(refSymbol);
                    }

                    yield return (symbol, node);
                }
            }
        }
    }
}
