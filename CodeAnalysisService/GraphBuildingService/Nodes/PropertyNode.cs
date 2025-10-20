using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Represents a property in the code graph.
    /// </summary>
    public class PropertyNode : INode
    {
        public object SyncRoot { get; } = new object();
        public required PropertyDeclarationSyntax PropertySyntax { get; set; }
        public SyntaxNode Syntax => PropertySyntax;
        public NodeType NodeType => NodeType.Property;
        public required IPropertySymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<ISymbol> ReferencedSymbols { get; set; } = new(); 
        public List<EdgeNode> Edges { get; set; } = new List<EdgeNode>();
    }
}