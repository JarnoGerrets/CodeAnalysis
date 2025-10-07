public class DCLSingleton
{
    private static DCLSingleton _instance = null!;
    private static readonly object _lock = new();

    private DCLSingleton() { }

    public static DCLSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new DCLSingleton();
                }
            }
            return _instance;
        }
    }
}
