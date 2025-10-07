using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeAnalysisService.GraphService.Helpers;

namespace CodeAnalysisService.GraphService.EdgeBuilder
{
    /// <summary>
    /// Defines a contract for building edges in the graph for a specific <see cref="NodeType"/>.
    /// Each implementation inspects a given <see cref="INode"/> and produces
    /// outgoing <see cref="EdgeNode"/> connections based on Roslyn symbols and syntax.
    /// </summary>

    public interface IEdgeBuilder
    {
        NodeType NodeType { get; }
        IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, Compilation compilation, Dictionary<SyntaxTree, SemanticModel> semanticModels);
        void WithCallResolver(CallResolver resolver) { }
    }
}
