using CodeAnalysisService.PatternAnalyser.Checks;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Runs analysis rules for detecting the Strategy design pattern.
    /// Uses <see cref="StrategyChecks"/> to:
    /// - Identify strategy interfaces/abstracts.
    /// - Detect concrete strategy implementations.
    /// - Detect context classes referencing strategies.
    /// - Verify that contexts delegate calls to strategies.
    /// Steps are executed via <see cref="BaseAnalyser"/> for consistent rule handling.
    /// </summary>
    public class StateAnalyser : BaseAnalyser
    {
        public StateAnalyser(IAnalyzerNode node, GraphBuilder graph)
            : base(node, graph) { }

        protected override string PatternName => PatternNames.State;

        protected override List<RuleStep> BuildSteps() => new()
        {
            StateChecks.HasStateInterface(),
            StateChecks.HasConcreteStates(Graph),
            StateChecks.ContextHasStateReference(Graph),
            StateChecks.ContextDelegatesToState(Graph),
            StateChecks.StateMutatesContext(Graph),
            StateChecks.StateHasContextReference(Graph),
            StateChecks.StateMethodNameHint()
        };
    }
}
