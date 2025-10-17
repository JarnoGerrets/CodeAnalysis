using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.Printing;
using CodeAnalysisService.PatternAnalyser.RuleFactories;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.PatternAnalyser.Names;
using System.Collections.Concurrent;

namespace CodeAnalysisService.PatternAnalyser
{
    public class PatternAnalyserManager
    {
        private readonly GraphBuilder _graph;
        private readonly List<PatternRule> _rules;
        private readonly Dictionary<string, IPatternPrinter> _printers;

        public PatternAnalyserManager(GraphBuilder graph)
        {
            _graph = graph;

            _rules = new List<PatternRule>
            {
                ObserverRuleFactory.Create(),
                SingletonRuleFactory.Create(),
                FactoryMethodRuleFactory.Create(),
                StrategyOrStateRuleFactory.Create(),
                AdapterRuleFactory.Create(),
            };

            _printers = new Dictionary<string, IPatternPrinter>
            {
                { PatternNames.Observer , new ObserverPatternPrinter() },
                { PatternNames.Singleton , new SingletonPatternPrinter() },
                { PatternNames.FactoryMethod , new FactoryMethodPatternPrinter() },
                { PatternNames.Strategy, new StrategyPatternPrinter() },
                { PatternNames.Adapter , new AdapterPatternPrinter() },
                { PatternNames.State, new StatePatternPrinter() }
            };
        }

        public List<PatternResult> AnalyseAll()
        {
            var results = new ConcurrentBag<PatternResult>();
            var nodes = _graph.Registry.GetAll<IAnalyzerNode>().ToList();

            Parallel.ForEach(nodes, node =>
            {
                foreach (var rule in _rules)
                {
                    var result = rule.Evaluate(node, _graph);
                    if (result.MatchesPattern)
                        results.Add(result);
                }
            });

            return results.ToList();
        }

        public void PrintResults(List<PatternResult> results)
        {
            int detectedPatterns = 0;

            foreach (var result in results.Where(r => r.MatchesPattern))
            {
                Console.WriteLine($"[{result.PatternName}] {string.Join(", ", result.Roles.Select(r => r.Class.Symbol.Name))}");
                detectedPatterns++;

                if (_printers.TryGetValue(result.PatternName, out var printer))
                {
                    printer.Print(result);
                }
                else
                {
                    Console.WriteLine($"No printer registered for pattern {result.PatternName}");
                }
            }

            Console.WriteLine($"{detectedPatterns} patterns detected");
        }
    }
}
