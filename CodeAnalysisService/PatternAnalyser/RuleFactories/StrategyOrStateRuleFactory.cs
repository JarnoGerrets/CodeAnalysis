using CodeAnalysisService.GraphService.Helpers;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.Names;
using CodeAnalysisService.PatternAnalyser.Queries;
using CodeAnalysisService.Enums;
using CodeAnalysisService.Names;

namespace CodeAnalysisService.PatternAnalyser.RuleFactories
{
    /// <summary>
    /// Rule factory to detect the strategy pattern and if possible promote it to State.
    /// </summary>
    public static class StrategyOrStateRuleFactory
    {
        public static PatternRule Create()
        {
            return new PatternRule(PatternNames.Strategy)

                .AddCheck("Detects interface/abstract defining strategy contract", 20,
                    (node, _) =>
                        (node.Symbol.TypeKind == TypeKind.Interface || node.Symbol.IsAbstract)
                            ? PatternRuleResult.Success(new[] { new PatternRole(Roles.Strategy, node) })
                            : PatternRuleResult.Empty)

                .AddCheck("Finds classes implementing/deriving from strategy", 20,
                    (node, graph) =>
                    {
                        if (node.Symbol.TypeKind != TypeKind.Interface && !node.Symbol.IsAbstract)
                            return PatternRuleResult.Empty;
                        if (node.Symbol is not INamedTypeSymbol named) return PatternRuleResult.Empty;

                        var implementors = named.GetImplementorsOf(graph.Registry).ToList();
                        if (!implementors.Any()) return PatternRuleResult.Empty;

                        var roles = new List<PatternRole> { new(Roles.Strategy, node) };
                        roles.AddRange(implementors.Select(c => new PatternRole(Roles.ConcreteStrategy, c)));

                        return PatternRuleResult.Success(roles);
                    })

                .AddCheck("Detects context class with a reference to strategy", 10,
                    (node, graph) =>
                    {
                        if (node.Symbol.TypeKind != TypeKind.Interface && !node.Symbol.IsAbstract)
                            return PatternRuleResult.Empty;
                        if (node.Symbol is not INamedTypeSymbol named) return PatternRuleResult.Empty;

                        var candidates = graph.Registry.GetAll<ClassNode>()
                            .Where(c => !named.Equals(c.Symbol, SymbolEqualityComparer.Default) &&
                                        !c.Symbol.ImplementsOrInherits(named))
                            .Where(c => c.HoldsReferenceTo(named))
                            .Where(c => !c.GetMethods().Any(m => m.Edges.Any(e => e.Type == EdgeType.Creates)))
                            .ToList();

                        if (!candidates.Any()) return PatternRuleResult.Empty;

                        var roles = new List<PatternRole> { new(Roles.Strategy, node) };
                        roles.AddRange(candidates.Select(c => new PatternRole(Roles.ContextCandidate, c)));

                        return PatternRuleResult.Success(roles);
                    })

                .AddCheck("Context delegates to strategy methods", 50,
                    (node, graph) =>
                    {
                        if (node.Symbol.TypeKind != TypeKind.Interface && !node.Symbol.IsAbstract)
                            return PatternRuleResult.Empty;
                        if (node.Symbol is not INamedTypeSymbol named) return PatternRuleResult.Empty;

                        var contexts = graph.Registry.GetAll<ClassNode>()
                            .Where(c => !named.Equals(c.Symbol, SymbolEqualityComparer.Default) &&
                                        !c.Symbol.ImplementsOrInherits(named))
                            .Where(c => c.DelegatesTo(named, graph.Registry))
                            .Where(c => !c.GetMethods().Any(m => m.Edges.Any(e => e.Type == EdgeType.Creates)))
                            .ToList();

                        if (!contexts.Any()) return PatternRuleResult.Empty;

                        var roles = new List<PatternRole> { new(Roles.Strategy, node) };
                        roles.AddRange(contexts.Select(c => new PatternRole(Roles.Context, c)));

                        return PatternRuleResult.Success(roles);
                    })

                //  Promote Strategy to State
                .WithPostProcessor((result, graph) =>
                {
                    if (result.PatternName != PatternNames.Strategy || result.Score < 70)
                        return result;

                    var strategyRole = result.Roles.FirstOrDefault(r => r.Role == Roles.Strategy);
                    if (strategyRole?.Class.Symbol is not INamedTypeSymbol strategySym)
                        return result;

                    var contexts = result.Roles.Where(r => r.Role == Roles.Context)
                                               .Select(r => r.Class).OfType<ClassNode>().ToList();
                    var concrete = result.Roles.Where(r => r.Role == Roles.ConcreteStrategy)
                                               .Select(r => r.Class).OfType<ClassNode>().ToList();

                    if (!contexts.Any() || !concrete.Any()) return result;

                    bool anyStateRefsContext = concrete.Any(st =>
                        contexts.Any(ctx => st.HoldsReferenceTo(ctx.Symbol)));

                    bool contextCanSwitchState = contexts.Any(ctx =>
                        ctx.HasStateSwitchMethod(strategySym));

                    bool hasNameHints = concrete.Any(s =>
                        s.Symbol.Name.EndsWith("State", StringComparison.OrdinalIgnoreCase));

                    var stateChecks = new List<CheckResult>
                    {
                        new("Concrete state references context", 25, anyStateRefsContext),
                        new("Context can switch states", 35, contextCanSwitchState),
                        new("State naming hint", 10, hasNameHints)
                    };

                    int gained = stateChecks.Where(c => c.Passed).Sum(c => c.Weight);
                    int total = stateChecks.Sum(c => c.Weight);
                    int stateScore = total > 0 ? (int)((double)gained / total * 100) : 0;

                    if (stateScore < 51) return result;

                    var remappedRoles = result.Roles.Select(r => new PatternRole(
                        r.Role switch
                        {
                            Roles.Strategy => Roles.StateInterface,
                            Roles.ConcreteStrategy => Roles.ConcreteState,
                            Roles.Context => Roles.StateContext,
                            Roles.ContextCandidate => Roles.StateContextCandidate,
                            _ => r.Role
                        }, r.Class)).ToList();

                    if (hasNameHints)
                        remappedRoles.AddRange(concrete
                            .Where(s => s.Symbol.Name.EndsWith("State", StringComparison.OrdinalIgnoreCase))
                            .Select(s => new PatternRole(Roles.StateNameHint, s)));

                    var augmentedChecks = result.Checks.Concat(stateChecks).Concat(new[]
                    {
                        new CheckResult("Promoted to State: context-state coupling verified", 0, true)
                    }).ToList();

                    return new PatternResult(
                        PatternNames.State,
                        result.Score,
                        result.Classification,
                        augmentedChecks,
                        remappedRoles.Distinct(Comparers.PatternRole).ToList()
                    );
                });
        }
    }
}
