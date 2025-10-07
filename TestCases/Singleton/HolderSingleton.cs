public sealed class HolderSingleton
{
    private HolderSingleton() { }

    public static HolderSingleton Instance => Nested.Instance;

    private static class Nested
    {
        internal static readonly HolderSingleton Instance = new HolderSingleton();
    }
}
