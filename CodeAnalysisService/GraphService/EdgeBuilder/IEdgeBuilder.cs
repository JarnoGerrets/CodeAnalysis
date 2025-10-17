using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Helpers;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Defines a contract for building edges in the graph for a specific <see cref="NodeType"/>.
    /// </summary>

    public interface IEdgeBuilder
    {
        NodeType NodeType { get; }
        IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels);
        void WithCallResolver(CallResolver resolver) { }
    }
}
