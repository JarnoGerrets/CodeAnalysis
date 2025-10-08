using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Base interface for analyzer nodes that represent types to be analyzed for pattern detection
    /// </summary>
    public interface IAnalyzerNode : INode
    {
        new INamedTypeSymbol Symbol { get; }
        ISymbol INode.Symbol => Symbol;
        bool IsAbstract { get; }
    }
}