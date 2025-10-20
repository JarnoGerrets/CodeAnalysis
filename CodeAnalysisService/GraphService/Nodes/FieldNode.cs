using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Represents a field in the code graph.
    /// </summary>
    public class FieldNode : INode
    {
        public object SyncRoot { get; } = new object();
        public FieldDeclarationSyntax DeclarationSyntax { get; set; } = default!;
        public VariableDeclaratorSyntax VariableSyntax { get; set; } = default!;
        public SyntaxNode Syntax => DeclarationSyntax;
        public NodeType NodeType => NodeType.Field;
        public required IFieldSymbol Symbol { get; set; }
        ISymbol INode.Symbol => Symbol;
        public List<EdgeNode> Edges { get; set; } = new List<EdgeNode>();
    }
}