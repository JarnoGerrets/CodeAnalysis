using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.PatternAnalyser.RuleSteps
{
    /// <summary>
    /// Represents a single step in detecting a design pattern.
    /// Each step has a description, a check function, and whether it is required.
    /// </summary>
    public class RuleStep
    {
        public required string Description { get; set; }   
        public required Func<IAnalyzerNode, RuleStepResult> Check { get; set; }
        public bool MustPass { get; set; }
    }
}