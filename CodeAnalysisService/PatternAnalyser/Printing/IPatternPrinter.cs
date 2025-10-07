using CodeAnalysisService.PatternAnalyser.RuleResult;

namespace CodeAnalysisService.PatternAnalyser.Printing
{
    public interface IPatternPrinter
    {
        string PatternName { get; }
        void Print(PatternResult result);
    }
}
