namespace CodeAnalysisService.Helpers
{
    /// <summary>
    /// Compares edges to remove duplicates
    /// </summary>
    /// <summary>
    /// Generic structural equality comparer based on custom equality and hash functions.
    /// </summary>
    public class GeneralEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public GeneralEqualityComparer(Func<T?, T?, bool> equals, Func<T, int> getHashCode)
        {
            _equals = equals ?? throw new ArgumentNullException(nameof(equals));
            _getHashCode = getHashCode ?? throw new ArgumentNullException(nameof(getHashCode));
        }

        public bool Equals(T? x, T? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return _equals(x, y);
        }

        public int GetHashCode(T obj) => _getHashCode(obj);
    }
}
