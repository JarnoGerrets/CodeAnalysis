using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Base interface for all nodes in the code graph.
    /// </summary>
    public interface INode
    {
        object SyncRoot { get; }
        NodeType NodeType { get; }
        ISymbol Symbol { get; }
        List<EdgeNode> OutgoingEdges { get; set; }
    }
}