namespace CodeAnalysis.TestCases.Observer.WeatherStation
{
    // Concrete Observer
    public class PhoneDisplay : IObserver
    {
        public void Update(string data)
        {
            Console.WriteLine($"Phone Display updated: {data}");
        }
    }
}