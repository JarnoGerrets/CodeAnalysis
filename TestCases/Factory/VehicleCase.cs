public abstract class VehicleFactory
{
    public abstract Vehicle CreateVehicle();
}

public class CarFactory : VehicleFactory
{
    public override Vehicle CreateVehicle() => new Car();
}

public class BikeFactory : VehicleFactory
{
    public override Vehicle CreateVehicle() => new Bike();
}

public interface Vehicle { }
public class Car : Vehicle { }
public class Bike : Vehicle { }
