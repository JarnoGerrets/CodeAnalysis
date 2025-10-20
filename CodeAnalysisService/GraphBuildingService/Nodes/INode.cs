using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Base interface for all nodes in the code graph.
    /// </summary>
    public interface INode
    {
        object SyncRoot { get; }
        NodeType NodeType { get; }
        ISymbol Symbol { get; }
        SyntaxNode Syntax { get; }
        List<EdgeNode> Edges { get; set; }
    }
}