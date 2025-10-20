using CodeAnalysisService.Helpers;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphBuildingService;
using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.RuleResult;

namespace CodeAnalysisService.PatternAnalyser.Rules
{
    /// <summary>
    /// Represents a specific rule in the pattern detection. Any RuleFactory uses a multitude of Patternrules to validate a pattern.
    /// </summary>
    public class PatternRule
    {
        public string Name { get; }
        private readonly List<PatternCheck> _checks = new();

        private Func<PatternResult, GraphBuilder, PatternResult>? _postProcessor;

        public PatternRule(string name) => Name = name;

        public PatternRule AddCheck(
            string description,
            int weight,
            Func<IAnalyzerNode, GraphBuilder, PatternRuleResult> predicate)
        {
            _checks.Add(new PatternCheck(description, weight, predicate));
            return this;
        }

        public PatternRule WithPostProcessor(Func<PatternResult, GraphBuilder, PatternResult> postProcessor)
        {
            _postProcessor = postProcessor;
            return this;
        }

        public PatternResult Evaluate(IAnalyzerNode node, GraphBuilder graph)
        {
            var checks = new List<CheckResult>();
            var roles  = new List<PatternRole>();

            int totalWeight  = _checks.Sum(c => c.Weight);
            int gainedWeight = 0;

            foreach (var check in _checks)
            {
                var result = check.Predicate(node, graph);

                checks.Add(new CheckResult(check.Description, check.Weight, result.Passed));

                if (result.Passed)
                {
                    gainedWeight += check.Weight;
                    roles.AddRange(result.Roles);
                }
            }

            int score = totalWeight > 0 ? (int)((double)gainedWeight / totalWeight * 100) : 0;
            
            // Anything below 51 is not too low to confidentially match a pattern, so it is discarded.
            if (score < 51)
                return PatternResult.None(Name);

            string classification = score switch
            {
                >= 80 => "Strong match",
                >= 71 => "Almost",
                _     => "Attempted but weak"
            };

            var distinctRoles = roles.Distinct(Comparers.PatternRole).ToList();
            var baseResult = new PatternResult(Name, score, classification, checks, distinctRoles);

            return _postProcessor is null ? baseResult : _postProcessor(baseResult, graph);
        }

        private record PatternCheck(
            string Description,
            int Weight,
            Func<IAnalyzerNode, GraphBuilder, PatternRuleResult> Predicate);
    }

    public record CheckResult(string Description, int Weight, bool Passed);

}
