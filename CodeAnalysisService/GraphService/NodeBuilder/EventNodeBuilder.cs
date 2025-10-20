using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.SyntaxWrappers;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    public class EventNodeBuilder : INodeBuilder
    {
        public NodeType NodeType => NodeType.Event;
        public Type SyntaxType => typeof(EventDeclarationSyntax);

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNodes(GraphContext context, SyntaxNode node, SemanticModel model)
        {
            if (node is EventDeclarationSyntax evtDecl)
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

            if (node is EventFieldDeclarationSyntax evtFieldDecl)
            {
                foreach (var variable in evtFieldDecl.Declaration.Variables)
                {
                    if (model.GetDeclaredSymbol(variable) is IEventSymbol symbol)
                    {
                        yield return (symbol, new EventNode
                        {
                            EventSyntax = new EventSyntaxWrapper(evtFieldDecl),
                            Symbol = symbol
                        });
                    }
                }
            }
        }
    }
}

