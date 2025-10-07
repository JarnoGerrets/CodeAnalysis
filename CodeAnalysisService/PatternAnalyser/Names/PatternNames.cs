namespace CodeAnalysisService.PatternAnalyser.Names
{
    /// <summary>
    /// Centralized constants for all supported design pattern names.
    /// Helps avoid magic strings when identifying patterns across analysers and printers.
    /// </summary>
    public static class PatternNames
    {
        public const string Observer = "ObserverPattern";
        public const string Singleton = "SingletonPattern";
        public const string FactoryMethod = "FactoryMethodPattern";
        public const string Strategy = "StrategyPattern";
        public const string Adapter = "AdapterPattern";
        public const string State = "StatePattern";
    }
}
