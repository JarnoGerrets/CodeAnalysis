namespace CodeAnalysis.TestCases.Observer.WeatherStation
{
    // Subject
    public class WeatherStation
    {
        private readonly List<IObserver> _observers = new();

        public void AddObserver(IObserver observer) => _observers.Add(observer);

        public void RemoveObserver(IObserver observer) => _observers.Remove(observer);

        public void NotifyObservers(string data)
        {
            foreach (var obs in _observers)
            {
                obs.Update(data);
            }
        }
    }
}




