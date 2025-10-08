using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a class in the code graph.
    /// </summary>
    public class ClassNode : IAnalyzerNode
    {
        public object SyncRoot { get; } = new object();
        public required ClassDeclarationSyntax ClassSyntax { get; set; }
        public NodeType NodeType => NodeType.Class;
        public required INamedTypeSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public bool IsAbstract { get; set; } = false;
        public List<EdgeNode> OutgoingEdges { get; set; } = new List<EdgeNode>();
    }
}