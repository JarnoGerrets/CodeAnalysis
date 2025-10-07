using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a constructor in the code graph.
    /// Holds its Roslyn syntax, symbol, and outgoing edges to related nodes
    /// (e.g. created types, calls, or usage relationships).
    /// </summary>
    public class ConstructorNode : INode
    {
        public required ConstructorDeclarationSyntax ConstructorSyntax { get; set; }
        public NodeType NodeType => NodeType.Constructor;
        public required IMethodSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> OutgoingEdges { get; set; } = new();
    }
}
