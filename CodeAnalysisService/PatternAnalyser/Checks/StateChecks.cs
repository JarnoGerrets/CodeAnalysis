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
    /// State pattern: detect state interface, concrete states,
    /// context holding and mutating state, and optional hints.
    /// </summary>
    public static class StateChecks
    {
        private static class Roles
        {
            public const string StateInterface = "StateInterface";
            public const string ConcreteState = "ConcreteState";
            public const string StateContextCandidate = "StateContextCandidate";
            public const string StateContext = "StateContext";
            public const string StateNameHint = "StateNameHint";
        }

        public static RuleStep HasStateInterface()
        {
            return new RuleStep
            {
                Description = "Detects interface/abstract defining state contract",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStateContract(node))
                        return RuleStepResult.Empty;

                    return new RuleStepResult(
                        100,
                        true,
                        new[] { new PatternRole(Roles.StateInterface, node) }
                    );
                }
            };
        }

        public static RuleStep HasConcreteStates(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Finds classes implementing/deriving from state interface/abstract",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStateContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.StateInterface, node)
                    };

                    var allNodes = graph.Registry.GetAll<IAnalyzerNode>();

                    var concrete = allNodes
                        .Where(c =>
                            c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)) ||
                            SymbolEqualityComparer.Default.Equals(c.Symbol.BaseType, node.Symbol))
                        .ToList();

                    foreach (var c in concrete)
                        roles.Add(new PatternRole(Roles.ConcreteState, c));

                    return concrete.Any()
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep ContextHasStateReference(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Detects context class with a field/prop/param of state type (direct or collection)",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStateContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.StateInterface, node)
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
                        roles.Add(new PatternRole(Roles.StateContextCandidate, ctx));

                    return contexts.Any()
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep ContextDelegatesToState(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Context must call methods on its state or its implementations",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStateContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.StateInterface, node)
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
                                roles.Add(new PatternRole(Roles.StateContext, ctx));
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

        public static RuleStep StateMutatesContext(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Concrete State causes the Context to change its current state",
                MustPass = true,
                Check = node =>
                {
                    if (!IsStateContract(node))
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.StateInterface, node)
                    };

                    // All concrete states of this interface
                    var states = graph.Registry.GetAll<ClassNode>()
                        .Where(c => c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)))
                        .ToList();

                    Console.WriteLine($"[DEBUG] StateMutatesContext: Checking state contract {node.Symbol.Name}");
                    Console.WriteLine($"[DEBUG]   Found {states.Count} concrete state(s) implementing {node.Symbol.Name}");

                    foreach (var state in states)
                    {
                        foreach (var methodEdge in state.OutgoingEdges.Where(e => e.Type == EdgeType.HasMethod))
                        {
                            if (methodEdge.Target is not MethodNode method) continue;

                            var calls = method.OutgoingEdges
                                .Where(e => e.Type == EdgeType.Calls)
                                .Select(e => e.Target as MethodNode)
                                .Where(m => m != null)
                                .ToList();

                            var creates = method.OutgoingEdges
                                .Where(e => e.Type == EdgeType.Creates)
                                .Select(e => e.Target as ClassNode)
                                .Where(c => c != null)
                                .ToList();

                            // filter only created states
#pragma warning disable CS8602 // Dereference of a possibly null reference -> on c.
                            var createdStates = creates
                                .Where(c => c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)))
                                .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                            if (!createdStates.Any())
                                continue;

                            // check if any call goes into a context
                            foreach (var called in calls)
                            {
                                var containing = called!.Symbol.ContainingType;
                                if (containing == null) continue;

                                // heuristic: the containing type is the context if it has a field of the state interface
                                var isContext = graph.Registry.GetAll<ClassNode>()
                                    .Any(ctx => SymbolEqualityComparer.Default.Equals(ctx.Symbol, containing) &&
                                                ctx.OutgoingEdges.Any(e =>
                                                    e.Target is FieldNode f &&
                                                    SymbolEqualityComparer.Default.Equals(f.Symbol.Type, node.Symbol)));

                                if (isContext)
                                {   
                                    Console.WriteLine($"[DEBUG] {state.Symbol.Name}.{method.Symbol.Name} calls {containing.Name}.{called.Symbol.Name}(...) and creates {string.Join(", ", createdStates.Select(c => c?.Symbol.Name))}");

                                    roles.Add(new PatternRole(Roles.ConcreteState, state));

                                    var contextNode = graph.Registry.GetAll<IAnalyzerNode>()
                                        .FirstOrDefault(n => SymbolEqualityComparer.Default.Equals(n.Symbol, containing));
                                    if (contextNode != null)
                                        roles.Add(new PatternRole(Roles.StateContext, contextNode));

                                    return new RuleStepResult(100, true, roles);
                                }
                            }
                        }
                    }

                    Console.WriteLine($"[DEBUG]   ❌ {node.Symbol.Name} has no concrete states mutating its context.");
                    return RuleStepResult.Empty;
                }
            };
        }


        public static RuleStep StateHasContextReference(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Concrete state has a reference to the Context (for transitions)",
                MustPass = false,
                Check = node =>
                {
                    var stateInterface = node.Symbol;
                    var states = graph.Registry.GetAll<ClassNode>()
                        .Where(c => c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, stateInterface)))
                        .ToList();

                    foreach (var s in states)
                    {
                        if (s.OutgoingEdges.Any(e =>
                            (e.Type == EdgeType.HasField || e.Type == EdgeType.HasProperty || e.Type == EdgeType.HasConstructor) &&
                            SymbolEqualityComparer.Default.Equals(
                                e.Target switch
                                {
                                    FieldNode f => f.Symbol.Type,
                                    PropertyNode p => p.Symbol.Type,
                                    ConstructorNode ctor => ctor.Symbol.ContainingType,
                                    _ => null
                                },
                                node.Symbol.ContainingType
                            )))
                        {
                            return new RuleStepResult(50, true, new[] { new PatternRole(Roles.ConcreteState, s) });
                        }
                    }

                    return RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep StateMethodNameHint()
        {
            string[] stateHints = { "handle", "change", "next", "enter", "exit" };

            return new RuleStep
            {
                Description = "Interface methods have stateful naming hints",
                MustPass = false,
                Check = node =>
                {
                    if (node.Symbol.TypeKind != TypeKind.Interface && !node.IsAbstract)
                        return RuleStepResult.Empty;

                    var methods = node.Symbol.GetMembers().OfType<IMethodSymbol>();
                    if (methods.Any(m => stateHints.Any(h => m.Name.ToLower().Contains(h))))
                    {
                        return new RuleStepResult(25, true, new[] { new PatternRole(Roles.StateNameHint, node) });
                    }

                    return RuleStepResult.Empty;
                }
            };
        }

        private static bool IsStateContract(IAnalyzerNode node) =>
            node.Symbol.TypeKind == TypeKind.Interface || node.IsAbstract;
    }
}
