public class FakePublisher
{
    public List<IObserverN1> Observers = new();
    // No Notify method
}


public class WrongNotify
{
    public List<IObserverN2> Observers = new();
    public void Notify() => Console.WriteLine("I don’t call observers!");
}


public class SingleObserver
{
    private IObserverN3 _obs = null!;
    public void Register(IObserverN3 obs) => _obs = obs;
    public void Trigger(string msg) => _obs.Update(msg); // not a collection
}


public class WrongCollection
{
    public List<string> Observers = new(); // not IObserver
    public void NotifyAll() { } // nothing calls Update
}


    public interface IObserverN1
    {
    }
    public interface IObserverN2
    {
    }
    public interface IObserverN3
    {
        void Update(string data);
    }
