// Component
public interface IComponent
{
    void Operation();
}

// Leaf
public class LeafA : IComponent
{
    public void Operation() => Console.WriteLine("LeafA Operation");
}

public class LeafB : IComponent
{
    public void Operation() => Console.WriteLine("LeafB Operation");
}

// Composite
public class CompositeNode : IComponent
{
    private readonly List<IComponent> _children = new();

    public void Add(IComponent component) => _children.Add(component);
    public void Remove(IComponent component) => _children.Remove(component);

    public void Operation()
    {
        Console.WriteLine("CompositeNode Operation start");
        foreach (var child in _children)
            child.Operation();
        Console.WriteLine("CompositeNode Operation end");
    }
}


// ==================== Positive Composite Cases ====================

// --- Positive Case 1: Classic Composite with two leaves ---
public class ClassicCompositeExample
{
    public static void Run()
    {
        var root = new CompositeNode();
        root.Add(new LeafA());
        root.Add(new LeafB());
        root.Operation();
    }
}

// --- Positive Case 2: Nested Composite ---
public class NestedCompositeExample
{
    public static void Run()
    {
        var root = new CompositeNode();
        var branch = new CompositeNode();
        branch.Add(new LeafA());
        branch.Add(new LeafB());
        root.Add(branch);
        root.Add(new LeafA());
        root.Operation();
    }
}

// --- Positive Case 3: FileSystem-like Composite ---
public interface IFileSystemEntry
{
    void Display(string indent = "");
}

public class FileEntry : IFileSystemEntry
{
    private readonly string _name;
    public FileEntry(string name) => _name = name;
    public void Display(string indent = "") => Console.WriteLine($"{indent}- {_name}");
}

public class DirectoryEntry : IFileSystemEntry
{
    private readonly string _name;
    private readonly List<IFileSystemEntry> _entries = new();

    public DirectoryEntry(string name) => _name = name;
    public void Add(IFileSystemEntry entry) => _entries.Add(entry);

    public void Display(string indent = "")
    {
        Console.WriteLine($"{indent}+ {_name}");
        foreach (var e in _entries)
            e.Display(indent + "  ");
    }
}

public class FileSystemCompositeExample
{
    public static void Run()
    {
        var root = new DirectoryEntry("root");
        var bin = new DirectoryEntry("bin");
        bin.Add(new FileEntry("bash"));
        bin.Add(new FileEntry("ls"));
        var etc = new DirectoryEntry("etc");
        etc.Add(new FileEntry("hosts"));
        root.Add(bin);
        root.Add(etc);
        root.Display();
    }
}


// ==================== Negative (Non-Composite) Cases ====================

// --- Negative Case 1: No child storage, just a leaf-like class ---
public class SoloLeaf : IComponent
{
    public void Operation() => Console.WriteLine("SoloLeaf only, no children");
}

// --- Negative Case 2: Collection of items but not through common component interface ---
public class NotCompositeCollection
{
    private readonly List<string> _items = new();
    public void Add(string s) => _items.Add(s);
    public void PrintAll()
    {
        foreach (var s in _items) Console.WriteLine(s);
    }
}

// --- Negative Case 3: Inheritance hierarchy without aggregation ---
public abstract class Animal
{
    public abstract void Speak();
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Woof");
}

public class Cat : Animal
{
    public override void Speak() => Console.WriteLine("Meow");
}
// No composite node aggregating Animals ? should NOT match
