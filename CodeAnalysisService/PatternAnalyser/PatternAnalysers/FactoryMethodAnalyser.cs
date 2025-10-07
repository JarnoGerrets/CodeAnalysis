using CodeAnalysisService.PatternAnalyser.Checks;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Runs analysis rules for detecting the Factory Method design pattern.
    /// Uses <see cref="FactoryMethodChecks"/> to:
    /// - Detect abstract factory methods returning abstract/interface products.
    /// - Verify that subclasses override factory methods to create products.
    /// - Check that created products implement or derive from the expected return type.
    /// Steps are assembled into a <see cref="PatternRule"/> via <see cref="BaseAnalyser"/>
    /// for consistent rule execution and scoring.
    /// </summary>
    public class FactoryMethodAnalyser : BaseAnalyser
    {
        public FactoryMethodAnalyser(IAnalyzerNode node, GraphBuilder graph)
            : base(node, graph) { }

        protected override string PatternName => PatternNames.FactoryMethod;

        protected override List<RuleStep> BuildSteps() => new()
        {
            FactoryMethodChecks.HasAbstractFactoryMethod(),
            FactoryMethodChecks.SubclassesOverrideFactoryMethod(Graph),
            FactoryMethodChecks.CreatedProductImplementsReturnType(Graph)
        };
    }
}
