using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a directed edge in the code graph.
    /// Connects a source node to a target node with a specific relationship type.
    /// </summary>
    public class EdgeNode
    {
        public required INode Target { get; set; }
        public NodeType NodeType => Target.NodeType; 
        public required EdgeType Type { get; set; }
    }
}   