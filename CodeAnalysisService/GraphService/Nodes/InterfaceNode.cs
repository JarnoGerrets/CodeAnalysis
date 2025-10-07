using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents an interface in the code graph.
    /// Stores its Roslyn syntax, symbol, and outgoing edges
    /// to related nodes (e.g. implemented classes).
    /// </summary>
    public class InterfaceNode : IAnalyzerNode
    {
        public object SyncRoot { get; } = new object();
        public required InterfaceDeclarationSyntax InterfaceSyntax { get; set; }
        public NodeType NodeType => NodeType.Interface;
        public required INamedTypeSymbol Symbol { get; set;}
        ISymbol INode.Symbol => Symbol;
        public bool IsAbstract { get; } = false;
        public List<EdgeNode> OutgoingEdges { get; set; } = new List<EdgeNode>();
    }
}
