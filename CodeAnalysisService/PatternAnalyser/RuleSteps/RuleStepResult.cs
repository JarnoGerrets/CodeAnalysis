using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.RuleSteps
{
    /// <summary>
    /// Result of executing a <see cref="RuleStep"/>.
    /// Immutable value object holding score, pass/fail state, and related roles.
    /// </summary>
    public readonly record struct RuleStepResult(
        int Score,
        bool PassedMustPass,
        IReadOnlyList<PatternRole> RelatedRoles
    )
    {
        public static readonly RuleStepResult Empty =
            new RuleStepResult(0, false, Array.Empty<PatternRole>());
    }
}
