using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a property in the code graph.
    /// Captures its Roslyn syntax, symbol, referenced symbols
    /// (e.g. backing fields or accessed members), and outgoing edges.
    /// </summary>
    public class PropertyNode : INode
    {
        public required PropertyDeclarationSyntax PropertySyntax { get; set; }
        public NodeType NodeType => NodeType.Property;
        public required IPropertySymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<ISymbol> ReferencedSymbols { get; set; } = new(); 
        public List<EdgeNode> OutgoingEdges { get; set; } = new List<EdgeNode>();
    }
}