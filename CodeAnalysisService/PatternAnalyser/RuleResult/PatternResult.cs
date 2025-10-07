using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.RuleResult
{
    /// <summary>
    /// Represents the result of running a pattern detection rule.
    /// Contains the overall score, pass/fail status, subject class, and involved roles.
    /// </summary>
    public class PatternResult
    {
        public required PatternRule Rule { get; set; }
        public int Score { get; set; }
        public bool PassedMustPass { get; set; }
        public bool MatchesPattern => PassedMustPass && Score >= Rule.expectedTotalScore / 1.4;
        public required IAnalyzerNode Subject { get; set; }
        public List<PatternRole> InvolvedClasses { get; set; } = new();
    }
}
