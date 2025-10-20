using Microsoft.CodeAnalysis;


namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Base interface for analyzer nodes that represent types to be analyzed for pattern detection
    /// </summary>
    public interface IAnalyzerNode : INode
    {
        new INamedTypeSymbol Symbol { get; }
        ISymbol INode.Symbol => Symbol;
    }
}