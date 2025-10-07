public abstract class ShapeFactory
{
    public abstract Shape CreateShape();
}

public class CircleFactory : ShapeFactory
{
    public override Shape CreateShape() => new Circle();
}

public class SquareFactory : ShapeFactory
{
    public override Shape CreateShape() => new Square();
}

public interface Shape { }
public class Circle : Shape { }
public class Square : Shape { }
