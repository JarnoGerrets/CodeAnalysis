public abstract class BaseFactory
{
    public abstract Animal2 CreateAnimal();
}

public abstract class MammalFactory : BaseFactory { }

public class DogFactory : MammalFactory
{
    public override Animal2 CreateAnimal() => new Dog2();
}

public interface Animal2 { }
public class Dog2 : Animal2 { }
