using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using System.Collections.Generic;
using CodeAnalysisService.GraphService.SyntaxWrappers;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents an event in the code graph.
    /// Stores its Roslyn syntax wrapper, symbol, and outgoing edges
    /// to related nodes (e.g. handlers or containing types).
    /// </summary>
    public class EventNode : INode
    {
        public required EventSyntaxWrapper  EventSyntax { get; set; }
        public NodeType NodeType => NodeType.Event;
        public required IEventSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> OutgoingEdges { get; set; } = new List<EdgeNode>();
    }

}