using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.PatternAnalysers;
using CodeAnalysisService.PatternAnalyser.Printing;
using System.Collections.Concurrent;

namespace CodeAnalysisService.PatternAnalyser
{
    /// <summary>
    /// Orchestrates execution of all pattern analysers against a graph of code elements.
    /// - Runs each registered analyser (Observer, Singleton, Factory Method, Strategy, Adapter)
    ///   on every node in the <see cref="GraphBuilder"/> registry.
    /// - Collects <see cref="PatternResult"/> instances for detected patterns.
    /// - Uses registered <see cref="IPatternPrinter"/>s to print results for each pattern.
    /// Ensures parallel analysis across nodes and centralized result printing.
    /// </summary>
    public class PatternAnalyserManager
    {
        private readonly GraphBuilder _graph;
        private readonly Dictionary<string, IPatternPrinter> _printers;
        private readonly List<Func<IAnalyzerNode, GraphBuilder, BaseAnalyser>> _analyserFactories;

        public PatternAnalyserManager(GraphBuilder graph, IEnumerable<IPatternPrinter>? customPrinters = null)
        {
            _graph = graph;

            _printers = new List<IPatternPrinter>
            {
                new ObserverPatternPrinter(),
                new SingletonPatternPrinter(),
                new FactoryMethodPatternPrinter(),
                new StrategyPatternPrinter(),
                new AdapterPatternPrinter(),
                new StatePatternPrinter()
            }.ToDictionary(p => p.PatternName, p => p);

            if (customPrinters != null)
            {
                foreach (var printer in customPrinters)
                {
                    _printers[printer.PatternName] = printer;
                }
            }

            _analyserFactories = new()
            {
                (node, graph) => new ObserverAnalyser(node, graph),
                (node, graph) => new SingletonAnalyser(node, graph),
                (node, graph) => new FactoryMethodAnalyser(node, graph),
                (node, graph) => new StrategyAnalyser(node, graph),
                (node, graph) => new AdapterAnalyser(node, graph),
                (node, graph) => new StateAnalyser(node, graph) 
            };
        }

        public List<PatternResult> AnalyseAll()
        {
            var results = new ConcurrentBag<PatternResult>();

            Parallel.ForEach(_graph.Registry.GetAll<IAnalyzerNode>(), node =>
            {
                foreach (var factory in _analyserFactories)
                {
                    var analyser = factory(node, _graph);

                    if (analyser is SingletonAnalyser && node.Symbol.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class)
                        continue;

                    results.Add(analyser.Analyse());
                }
            });

            return results.ToList();
        }

        public void PrintResults(List<PatternResult> results)
        {
            int detectedPatterns = 0;

            foreach (var result in results.Where(r => r.MatchesPattern))
            {
                detectedPatterns++;

                if (_printers.TryGetValue(result.Rule.Name, out var printer))
                {
                    printer.Print(result);
                }
                else
                {
                    Console.WriteLine($"No printer registered for pattern {result.Rule.Name}");
                }
            }

            Console.WriteLine($"{detectedPatterns} patterns detected");
        }
    }
}