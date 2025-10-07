using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.GraphService.Helpers;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Detect Observer interactions via method calls from a subject to observer element types.
    /// </summary>
    public static class MethodCallChecks
    {
        private static class Roles
        {
            public const string Observer = "Observer";
        }

        public static RuleStep CallsObserverElements(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Iterates/calls element types",
                MustPass = false,
                Check = node =>
                {
                    var observerTypes = GetObserverCollectionTypes(node);
                    if (!observerTypes.Any())
                        return RuleStepResult.Empty;

                    int calledCount = 0;
                    var relatedRoles = new List<PatternRole>();

                    foreach (var methodEdge in node.OutgoingEdges.Where(e => e.Type == EdgeType.HasMethod))
                    {
                        if (methodEdge.Target is not MethodNode methodNode) continue;

                        foreach (var calledMethod in GetCalledMethods(methodNode))
                        {
                            if (IsObserverType(calledMethod.Symbol.ContainingType, observerTypes))
                            {
                                calledCount++;

                                var obsNode = graph.Registry
                                    .GetAll<IAnalyzerNode>()
                                    .FirstOrDefault(c => SymbolEqualityComparer.Default.Equals(
                                        c.Symbol, calledMethod.Symbol.ContainingType));

                                if (obsNode != null && !relatedRoles.Any(r => r.Class == obsNode))
                                {
                                    relatedRoles.Add(new PatternRole(Roles.Observer, obsNode));
                                }
                            }
                        }
                    }

                    int score = Math.Min(100, (int)((double)calledCount / observerTypes.Count * 100));
                    return new RuleStepResult(score, score > 0, relatedRoles);
                }
            };
        }

        private static IEnumerable<MethodNode> GetCalledMethods(MethodNode methodNode) =>
            methodNode.OutgoingEdges
                .Where(e => e.Type == EdgeType.Calls || e.Type == EdgeType.ImplementedBy)
                .Select(e => e.Target)
                .OfType<MethodNode>();

        public static List<ITypeSymbol> GetObserverCollectionTypes(IAnalyzerNode node)
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

                var elem = type != null ? TypeHelper.GetElementType(type) : null;
                if (elem != null)
                    types.Add(elem);
            }

            return types.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default).ToList();
        }

        public static bool IsObserverType(ITypeSymbol calledType, List<ITypeSymbol> observerTypes)
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
