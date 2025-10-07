using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace CodeAnalysisService.GraphService.Nodes
{
    /// <summary>
    /// Base interface for analyzer nodes that represent types
    /// (classes, interfaces, etc.) in the code graph.
    /// Provides access to the Roslyn type symbol, abstractness,
    /// and outgoing edges. This interface bunches the Class and Interface nodes together to handle them.
    /// </summary>
    public interface IAnalyzerNode : INode
    {
        new INamedTypeSymbol Symbol { get; }
        ISymbol INode.Symbol => Symbol;
        bool IsAbstract { get; }
    }
}