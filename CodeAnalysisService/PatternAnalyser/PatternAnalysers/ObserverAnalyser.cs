using CodeAnalysisService.PatternAnalyser.Checks;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Runs analysis rules for detecting the Observer design pattern.
    /// Uses <see cref="ObserverChecks"/> to:
    /// - Detect subjects holding observer collections.
    /// - Verify notify methods that call observer methods.
    /// - Detect attach/detach methods that register/unregister observers.
    /// Steps are executed via <see cref="BaseAnalyser"/> for consistent rule handling.
    /// </summary>
    public class ObserverAnalyser : BaseAnalyser
    {
        public ObserverAnalyser(IAnalyzerNode node, GraphBuilder graph)
            : base(node, graph) { }

        protected override string PatternName => PatternNames.Observer;

        protected override List<RuleStep> BuildSteps() => new()
        {
            ObserverChecks.HasObserverCollection(),
            ObserverChecks.HasNotifyMethod(Graph),
            ObserverChecks.HasAttachDetachMethods()
        };
    }
}
