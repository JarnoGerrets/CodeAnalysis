public class StockMarket
{
    public Dictionary<string, IObserver2> Observers { get; } = new();

    public void AddObserver(string key, IObserver2 observer) => Observers[key] = observer;

    public void NotifyObservers(string stockChange)
    {
        foreach (var observer in Observers.Values)
            observer.Update(stockChange);
    }
}

// Observer interface
public interface IObserver2
{
    void Update(string message);
}

public class TraderDisplay : IObserver2
{
    public void Update(string message) => Console.WriteLine($"TraderDisplay: {message}");
}
