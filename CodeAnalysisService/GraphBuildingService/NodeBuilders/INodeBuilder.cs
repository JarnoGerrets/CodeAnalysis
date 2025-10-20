using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.Nodes;

namespace CodeAnalysisService.GraphBuildingService.NodeBuilder
{
    /// <summary>
    /// Defines a builder that creates graph nodes for a specific <see cref="NodeType"/> 
    /// from Roslyn symbols and syntax.
    /// </summary>
    public interface INodeBuilder
    {
        IReadOnlyList<Type> SyntaxTypes { get; }
        IEnumerable<(ISymbol Symbol, INode Node)>  BuildNode(SyntaxNode node, SemanticModel model);
    }
}
