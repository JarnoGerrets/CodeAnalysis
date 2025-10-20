using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphBuildingService.Nodes;


using CodeAnalysisService.GraphBuildingService.SyntaxWrappers;

namespace CodeAnalysisService.GraphBuildingService.NodeBuilder
{
    public class EventNodeBuilder : INodeBuilder
    {
        public IReadOnlyList<Type> SyntaxTypes =>
        [
            typeof(EventDeclarationSyntax),
            typeof(EventFieldDeclarationSyntax)
        ];

        public IEnumerable<(ISymbol Symbol, INode Node)> BuildNode(SyntaxNode node, SemanticModel model)
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

