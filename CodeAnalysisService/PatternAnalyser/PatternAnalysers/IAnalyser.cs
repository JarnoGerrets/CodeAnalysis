using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.RuleResult;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    public interface IAnalyser
    {
        PatternResult Analyse();
    }
}