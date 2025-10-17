using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.NodeBuilder
{
    /// <summary>
    /// Defines a builder that creates graph nodes for a specific <see cref="NodeType"/> 
    /// from Roslyn symbols and syntax.
    /// </summary>
    public interface INodeBuilder
    {
        NodeType NodeType { get; }
        IEnumerable<(ISymbol Symbol, INode Node)>  BuildNodes(GraphContext context, SyntaxTree tree, SemanticModel model);
    }
}
