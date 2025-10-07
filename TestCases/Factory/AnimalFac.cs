public static class AnimalFactory
{
    public static Animal CreateDog() => new Dog();
    public static Animal CreateCat() => new Cat();
}

public interface Animal { }
public class Dog : Animal { }
public class Cat : Animal { }
