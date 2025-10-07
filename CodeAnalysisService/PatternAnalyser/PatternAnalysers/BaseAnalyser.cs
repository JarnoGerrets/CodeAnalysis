using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Abstract base class for pattern analysers.
    /// Provides a template method implementation of <see cref="IAnalyser.Analyse"/>:
    /// - Requires derived classes to specify the <see cref="PatternName"/>.
    /// - Requires derived classes to provide the <see cref="BuildSteps"/> that define detection logic.
    /// - Wraps those steps in a <see cref="PatternRule"/> and executes it against the target node.
    /// Ensures consistent boilerplate across all specific pattern analysers (Adapter, Strategy, Observer, etc.).
    /// </summary>
    public abstract class BaseAnalyser : IAnalyser
    {
        protected readonly IAnalyzerNode Node;
        protected readonly GraphBuilder Graph;

        protected BaseAnalyser(IAnalyzerNode node, GraphBuilder graph)
        {
            Node = node;
            Graph = graph;
        }

        protected abstract string PatternName { get; }

        protected abstract List<RuleStep> BuildSteps();

        protected virtual PatternRule BuildRule(List<RuleStep> steps)
        {
            return new PatternRule
            {
                Name = PatternName,
                Steps = steps,
                expectedTotalScore = steps.Count * 100
            };
        }

        public PatternResult Analyse()
        {
            var steps = BuildSteps();
            var rule = BuildRule(steps);
            return rule.Run(Node);
        }

    }
}
