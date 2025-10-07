public abstract class CarFactory1
{
    public abstract Car1 CreateCar();
}

public class SedanFactory1 : CarFactory1
{
    public override Car1 CreateCar() => new Car1();
}

public class Car1 { } 
