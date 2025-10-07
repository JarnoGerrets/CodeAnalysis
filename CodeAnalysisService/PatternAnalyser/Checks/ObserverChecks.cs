using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Helpers;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using System.Collections.Generic;
using System.Linq;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Observer pattern checks: subject with observer collections, notify calls, attach/detach.
    /// </summary>
    public static class ObserverChecks
    {
        private static class Roles
        {
            public const string Subject = "Subject";
            public const string Observer = "Observer";
        }

        public static RuleStep HasObserverCollection()
        {
            return new RuleStep
            {
                Description = "Class has IEnumerable<T>/List<T>/Dictionary<,T> field or property of non-primitive type",
                MustPass = true,
                Check = node =>
                {
                    var observerTypes = GetObserverCollectionTypes(node);
                    if (!observerTypes.Any())
                        return RuleStepResult.Empty;

                    return new RuleStepResult(
                        100,
                        true,
                        new[] { new PatternRole(Roles.Subject, node) }
                    );
                }
            };
        }

        public static RuleStep HasNotifyMethod(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Class has a method that iterates observers and calls one of their methods",
                MustPass = true,
                Check = node =>
                {
                    var observerTypes = GetObserverCollectionTypes(node);
                    if (!observerTypes.Any())
                        return RuleStepResult.Empty;

                    int calledCount = 0;
                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Subject, node)
                    };

                    foreach (var methodEdge in node.OutgoingEdges.Where(e => e.Type == EdgeType.HasMethod))
                    {
                        if (methodEdge.Target is not MethodNode methodNode) continue;

                        foreach (var calledMethod in GetCalledMethods(methodNode))
                        {
                            if (IsObserverType(calledMethod.Symbol.ContainingType, observerTypes))
                            {
                                calledCount++;

                                var obsNode = graph.Registry.GetNode<IAnalyzerNode>(calledMethod.Symbol.ContainingType);

                                var concreteImpls = calledMethod.OutgoingEdges
                                    .Where(e => e.Type == EdgeType.ImplementedBy && e.Target is MethodNode)
                                    .Select(e => (e.Target as MethodNode)?.Symbol.ContainingType)
                                    .OfType<INamedTypeSymbol>()
                                    .Select(t => graph.Registry.GetNode<IAnalyzerNode>(t))
                                    .Where(n => n != null);

                                foreach (var impl in concreteImpls)
                                {
                                    if (!roles.Any(r => r.Class == impl) && impl != null)
                                        roles.Add(new PatternRole(Roles.Observer, impl));
                                }

                            }
                        }
                    }

                    return calledCount > 0
                        ? new RuleStepResult(100, true, roles)
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep HasAttachDetachMethods()
        {
            return new RuleStep
            {
                Description = "Class has methods that register/unregister observers (parameter of observer type)",
                MustPass = false,
                Check = node =>
                {
                    var observerTypes = GetObserverCollectionTypes(node);
                    if (!observerTypes.Any())
                        return RuleStepResult.Empty;

                    foreach (var methodEdge in node.OutgoingEdges.Where(e => e.Type == EdgeType.HasMethod))
                    {
                        if (methodEdge.Target is not MethodNode methodNode) continue;

                        foreach (var param in methodNode.Symbol.Parameters)
                        {
                            if (IsObserverType(param.Type, observerTypes))
                            {
                                return new RuleStepResult(
                                    100,
                                    true,
                                    new[] { new PatternRole(Roles.Subject, node) }
                                );
                            }
                        }
                    }

                    return RuleStepResult.Empty;
                }
            };
        }

        private static IEnumerable<MethodNode> GetCalledMethods(MethodNode methodNode) =>
            methodNode.OutgoingEdges
                .Where(e => e.Type == EdgeType.Calls)
                .Select(e => e.Target)
                .OfType<MethodNode>();

        private static List<ITypeSymbol> GetObserverCollectionTypes(IAnalyzerNode node)
        {
            var types = new List<ITypeSymbol>();

            foreach (var edge in node.OutgoingEdges
                .Where(e => e.Type == EdgeType.HasField || e.Type == EdgeType.HasProperty))
            {
                ITypeSymbol? type = edge.Target switch
                {
                    FieldNode f => f.Symbol.Type,
                    PropertyNode p => p.Symbol.Type,
                    _ => null
                };

                var element = type != null ? TypeHelper.GetElementType(type) : null;
                if (element != null)
                {
                    element = TypeHelper.GetInnermostElementType(element);
                    types.Add(element);
                }
            }

            return types.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default).ToList();
        }

        private static bool IsObserverType(ITypeSymbol calledType, List<ITypeSymbol> observerTypes)
        {
            foreach (var obsType in observerTypes)
            {
                if (SymbolEqualityComparer.Default.Equals(calledType, obsType))
                    return true;

                if (calledType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, obsType)))
                    return true;
            }
            return false;
        }
    }
}
