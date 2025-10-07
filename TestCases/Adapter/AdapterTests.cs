using System;

// Target
public interface ITarget
{
    void Request();
}

// Adaptee (incompatible interface)
public class Adaptee
{
    public void SpecificRequest() => Console.WriteLine("Adaptee SpecificRequest");
}

// Adapter
public class Adapter : ITarget
{
    private readonly Adaptee _adaptee;
    public Adapter(Adaptee adaptee) => _adaptee = adaptee;

    public void Request()
    {
        // delegate call to adaptee
        _adaptee.SpecificRequest();
    }
}



// ==================== Positive Adapter Cases ====================

// --- Positive Case 1: Classic Adapter ---
public interface IAdapterTarget1
{
    void Request();
}

public class Adaptee1
{
    public void SpecificRequest() => Console.WriteLine("Adaptee1 SpecificRequest");
}

public class ConcreteAdapter1 : IAdapterTarget1
{
    private readonly Adaptee1 _adaptee;
    public ConcreteAdapter1(Adaptee1 adaptee) => _adaptee = adaptee;

    public void Request()
    {
        _adaptee.SpecificRequest();
    }
}



// --- Positive Case 2: Media Player Adapter ---
public interface IAdapterMediaPlayer
{
    void Play(string file);
}

public class AdapteeMediaLibrary
{
    public void PlayFile(string path) => Console.WriteLine($"Playing file: {path}");
}

public class MediaPlayerAdapter : IAdapterMediaPlayer
{
    private readonly AdapteeMediaLibrary _library;
    public MediaPlayerAdapter(AdapteeMediaLibrary library) => _library = library;

    public void Play(string file)
    {
        _library.PlayFile(file);
    }
}



// --- Positive Case 3: Notification Adapter ---
public interface IAdapterNotifier
{
    void Notify(string msg);
}

public class AdapteeEmailService
{
    public void SendEmail(string text) => Console.WriteLine($"Email sent: {text}");
}

public class EmailNotifierAdapter : IAdapterNotifier
{
    private readonly AdapteeEmailService _email;
    public EmailNotifierAdapter(AdapteeEmailService email) => _email = email;

    public void Notify(string msg)
    {
        _email.SendEmail(msg);
    }
}



// ==================== Negative (Non-Adapter) Cases ====================

// --- Negative Case 1: Plain class, no adaptee ---
public class PlainWorker
{
    public void DoWork() => Console.WriteLine("Doing work directly");
}



// --- Negative Case 2: Abstract base + derived (inheritance only) ---
public abstract class AbstractWorker
{
    public abstract void DoJob();
}

public class ConcreteWorker : AbstractWorker
{
    public override void DoJob() => Console.WriteLine("Job done");
}



// --- Negative Case 3: Utility static class ---
public static class MathUtils
{
    public static int Add(int a, int b) => a + b;
}



// --- Negative Case 4: Simple interface + implementation (not adapting anything) ---
public interface IWorker
{
    void Work();
}

public class SimpleWorker : IWorker
{
    public void Work() => Console.WriteLine("Simple work done");
}
