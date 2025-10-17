using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.GraphService.Helpers
{
    /// <summary>
    /// Collection of specific comparers
    /// </summary>
    public static class Comparers
    {
        public static readonly IEqualityComparer<EdgeNode> Edge =
            new GeneralEqualityComparer<EdgeNode>(
                (x, y) => x?.Type == y?.Type && Equals(x?.Target, y?.Target),
                e => HashCode.Combine(e.Type, e.Target)
            );

        public static readonly IEqualityComparer<PatternRole> PatternRole =
            new GeneralEqualityComparer<PatternRole>(
                (x, y) =>
                    string.Equals(x?.Role, y?.Role, StringComparison.Ordinal) &&
                    SymbolEqualityComparer.Default.Equals(x?.Class.Symbol, y?.Class.Symbol),
                r => HashCode.Combine(
                    StringComparer.Ordinal.GetHashCode(r.Role),
                    SymbolEqualityComparer.Default.GetHashCode(r.Class.Symbol))
            );
    }
}
