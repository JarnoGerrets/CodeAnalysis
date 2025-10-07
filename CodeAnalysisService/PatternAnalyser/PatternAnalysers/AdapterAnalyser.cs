using CodeAnalysisService.PatternAnalyser.Checks;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Runs analysis rules for detecting the Adapter design pattern on a given node.
    /// Uses <see cref="AdapterChecks"/> to:
    /// - Identify the target interface/abstract class.
    /// - Detect adapter candidates implementing or deriving from the target.
    /// - Verify that adapters delegate target methods to adaptees.
    /// Wraps these steps into a <see cref="PatternRule"/> and executes it against
    /// the provided node using <see cref="PatternRule.Run(IAnalyzerNode)"/>.
    /// </summary>
    public class AdapterAnalyser : BaseAnalyser
    {
        public AdapterAnalyser(IAnalyzerNode node, GraphBuilder graph)
            : base(node, graph) { }

        protected override string PatternName => PatternNames.Adapter;

        protected override List<RuleStep> BuildSteps() => new()
        {
            AdapterChecks.HasTarget(),
            AdapterChecks.HasAdapterCandidates(Graph),
            AdapterChecks.AdapterDelegatesToAdaptee(Graph)
        };
    }
}
