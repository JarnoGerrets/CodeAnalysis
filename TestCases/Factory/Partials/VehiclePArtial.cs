public abstract class TransportFactory
{
    public abstract TransportMode? CreatePlane();
    public abstract TransportMode? CreateBus();
}

public class HybridTransportFactory : TransportFactory
{
    public override TransportMode? CreatePlane() => new Plane();
    public override TransportMode? CreateBus() => null;
}

public interface TransportMode { }
public class Plane : TransportMode { }
public class Bus : TransportMode { }
