public abstract class Animal1
{
    public abstract void Speak();
}

public class Dog1 : Animal1
{
    public override void Speak() => Console.WriteLine("Woof");
}
