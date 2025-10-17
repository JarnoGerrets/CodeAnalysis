using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.Rules
{
    /// <summary>
    /// Consolidates the output of any PatternRule to hold both the boolean and corresponding roles. 
    /// This way the printer knows which role in the pattern it is.
    /// </summary>
    public class PatternRuleResult
    {
        public static readonly PatternRuleResult Empty = new PatternRuleResult(false, Enumerable.Empty<PatternRole>());
        public bool Passed { get; }
        public IEnumerable<PatternRole> Roles { get; }

        public PatternRuleResult(bool passed, IEnumerable<PatternRole> roles)
        {
            Passed = passed;
            Roles = roles ?? Enumerable.Empty<PatternRole>();
        }

        public static PatternRuleResult Success(IEnumerable<PatternRole> roles) =>
            new PatternRuleResult(true, roles);
    }
}