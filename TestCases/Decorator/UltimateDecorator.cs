namespace CodeAnalysis.TestCases.Decorator;

using System;

// Component interface (Component role)
public interface IUltimateDecoratorComponent
{
    void Operation();
    int Cost();
}

// ConcreteComponent
public class UltimateConcreteComponent : IUltimateDecoratorComponent
{
    public void Operation()
    {
        Console.WriteLine("UltimateConcreteComponent Operation");
    }

    public int Cost()
    {
        return 10;
    }
}

// Abstract Decorator (implements Component + stores Component)
public abstract class UltimateDecorator : IUltimateDecoratorComponent
{
    protected readonly IUltimateDecoratorComponent _component;

    protected UltimateDecorator(IUltimateDecoratorComponent component)
    {
        _component = component;
    }

    public virtual void Operation()
    {
        // Delegation to wrapped component
        _component.Operation();
    }

    public virtual int Cost()
    {
        // Delegation to wrapped component
        return _component.Cost();
    }
}

// ConcreteDecorator A
public class UltimateConcreteDecoratorA : UltimateDecorator
{
    public UltimateConcreteDecoratorA(IUltimateDecoratorComponent component)
        : base(component)
    {
    }

    public override void Operation()
    {
        Console.WriteLine("Decorator A before");
        base.Operation(); // delegation
        Console.WriteLine("Decorator A after");
    }

    public override int Cost()
    {
        return base.Cost() + 5;
    }
}

// ConcreteDecorator B
public class UltimateConcreteDecoratorB : UltimateDecorator
{
    public UltimateConcreteDecoratorB(IUltimateDecoratorComponent component)
        : base(component)
    {
    }

    public override void Operation()
    {
        Console.WriteLine("Decorator B before");
        base.Operation(); // delegation
        Console.WriteLine("Decorator B after");
    }

    public override int Cost()
    {
        return base.Cost() + 20;
    }
}

// Demo harness (ensures everything is connected)
public static class UltimateDecoratorDemo
{
    public static void Run()
    {
        IUltimateDecoratorComponent component =
            new UltimateConcreteComponent();

        component = new UltimateConcreteDecoratorA(component);
        component = new UltimateConcreteDecoratorB(component);

        component.Operation();
        Console.WriteLine($"Total Cost: {component.Cost()}");
    }
}
