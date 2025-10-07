using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using System.Collections.Generic;
using CodeAnalysisService.GraphService.SyntaxWrappers;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
   public class EventNodeBuilder : INodeBuilder
{
    public NodeType NodeType => NodeType.Event;

    public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(
        GraphContext context,
        SyntaxTree tree,
        SemanticModel model)
    {
        var root = context.GetRoot(tree);

        foreach (var evtDecl in root.DescendantNodes().OfType<EventDeclarationSyntax>())
        {
            if (model.GetDeclaredSymbol(evtDecl) is IEventSymbol symbol)
            {
                yield return (symbol, new EventNode
                {
                    EventSyntax = new EventSyntaxWrapper(evtDecl),
                    Symbol = symbol
                });
            }
        }

        foreach (var field in root.DescendantNodes().OfType<EventFieldDeclarationSyntax>())
        {
            foreach (var variable in field.Declaration.Variables)
            {
                if (model.GetDeclaredSymbol(variable) is IEventSymbol symbol)
                {
                    yield return (symbol, new EventNode
                    {
                        EventSyntax = new EventSyntaxWrapper(field),
                        Symbol = symbol
                    });
                }
            }
        }
    }
}

}
