public class GenericSingleton<T> where T : new()
{
    private static readonly T _instance = new();

    private GenericSingleton() { }

    public static T Instance => _instance;
}
