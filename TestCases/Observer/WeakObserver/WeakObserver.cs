public class WeakObserverPublisher
{
    private readonly List<WeakReference<IObserver4>> _observers = new();

    public void AddObserver(IObserver4 observer) => _observers.Add(new WeakReference<IObserver4>(observer));
    public void Notify(string data)
    {
        foreach (var weakRef in _observers)
            if (weakRef.TryGetTarget(out var obs))
                obs.Update(data);
    }
}

// Observer interface
public interface IObserver4
{
    void Update(string signal);
}

public class WkObsrvrDisplay : IObserver4
{
    public void Update(string signal) => Console.WriteLine($"WkObsrvrDisplay: {signal}");
}

