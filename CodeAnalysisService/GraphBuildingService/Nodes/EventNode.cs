using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.SyntaxWrappers;

namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Represents an event in the code graph.
    /// </summary>
    public class EventNode : INode
    {
        public object SyncRoot { get; } = new object();
        public required EventSyntaxWrapper EventSyntax { get; set; }
        public SyntaxNode Syntax => EventSyntax.Syntax;
        public NodeType NodeType => NodeType.Event;
        public required IEventSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> Edges { get; set; } = new List<EdgeNode>();
    }

}