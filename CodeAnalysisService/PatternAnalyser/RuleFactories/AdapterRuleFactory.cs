using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.PatternAnalyser.Queries;
using CodeAnalysisService.Names;

namespace CodeAnalysisService.PatternAnalyser.RuleFactories
{

    /// <summary>
    /// Rulefactory to detect Adapter pattern.
    /// </summary>
    public static class AdapterRuleFactory
    {

        public static PatternRule Create()
        {
            return new PatternRule("Adapter")

                .AddCheck("Detects Target interface/abstract", 15, (node, _) =>
                {
                    if (node.Symbol is not INamedTypeSymbol t ||
                        (t.TypeKind != TypeKind.Interface && !node.IsAbstract))
                        return PatternRuleResult.Empty;

                    return PatternRuleResult.Success(new[] { new PatternRole(Roles.Target, node) });
                })

                .AddCheck("Find Adapter candidates", 20, (target, graph) =>
                {
                    if (target.Symbol is not INamedTypeSymbol iface)
                        return PatternRuleResult.Empty;

                    var candidates = iface.GetImplementorsOf(graph.Registry).ToList();
                    if (!candidates.Any()) return PatternRuleResult.Empty;

                    return PatternRuleResult.Success(
                        new[] { new PatternRole(Roles.Target, target) }
                        .Concat(candidates.Select(c => new PatternRole(Roles.AdapterCandidate, c))));
                })

                .AddCheck("Adapter delegates to Adaptee", 60, (target, graph) =>
                {
                    if (target.Symbol is not INamedTypeSymbol iface)
                        return PatternRuleResult.Empty;

                    var ifaceMethods = iface.GetMembers().OfType<IMethodSymbol>().ToList();
                    var candidates = iface.GetImplementorsOf(graph.Registry);

                    var adapters = new HashSet<ClassNode>();
                    var adaptees = new HashSet<ClassNode>();

                    foreach (var adapter in candidates)
                    {
                        foreach (var adapteeType in adapter.GetHeldTypes().OfType<INamedTypeSymbol>())
                        {
                            if (adapteeType.ImplementsOrInherits(iface)) continue;
                            if (!adapter.HasInjectionOf(adapteeType)) continue;

                            if (adapter.DelegatesToType(adapteeType, ifaceMethods, graph.Registry))
                            {
                                adapters.Add(adapter);
                                if (graph.Registry.GetNode<ClassNode>(adapteeType) is { } adapteeNode)
                                    adaptees.Add(adapteeNode);
                            }
                        }
                    }

                    if (!adapters.Any()) return PatternRuleResult.Empty;

                    return PatternRuleResult.Success(
                        new[] { new PatternRole(Roles.Target, target) }
                        .Concat(adapters.Select(a => new PatternRole(Roles.Adapter, a)))
                        .Concat(adaptees.Select(d => new PatternRole(Roles.Adaptee, d))));
                })

                .AddCheck("Class name contains 'Adapter'", 5, (target, graph) =>
                {
                    if (target.Symbol is not INamedTypeSymbol iface)
                        return PatternRuleResult.Empty;

                    var candidates = iface.GetImplementorsOf(graph.Registry)
                        .Where(c => c.Symbol.Name.Contains("Adapter", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    return candidates.Any()
                        ? PatternRuleResult.Success(candidates.Select(c => new PatternRole(Roles.Adapter, c)))
                        : PatternRuleResult.Empty;
                });
        }
    }
}
