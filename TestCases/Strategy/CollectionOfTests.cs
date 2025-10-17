//
// ✅ Example 1 – Compression Strategy (perfect match)
//
public interface ICompressionAlgo
{
    void Compress(string fileName);
}

public class ZipAlgo : ICompressionAlgo
{
    public void Compress(string fileName) => Console.WriteLine($"ZIP {fileName}");
}

public class RarAlgo : ICompressionAlgo
{
    public void Compress(string fileName) => Console.WriteLine($"RAR {fileName}");
}

public class CompressionTool
{
    private readonly ICompressionAlgo _algo;
    public CompressionTool(ICompressionAlgo algo) => _algo = algo;
    public void Run(string file) => _algo.Compress(file);
}

//
// ✅ Example 2 – Pathfinding Strategy (perfect match)
//
public interface IPathfinding
{
    void FindPath(string start, string end);
}

public class DijkstraPathfinding : IPathfinding
{
    public void FindPath(string start, string end) =>
        Console.WriteLine($"Dijkstra path from {start} to {end}");
}

public class AStarPathfinding : IPathfinding
{
    public void FindPath(string start, string end) =>
        Console.WriteLine($"A* path from {start} to {end}");
}

public class Navigator
{
    private readonly IPathfinding _pathfinder;
    public Navigator(IPathfinding pathfinder) => _pathfinder = pathfinder;
    public void Navigate(string from, string to) => _pathfinder.FindPath(from, to);
}

//
// ⚖️ Example 3 – Notification Strategy (incomplete, should score but not full)
//
public interface INotification
{
    void Send(string message);
}

public class EmailNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"Email: {message}");
}

public class SmsNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"SMS: {message}");
}

public class NotificationManager
{
    private readonly INotification _notification;
    public NotificationManager(INotification notification) => _notification = notification;

    // ❌ never calls _notification.Send()
    public void NotifyAll(string msg) => Console.WriteLine("Notifying users (but not via strategy)");
}

//
// ❌ Example 4 – Shape hierarchy, no context
//
public interface IGeoShape { void Draw(); }

public class CircleShape : IGeoShape
{
    public void Draw() => Console.WriteLine("Circle");
}

public class SquareShape : IGeoShape
{
    public void Draw() => Console.WriteLine("Square");
}
// ❌ no Context using IGeoShape → should NOT match

//
// ❌ Example 5 – Repository injected, but unused
//
public interface IDataStore { void Save(string data); }

public class FileDataStore : IDataStore
{
    public void Save(string data) { }
}

public class DataProcessor
{
    private readonly IDataStore _store;
    public DataProcessor(IDataStore store) => _store = store;

    // ❌ never delegates to _store.Save
    public void Process(string data) => Console.WriteLine("Processing only");
}

//
// ❌ Example 6 – Abstract hierarchy, no strategy interface
//
public abstract class PaymentBase
{
    public abstract void Pay(decimal amount);
}

public class CardPayment : PaymentBase
{
    public override void Pay(decimal amount) => Console.WriteLine($"Card: {amount}");
}

public class CashPayment : PaymentBase
{
    public override void Pay(decimal amount) => Console.WriteLine($"Cash: {amount}");
}
// ❌ no Context holding PaymentBase → not Strategy
