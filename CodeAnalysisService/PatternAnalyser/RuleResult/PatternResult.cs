using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using System.Collections.Generic;

namespace CodeAnalysisService.PatternAnalyser.RuleResult
{
    /// <summary>
    /// Outcome of a complete pattern check
    /// </summary>
    public class PatternResult
    {
        public string PatternName { get; }
        public int Score { get; }
        public string Classification { get; }
        public IReadOnlyList<CheckResult> Checks { get; }
        public IReadOnlyList<PatternRole> Roles { get; }

        public bool MatchesPattern => Score >= 50;

        public PatternResult(string name, int score, string classification,
            IReadOnlyList<CheckResult> checks, IReadOnlyList<PatternRole> roles)
        {
            PatternName = name;
            Score = score;
            Classification = classification;
            Checks = checks;
            Roles = roles;
        }

        public static PatternResult None(string name) =>
            new PatternResult(name, 0, "No match", new List<CheckResult>(), new List<PatternRole>());
    }
}


