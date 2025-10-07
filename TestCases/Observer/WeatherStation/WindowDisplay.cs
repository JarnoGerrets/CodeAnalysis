using CodeAnalysis.TestCases.Observer.WeatherStation;

namespace CodeAnalysis.TestCases.Observer.WeatherStation
{
    // Concrete Observer
    public class WindowDisplay : IObserver
    {
        public void Update(string data)
        {
            Console.WriteLine($"Window Display updated: {data}");
        }
    }
}