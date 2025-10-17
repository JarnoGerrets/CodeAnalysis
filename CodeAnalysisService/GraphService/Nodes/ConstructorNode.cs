using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a constructor in the code graph.
    /// </summary>
    public class ConstructorNode : INode
    {
        public object SyncRoot { get; } = new object();
        public required ConstructorDeclarationSyntax ConstructorSyntax { get; set; }
        public NodeType NodeType => NodeType.Constructor;
        public required IMethodSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> Edges { get; set; } = new();
    }
}
