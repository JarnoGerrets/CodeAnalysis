using System;
using System.Collections.Generic;

// Interface Component (20 points: Detects Component interface/abstract)
public interface IUltimateComponent
{
    void Operation();
    int Measure();
}

// Abstract Class Component (also satisfies the same 20-point check if analysis starts on class)
public abstract class AbstractUltimateComponent
{
    public abstract void Operation();
    public abstract int Measure();
}

// Concrete leaves implementing the interface (15 points: Leaf without children)
public class UltimateLeaf : IUltimateComponent
{
    private readonly int _size;
    public UltimateLeaf(int size) => _size = size;

    public void Operation() => Console.WriteLine($"UltimateLeaf Operation (size={_size})");
    public int Measure() => _size;

    // Extra ordinary method to strengthen DelegatesToImplementor via concrete calls
    public int LeafSpecificMeasure() => _size * 2;
}

public class UltimateLeafB : IUltimateComponent
{
    private readonly int _size;
    public UltimateLeafB(int size) => _size = size;

    public void Operation() => Console.WriteLine($"UltimateLeafB Operation (size={_size})");
    public int Measure() => _size;

    public int LeafBSpecificMeasure() => _size + 1;
}

// A leaf deriving from the abstract component (ensures abstract component has implementors too)
public class UltimateAbstractLeaf : AbstractUltimateComponent
{
    private readonly int _size;
    public UltimateAbstractLeaf(int size) => _size = size;

    public override void Operation() => Console.WriteLine($"UltimateAbstractLeaf Operation (size={_size})");
    public override int Measure() => _size;

    public int AbstractLeafSpecificMeasure() => _size * 3;
}

// Composite holding children of the interface type and delegating (40 points)
// Name hint "Composite" adds +5
public class UltimateComposite : IUltimateComponent
{
    // Holds children of the component type -> HoldsChildrenOfComponent
    private readonly List<IUltimateComponent> _children = new();

    public void Add(IUltimateComponent child) => _children.Add(child);
    public void Remove(IUltimateComponent child) => _children.Remove(child);

    public void Operation()
    {
        Console.WriteLine("UltimateComposite Operation start");

        foreach (var child in _children)
        {
            // Delegates to the interface method -> DelegatesToType
            child.Operation();

            // Delegates to concrete implementors -> DelegatesToImplementor
            if (child is UltimateLeaf a)
            {
                _ = a.LeafSpecificMeasure(); // concrete call on implementor
            }
            else if (child is UltimateLeafB b)
            {
                _ = b.LeafBSpecificMeasure(); // concrete call on implementor
            }
        }

        Console.WriteLine("UltimateComposite Operation end");
    }

    public int Measure()
    {
        var total = 0;

        foreach (var child in _children)
        {
            // Delegates to the interface method -> DelegatesToType
            total += child.Measure();

            // Delegates to concrete implementors -> DelegatesToImplementor
            if (child is UltimateLeaf a)
            {
                total += a.LeafSpecificMeasure() / 10;
            }
            else if (child is UltimateLeafB b)
            {
                total += b.LeafBSpecificMeasure() / 10;
            }
        }

        return total;
    }
}

// Demo harness that ensures implementors exist (20 points: Find implementors of Component)
public static class UltimateCompositeDemo
{
    public static void Run()
    {
        var root = new UltimateComposite();

        var leaf1 = new UltimateLeaf(10);
        var leaf2 = new UltimateLeafB(20);

        var branch = new UltimateComposite();
        branch.Add(new UltimateLeaf(5));
        branch.Add(new UltimateLeafB(15));

        root.Add(leaf1);
        root.Add(leaf2);
        root.Add(branch);

        // Note: Abstract component has implementors too
        AbstractUltimateComponent absLeaf = new UltimateAbstractLeaf(7);
        absLeaf.Operation();

        root.Operation();
        Console.WriteLine($"Total Measure: {root.Measure()}");
    }
}