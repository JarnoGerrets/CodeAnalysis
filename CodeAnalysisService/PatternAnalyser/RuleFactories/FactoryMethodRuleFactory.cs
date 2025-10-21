using Microsoft.CodeAnalysis;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.Nodes;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using CodeAnalysisService.PatternAnalyser.Rules;
using CodeAnalysisService.PatternAnalyser.Queries;
using CodeAnalysisService.Names;

namespace CodeAnalysisService.PatternAnalyser.RuleFactories
{
    /// <summary>
    /// Rulefactory to detect FactoryMethod pattern.
    /// </summary>
    public static class FactoryMethodRuleFactory
    {
        public static PatternRule Create()
        {
            return new PatternRule("FactoryMethod")

                .AddCheck("class has abstract or virtual method returning interface/abstract and has an creates edge", 40,
                    (node, _) =>
                    {
                        if (node is not ClassNode cls)
                            return PatternRuleResult.Empty;

                        var methods = cls.GetMethods().Where(m => (m.Symbol.IsAbstract || m.Symbol.IsVirtual) && m.Edges.Any(e => e.Type == EdgeType.Creates));

                        bool found = methods.Any(m =>
                        {
                            var returnType = m.Symbol.ReturnType;
                            return returnType.TypeKind == TypeKind.Interface
                                || (returnType is INamedTypeSymbol named && named.IsAbstract);
                        });

                        return found
                            ? PatternRuleResult.Success([new PatternRole(Roles.AbstractFactory, cls)])
                            : PatternRuleResult.Empty;
                    })

                .AddCheck("Concrete subclass overrides factory method and creates product", 40,
                    (node, graph) =>
                    {
                        if (!node.Symbol.IsAbstract || node is not ClassNode cls)
                            return PatternRuleResult.Empty;

                        var roles = new List<PatternRole> { new(Roles.AbstractFactory, cls) };

                        var concreteFactories = graph.Registry.GetAll<ClassNode>()
                            .Where(c => c.InheritsFrom(cls.Symbol))
                            .Where(sub =>
                                sub.GetMethods().Any(m =>
                                    m.Symbol.IsOverride &&
                                    m.Edges.Any(e => e.Type == EdgeType.Creates)))
                            .ToList();

                        foreach (var factory in concreteFactories)
                            roles.Add(new PatternRole(Roles.ConcreteFactory, factory));

                        return concreteFactories.Any()
                            ? PatternRuleResult.Success(roles)
                            : PatternRuleResult.Empty;
                    })

                .AddCheck("Created product implements or derives from abstract return type", 20,
                    (node, graph) =>
                    {
                        if (node is not ClassNode cls) return PatternRuleResult.Empty;

                        var roles = new List<PatternRole> { new(Roles.AbstractFactory, cls) };

                        int totalFactoryMethods = 0;
                        int validCreations = 0;

                        foreach (var sub in graph.Registry.GetAll<ClassNode>()
                            .Where(c => c.InheritsFrom(cls.Symbol)))
                        {
                            foreach (var method in sub.GetMethods())
                            {
                                totalFactoryMethods++;
                                bool methodProducedValid = false;

                                foreach (var createEdge in method.Edges.Where(e => e.Type == EdgeType.Creates))
                                {
                                    if (createEdge.Target is ClassNode createdClass)
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
                                    // Record unfulfilled contract
                                    var unfulfilledNode = graph.Registry.GetNode<ClassNode>(returnType);
                                    if (unfulfilledNode != null)
                                        roles.Add(new PatternRole(Roles.UnfulfilledReturn, unfulfilledNode));

                                    // Collect expected products that don’t match
                                    var expectedNodes = graph.Registry.GetAll<ClassNode>()
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

                        return score > 0
                            ? PatternRuleResult.Success(roles)
                            : PatternRuleResult.Empty;
                    });
        }
    }
}
