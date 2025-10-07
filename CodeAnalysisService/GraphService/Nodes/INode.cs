using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Base interface for all nodes in the code graph.
    /// Defines common properties for node type, Roslyn symbol,
    /// and outgoing edges to related nodes.
    /// </summary>
    public interface INode
    {
        NodeType NodeType { get; }
        ISymbol Symbol { get; }
        List<EdgeNode> OutgoingEdges { get; set; }
    }
}