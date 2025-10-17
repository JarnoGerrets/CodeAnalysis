public class NewsPublisher
{
    public HashSet<IObserver3> Observers { get; } = new HashSet<IObserver3>();

    public void Subscribe(IObserver3 observer) => Observers.Add(observer);
    public void NotifyAll(string news)
    {
        foreach (var obs in Observers)
            obs.Update(news);
    }
}
public interface IObserver3
{
    void Update(string message);
}

public class AppDisplay : IObserver3
{
    public void Update(string message) => Console.WriteLine($"AppDisplay: {message}");
}
