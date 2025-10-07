using System;

namespace Demo
{
    public class Publisher
    {
        // Declare an event using EventHandler delegate
        public event EventHandler SomethingHappened = null!;

        public void Trigger()
        {
            // Raise the event
            SomethingHappened?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Subscriber
    {
        public void OnSomething(object? sender, EventArgs e)
        {
            Console.WriteLine("Subscriber received event.");
        }
    }

    public static class Program1
    {
        public static void Main1()
        {
            var pub = new Publisher();
            var sub = new Subscriber();

            // Subscribe to event
            pub.SomethingHappened += sub.OnSomething;

            pub.Trigger();
        }
    }
}
