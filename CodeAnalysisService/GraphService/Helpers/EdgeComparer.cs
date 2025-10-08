using System.Collections.Generic;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.Helpers
{
    /// <summary>
    /// Compares edges to remove duplicates
    /// </summary>
    public class EdgeComparer : IEqualityComparer<EdgeNode>
    {
        public bool Equals(EdgeNode? x, EdgeNode? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Type == y.Type && Equals(x.Target, y.Target);
        }

        public int GetHashCode(EdgeNode obj) => HashCode.Combine(obj.Type, obj.Target);
    }
}
