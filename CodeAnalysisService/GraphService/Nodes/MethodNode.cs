using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a method in the code graph.
    /// Stores its Roslyn syntax, symbol, return type,
    /// abstractness, and outgoing edges to related nodes
    /// (e.g. calls, usage, or return types).
    /// </summary>
    public class MethodNode: INode
    {
        public object SyncRoot { get; } = new object();
        public required MethodDeclarationSyntax MethodSyntax { get; set; }
        public NodeType NodeType => NodeType.Method;
        public required IMethodSymbol Symbol { get; set; }
        public ITypeSymbol ReturnType => Symbol.ReturnType;
        ISymbol INode.Symbol => Symbol;
        public bool IsAbstract { get; set; } = false;
        public List<EdgeNode> OutgoingEdges { get; set; } = new List<EdgeNode>();
    }
}