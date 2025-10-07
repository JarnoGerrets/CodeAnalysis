using CodeAnalysisService.PatternAnalyser.Checks;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService;
using System.Collections.Generic;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.PatternAnalysers
{
    /// <summary>
    /// Runs analysis rules for detecting the Singleton design pattern.
    /// Uses <see cref="SingletonChecks"/> to:
    /// - Verify presence of private constructors.
    /// - Detect static fields holding the singleton instance.
    /// - Detect static accessors (method, property, or field) returning the instance.
    /// Uses <see cref="BaseAnalyser"/> for consistent rule execution.
    /// </summary>
    public class SingletonAnalyser : BaseAnalyser
    {
        public SingletonAnalyser(IAnalyzerNode node, GraphBuilder graph)
            : base(node, graph) { }

        protected override string PatternName => PatternNames.Singleton;

        protected override List<RuleStep> BuildSteps() => new()
        {
            SingletonChecks.HasPrivateConstructor(),
            SingletonChecks.HasStaticInstanceField(),
            SingletonChecks.HasStaticAccessor()
        };
    }
}
