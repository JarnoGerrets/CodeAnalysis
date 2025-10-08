using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a directed edge in the code graph.
    /// </summary>
    public class EdgeNode
    {
        public object SyncRoot { get; } = new object();
        public required INode Target { get; set; }
        public NodeType NodeType => Target.NodeType; 
        public required EdgeType Type { get; set; }
    }
}   