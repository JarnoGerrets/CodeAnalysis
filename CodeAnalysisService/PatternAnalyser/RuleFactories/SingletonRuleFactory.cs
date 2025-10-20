using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.Queries;
using CodeAnalysisService.PatternAnalyser.Rules;

namespace CodeAnalysisService.PatternAnalyser.RuleFactories
{
    public static class SingletonRuleFactory
    {
        /// <summary>
        /// Rulefactory to detect singleton pattern.
        /// </summary>
        public static PatternRule Create()
        {
            return new PatternRule("Singleton")

                .AddCheck("Has private constructor", 50, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    return c.HasPrivateConstructor()
                        ? PatternRuleResult.Success(new[] { new PatternRole("Singleton", c) })
                        : PatternRuleResult.Empty;
                })

                .AddCheck("Has static instance field", 25, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var check =
                        c.HasStaticFieldOfOwnType() ||
                        c.HasLazyStaticFieldOfOwnType() ||
                        c.HasNestedStaticHolderOfOwnType() ||
                        c.HasGenericStaticFieldOfOwnType();

                    return check
                        ? PatternRuleResult.Success(new[] { new PatternRole("Singleton", c) })
                        : PatternRuleResult.Empty;
                })

                .AddCheck("Has static accessor", 25, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var check =
                        c.HasStaticPropertyOfOwnType() ||
                        c.HasStaticMethodOfOwnType() ||
                        c.HasStaticFieldAccessorOfOwnType();

                    return check
                        ? PatternRuleResult.Success(new[] { new PatternRole("Singleton", c) })
                        : PatternRuleResult.Empty;
                });
        }
    }
}
