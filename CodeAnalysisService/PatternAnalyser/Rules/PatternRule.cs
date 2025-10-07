using System.Linq;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.Rules
{
    /// <summary>
    /// Represents a rule for detecting a design pattern.
    /// A rule is composed of multiple <see cref="RuleStep"/> checks,
    /// and evaluates whether a node fulfills the pattern structure.
    /// </summary>
    public class PatternRule
    {
        public string Name { get; set; } = string.Empty;
        public List<RuleStep> Steps { get; set; } = new();
        public int expectedTotalScore { get; set; }

        public PatternResult Run(IAnalyzerNode node)
        {
            var evals = Steps.Select(step => (step, res: step.Check(node))).ToList();

            int totalScore = evals.Sum(e => e.res.Score);

            bool passedMustPass = evals
                .Where(e => e.step.MustPass)
                .All(e => e.res.PassedMustPass);

            var involvedRoles = evals
                .SelectMany(e => e.res.RelatedRoles ?? Enumerable.Empty<PatternRole>())
                .Append(new PatternRole ( "Subject", node ))
                .GroupBy(r => r.Class) 
                .Select(g => g.First())
                .ToList();

            return new PatternResult
            {
                Rule = this,
                Score = totalScore,
                PassedMustPass = passedMustPass,
                Subject = node,
                InvolvedClasses = involvedRoles
            };
        }
    }
}
