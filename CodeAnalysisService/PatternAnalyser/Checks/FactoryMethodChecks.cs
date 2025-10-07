using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using CodeAnalysisService.GraphService;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Factory Method detection: abstract factories, overriding factory methods,
    /// and created products matching declared return types.
    /// </summary>
    public static class FactoryMethodChecks
    {
        private static class Roles
        {
            public const string AbstractFactory = "AbstractFactory";
            public const string ConcreteFactory = "ConcreteFactory";
            public const string Product = "Product";
            public const string InvalidProduct = "InvalidProduct";
            public const string UnfulfilledReturn = "UnfulfilledReturn";
        }

        public static RuleStep HasAbstractFactoryMethod()
        {
            return new RuleStep
            {
                Description = "Abstract class has abstract method returning interface/abstract",
                MustPass = true,
                Check = node =>
                {
                    if (!node.IsAbstract)
                        return RuleStepResult.Empty;

                    var methods = node.OutgoingEdges
                        .Where(e => e.Type == EdgeType.HasMethod)
                        .Select(e => e.Target as MethodNode)
                        .Where(m => m?.IsAbstract == true);
                
                    bool found = methods.Any(m =>
                    {
                        if (m == null)
                            return false;

                        var returnType = m.Symbol.ReturnType;
                        return returnType.TypeKind == TypeKind.Interface
                            || (returnType is INamedTypeSymbol named && named.IsAbstract);
                    });

                    return found
                        ? new RuleStepResult(100, true, new List<PatternRole> { new PatternRole(Roles.AbstractFactory, node) })
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep SubclassesOverrideFactoryMethod(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Concrete subclass overrides factory method and creates product",
                MustPass = true,
                Check = node =>
                {
                    if (!node.IsAbstract)
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole> { new PatternRole(Roles.AbstractFactory, node) };

                    var concreteFactories = graph.Registry.GetAll<IAnalyzerNode>()
                        .Where(c => InheritsFrom(c.Symbol, node.Symbol))
                        .SelectMany(sub =>
                            sub.OutgoingEdges
                               .Where(e => e.Type == EdgeType.HasMethod)
                               .Select(e => e.Target as MethodNode)
                               .Where(m => m?.Symbol.IsOverride == true &&
                                           m.OutgoingEdges.Any(e => e.Type == EdgeType.Creates))
                               .Select(_ => sub))
                        .Distinct();

                    foreach (var factory in concreteFactories)
                        roles.Add(new PatternRole(Roles.ConcreteFactory, factory));

                    bool found = concreteFactories.Any();
                    return new RuleStepResult(found ? 100 : 0, found, roles);
                }
            };
        }

        public static RuleStep CreatedProductImplementsReturnType(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Created product implements or derives from abstract return type",
                MustPass = false,
                Check = node =>
                {
                    var roles = new List<PatternRole> { new PatternRole(Roles.AbstractFactory, node) };

                    int totalFactoryMethods = 0;
                    int validCreations = 0;

                    foreach (var sub in graph.Registry.GetAll<IAnalyzerNode>()
                            .Where(c => InheritsFrom(c.Symbol, node.Symbol)))
                    {
                        foreach (var method in sub.OutgoingEdges
                                .Where(e => e.Type == EdgeType.HasMethod)
                                .Select(e => e.Target as MethodNode))
                        {
                            if (method?.Symbol == null) continue;

                            totalFactoryMethods++;
                            bool methodProducedValid = false;

                            foreach (var createEdge in method.OutgoingEdges.Where(e => e.Type == EdgeType.Creates))
                            {
                                if (createEdge.Target is IAnalyzerNode createdClass)
                                {
                                    var returnType = method.Symbol.ReturnType;
                                    if (returnType != null &&
                                        (createdClass.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, returnType)) ||
                                         SymbolEqualityComparer.Default.Equals(createdClass.Symbol.BaseType, returnType)))
                                    {
                                        roles.Add(new PatternRole(Roles.Product, createdClass));
                                        methodProducedValid = true;
                                    }
                                }
                            }

                            if (methodProducedValid)
                            {
                                validCreations++;
                            }
                            else if (method.Symbol.ReturnType is INamedTypeSymbol returnType)
                            {
                                var unfulfilledNode = graph.Registry.GetNode<IAnalyzerNode>(returnType);
                                if (unfulfilledNode != null)
                                {
                                    roles.Add(new PatternRole(Roles.UnfulfilledReturn, unfulfilledNode));
                                }

                                var expectedNodes = graph.Registry.GetAll<IAnalyzerNode>()
                                    .Where(c =>
                                        c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, returnType)) ||
                                        SymbolEqualityComparer.Default.Equals(c.Symbol.BaseType, returnType));

                                foreach (var expected in expectedNodes)
                                    roles.Add(new PatternRole(Roles.InvalidProduct, expected));
                            }
                        }
                    }

                    int score = totalFactoryMethods > 0
                        ? (int)((double)validCreations / totalFactoryMethods * 100)
                        : 0;

                    return new RuleStepResult(score, score > 0, roles);
                }
            };
        }

        private static bool InheritsFrom(INamedTypeSymbol symbol, INamedTypeSymbol potentialBase)
        {
            var current = symbol.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, potentialBase))
                    return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
