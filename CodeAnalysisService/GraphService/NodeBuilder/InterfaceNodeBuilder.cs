using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Builds <see cref="InterfaceNode"/> instances for interface declarations 
    /// in a syntax tree using Roslyn symbols.
    /// </summary>
    public class InterfaceNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Interface;

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model)
        {
            var root = context.GetRoot(tree);
            foreach (var iface in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(iface);
                if (symbol == null) continue;

                yield return (symbol, new InterfaceNode 
                { 
                    InterfaceSyntax = iface, 
                    Symbol = symbol 
                });
            }            
        }
    }
}
