public class MySingleton
{
    private static MySingleton _instance = null!;
    private MySingleton() {}
    public static MySingleton Instance => _instance ??= new MySingleton();
}
