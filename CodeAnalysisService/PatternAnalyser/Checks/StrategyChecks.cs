using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.GraphService.Helpers;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Strategy pattern: detect contract, concrete strategies, contexts with references,
    /// and delegation calls from the context to the strategy.
    /// </summary>
    public static class StrategyChecks
    {
        private static class Roles
        {
            public const string Strategy = "Strategy";
            public const string ConcreteStrategy = "ConcreteStrategy";
            public const string ContextCandidate = "ContextCandidate";
            public const string Context = "Context";
        }

        public static RuleStep HasStrategyInterface()
        {
            return new RuleStep
            {
                Description = "Detects interface/abstract defining strategy contract",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStrategyContract(node))
                        return RuleStepResult.Empty;

                    return new RuleStepResult(
                        100,
                        true,
                        new[] { new PatternRole(Roles.Strategy, node) }
                    );
                }
            };
        }

        public static RuleStep HasConcreteStrategies(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Finds classes implementing/deriving from strategy",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStrategyContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Strategy, node)
                    };

                    var allNodes = graph.Registry.GetAll<IAnalyzerNode>();

                    var concrete = allNodes
                        .Where(c =>
                            c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)) ||
                            SymbolEqualityComparer.Default.Equals(c.Symbol.BaseType, node.Symbol))
                        .ToList();

                    foreach (var c in concrete)
                        roles.Add(new PatternRole(Roles.ConcreteStrategy, c));

                    return concrete.Any()
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep ContextHasStrategyReference(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Detects context class with a field/prop/param of strategy type (direct or collection)",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStrategyContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Strategy, node)
                    };

                    var allNodes = graph.Registry.GetAll<IAnalyzerNode>();

                    var contexts = allNodes
                        .Where(c =>
                            c.OutgoingEdges.Any(e =>
                                (e.Type == EdgeType.HasField && e.Target is FieldNode f &&
                                    (SymbolEqualityComparer.Default.Equals(f.Symbol.Type, node.Symbol) ||
                                     SymbolEqualityComparer.Default.Equals(TypeHelper.GetElementType(f.Symbol.Type), node.Symbol)))
                                ||
                                (e.Type == EdgeType.HasProperty && e.Target is PropertyNode p &&
                                    (SymbolEqualityComparer.Default.Equals(p.Symbol.Type, node.Symbol) ||
                                     SymbolEqualityComparer.Default.Equals(TypeHelper.GetElementType(p.Symbol.Type), node.Symbol)))
                                ||
                                (e.Type == EdgeType.HasConstructor && e.Target is ConstructorNode ctor &&
                                    ctor.Symbol.Parameters.Any(p =>
                                        SymbolEqualityComparer.Default.Equals(p.Type, node.Symbol) ||
                                        SymbolEqualityComparer.Default.Equals(TypeHelper.GetElementType(p.Type), node.Symbol)))
                            )
                        )
                        .ToList();

                    foreach (var ctx in contexts)
                        roles.Add(new PatternRole(Roles.ContextCandidate, ctx));

                    return contexts.Any()
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep ContextDelegatesToStrategy(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Context must call methods on its strategy or its implementations",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStrategyContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Strategy, node)
                    };

                    var contexts = new List<IAnalyzerNode>();

                    foreach (var ctx in graph.Registry.GetAll<IAnalyzerNode>())
                    {
                        var methods = ctx.OutgoingEdges
                            .Where(e => e.Type == EdgeType.HasMethod)
                            .Select(e => e.Target as MethodNode);

                        foreach (var method in methods)
                        {
                            if (method == null) continue;

                            var calls = method.OutgoingEdges
                                .Where(e => e.Type == EdgeType.Calls)
                                .Select(e => e.Target as MethodNode)
                                .Where(m => m != null);

                            if (calls.Any(m =>
                            {
                                if (m == null)
                                return false;

                                var containing = m.Symbol.ContainingType;
                                return containing != null &&
                                    (SymbolEqualityComparer.Default.Equals(containing, node.Symbol) ||
                                        containing.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)) ||
                                        (TypeHelper.GetElementType(containing) is ITypeSymbol elementType &&
                                        SymbolEqualityComparer.Default.Equals(elementType, node.Symbol)));
                            }))

                            {
                                contexts.Add(ctx);
                                roles.Add(new PatternRole(Roles.Context, ctx));
                                break;
                            }
                        }
                    }

                    return contexts.Any()
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        private static bool IsStrategyContract(IAnalyzerNode node) =>
            node.Symbol.TypeKind == TypeKind.Interface || node.IsAbstract;
    }
}
