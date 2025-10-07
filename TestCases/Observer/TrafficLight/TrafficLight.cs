using System;
using System.Collections.Generic;

public class TrafficLight
{
    private List<IObserver1> _observers = new List<IObserver1>();

    public IEnumerable<IObserver1> Observers => _observers;

    public void Attach(IObserver1 observer) => _observers.Add(observer);

    public void NotifyChange(string signal)
    {
        foreach (var obs in Observers)
            obs.Update(signal);
    }
}
// Observer interface
public interface IObserver1
{
    void Update(string signal);
}

public class CarDisplay : IObserver1
{
    public void Update(string signal) => Console.WriteLine($"CarDisplay: {signal}");
}
