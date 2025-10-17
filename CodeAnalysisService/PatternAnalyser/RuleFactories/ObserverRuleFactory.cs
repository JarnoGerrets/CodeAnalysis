using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.Queries;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.Names;

namespace CodeAnalysisService.PatternAnalyser.RuleFactories
{
    /// <summary>
    /// Rulefactory to detect Observer pattern.
    /// </summary>
    public static class ObserverRuleFactory
    {
        public static PatternRule Create()
        {
            return new PatternRule("Observer")

                .AddCheck("Has observer collection", 15, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var observerTypes = c.GetCollectionElementTypes(requireInterface: true).ToList();
                    if (!observerTypes.Any()) return PatternRuleResult.Empty;

                    return PatternRuleResult.Success(new[] { new PatternRole(Roles.Subject, c) });
                })

                .AddCheck("Subject notifies observers", 40, (node, graph) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var observerTypes = c.GetCollectionElementTypes(requireInterface: true).ToList();
                    if (!observerTypes.Any()) return PatternRuleResult.Empty;

                    var roles = new List<PatternRole> { new PatternRole(Roles.Subject, c) };
                    var foundAny = false;

                    foreach (var method in c.GetMethods())
                    {
                        foreach (var called in method.CalledMethods())
                        {
                            if (!called.Symbol.ContainingType.IsAssignableToAny(observerTypes))
                                continue;

                            foundAny = true;
                            foreach (var implType in called.GetImplementingTypes())
                            {
                                var observer = graph.Registry.GetNode<IAnalyzerNode>(implType);
                                if (observer != null && !roles.Any(r => r.Role == Roles.Observer && r.Class == observer))
                                    roles.Add(new PatternRole(Roles.Observer, observer));
                            }
                        }
                    }

                    return foundAny && roles.Any(r => r.Role == Roles.Observer)
                        ? PatternRuleResult.Success(roles)
                        : PatternRuleResult.Empty;
                })

                .AddCheck("Has attach/detach methods", 35, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var observerTypes = c.GetCollectionElementTypes(requireInterface: true).ToList();
                    if (!observerTypes.Any()) return PatternRuleResult.Empty;

                    return c.HasMethodWithParameterAssignableTo(observerTypes)
                        ? PatternRuleResult.Success(new[] { new PatternRole(Roles.Subject, c) })
                        : PatternRuleResult.Empty;
                })

                .AddCheck("Has common method names like Update/Notify", 10, (node, _) =>
                {
                    if (node is not ClassNode c) return PatternRuleResult.Empty;

                    var commonNames = new[] { "Update", "Notify", "Changed", "OnChange" };

                    bool found = c.GetMethods()
                        .Any(m => commonNames.Contains(m.Symbol.Name));

                    return found
                        ? PatternRuleResult.Success(new[] { new PatternRole(Roles.Subject, c) })
                        : PatternRuleResult.Empty;
                });
        }
    }
}
