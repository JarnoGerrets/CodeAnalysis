using CodeAnalysisService.PatternAnalyser.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;

namespace CodeAnalysisService.PatternAnalyser.Candidates
{
    public class PatternCandidate
    {
        public PatternKind Kind { get; }
        public RuleStepResult Result { get; }

        public PatternCandidate(PatternKind kind, RuleStepResult result)
        {
            Kind = kind;
            Result = result;
        }
    }
}
