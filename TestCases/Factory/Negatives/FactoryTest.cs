public interface IProduct
{
    string GetName();
}

public class ConcreteProductA : IProduct
{
    public string GetName() => "Product A";
}

public class ConcreteProductB : IProduct
{
    public string GetName() => "Product B";
}

public static class ProductFactory
{
    public static IProduct CreateProductA()
    {
        return new ConcreteProductA();
    }

    public static IProduct CreateProductB()
    {
        return new ConcreteProductB();
    }
}
