using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.GraphBuildingService.Registry;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;

namespace CodeAnalysisService.GraphBuildingService.EdgeBuilder
{
    /// <summary>
    /// Defines a contract for building edges in the graph for a specific <see cref="NodeType"/>.
    /// </summary>

    public interface IEdgeBuilder
    {
        NodeType NodeType { get; }
        IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, SemanticModel model);
    }
}
