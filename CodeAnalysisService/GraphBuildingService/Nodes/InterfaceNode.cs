using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Represents an interface in the code graph.
    /// </summary>
    public class InterfaceNode : IAnalyzerNode
    {
        public object SyncRoot { get; } = new object();
        public required InterfaceDeclarationSyntax InterfaceSyntax { get; set; }
        public SyntaxNode Syntax => InterfaceSyntax;
        public NodeType NodeType => NodeType.Interface;
        public required INamedTypeSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> Edges { get; set; } = new List<EdgeNode>();
    }
}
