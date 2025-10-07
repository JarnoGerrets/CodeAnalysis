public class StaticCtorSingleton
{
    public static readonly StaticCtorSingleton Instance;

    static StaticCtorSingleton()
    {
        Instance = new StaticCtorSingleton();
    }

    private StaticCtorSingleton() { }
}
