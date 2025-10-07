using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Provides static rule checks for detecting the Adapter design pattern.
    /// Includes steps to:
    /// - Identify a Target interface/abstract class.
    /// - Find Adapter candidates implementing or deriving from Target.
    /// - Verify Adapters delegate Target methods to injected Adaptees.
    /// Uses <see cref="GraphBuilder"/> and <see cref="NodeRegistry"/> to
    /// traverse class/interface nodes, fields, properties, constructors,
    /// and method calls for pattern analysis.
    /// </summary>
    public static class AdapterChecks
    {
        private static class Roles
        {
            public const string AdapterCandidate = "AdapterCandidate";
            public const string Adapter = "Adapter";
            public const string Adaptee = "Adaptee";
            public const string Target = "Target";
        }

        public static RuleStep HasTarget()
        {
            return new RuleStep
            {
                Description = "Detects Target interface/abstract",
                MustPass = true,
                Check = node =>
                {
                    if (node.Symbol.TypeKind != TypeKind.Interface && !node.IsAbstract)
                        return RuleStepResult.Empty;

                    return new RuleStepResult(100, true, new[]
                    {
                        new PatternRole(Roles.Target, node)
                    });
                }
            };
        }

        public static RuleStep HasAdapterCandidates(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Find classes that implement/derive from Target (Adapter candidates)",
                MustPass = true,
                Check = node =>
                {
                    if (node.Symbol.TypeKind != TypeKind.Interface && !node.IsAbstract)
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Target, node)
                    };

                    var candidates = graph.Registry.GetAll<ClassNode>()
                        .Where(c =>
                            c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, node.Symbol)) ||
                            SymbolEqualityComparer.Default.Equals(c.Symbol.BaseType, node.Symbol))
                        .ToList();

                    roles.AddRange(candidates.Select(a => new PatternRole(Roles.AdapterCandidate, a)));

                    var found = candidates.Any();
                    return new RuleStepResult(found ? 100 : 0, found, roles);
                }
            };
        }

        public static RuleStep AdapterDelegatesToAdaptee(GraphBuilder graph)
        {
            return new RuleStep
            {
                Description = "Adapter implements Target method(s) and delegates to an injected Adaptee",
                MustPass = true,
                Check = node =>
                {
                    if (node.Symbol.TypeKind != TypeKind.Interface && !node.IsAbstract)
                        return RuleStepResult.Empty;

                    var roles = new List<PatternRole>
                    {
                        new PatternRole(Roles.Target, node)
                    };

                    var targetInterface = node.Symbol;
                    var ifaceMethods = targetInterface.GetMembers().OfType<IMethodSymbol>().ToList();

                    var candidates = graph.Registry.GetAll<ClassNode>()
                        .Where(c => c.Symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetInterface)))
                        .ToList();

                    var promotedAdapters = new HashSet<ClassNode>();
                    var foundAdaptees = new HashSet<ClassNode>();

                    foreach (var adapter in candidates)
                    {
                        var heldAdapteeTypes = adapter.OutgoingEdges
                            .Where(e =>
                                (e.Type == EdgeType.HasField && e.Target is FieldNode) ||
                                (e.Type == EdgeType.HasProperty && e.Target is PropertyNode))
                            .Select(e => ResolveMemberType(e.Target))
                            .OfType<ITypeSymbol>()
                            .Distinct(SymbolEqualityComparer.Default)
                            .OfType<INamedTypeSymbol>()
                            .ToList();

                        if (!heldAdapteeTypes.Any())
                            continue;

                        bool HasInjectionOf(INamedTypeSymbol type) =>
                            adapter.OutgoingEdges
                                   .Where(e => e.Type == EdgeType.HasConstructor && e.Target is ConstructorNode)
                                   .Select(e => ((ConstructorNode)e.Target).Symbol)
                                   .Any(ctor => ctor.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, type)))
                            ||
                            adapter.OutgoingEdges
                                   .Where(e => e.Type == EdgeType.HasProperty && e.Target is PropertyNode)
                                   .Select(e => ((PropertyNode)e.Target).Symbol.Type)
                                   .Any(pt => SymbolEqualityComparer.Default.Equals(pt, type));

                        foreach (var adapteeType in heldAdapteeTypes)
                        {
                            if (adapteeType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetInterface)))
                                continue;

                            if (!HasInjectionOf(adapteeType))
                                continue;

                            bool anyTargetImplDelegates = ifaceMethods.Any(ifaceMethod =>
                            {
                                var impl = adapter.Symbol.FindImplementationForInterfaceMember(ifaceMethod) as IMethodSymbol;
                                if (impl == null) return false;

                                var implNode = graph.Registry.GetNode<MethodNode>(impl);
                                if (implNode == null) return false;

                                return implNode.OutgoingEdges
                                    .Where(e => e.Type == EdgeType.Calls)
                                    .Select(e => e.Target as MethodNode)
                                    .Any(m => m != null &&
                                              SymbolEqualityComparer.Default.Equals(m.Symbol.ContainingType, adapteeType));
                            });

                            if (anyTargetImplDelegates)
                            {
                                promotedAdapters.Add(adapter);

                                var adapteeNode = graph.Registry.GetNode<ClassNode>(adapteeType);
                                if (adapteeNode != null) foundAdaptees.Add(adapteeNode);
                            }
                        }
                    }

                    roles.AddRange(promotedAdapters.Select(a => new PatternRole(Roles.Adapter, a)));
                    roles.AddRange(foundAdaptees.Select(d => new PatternRole(Roles.Adaptee, d)));

                    var passed = promotedAdapters.Any();
                    return new RuleStepResult(passed ? 100 : 0, passed, roles);
                }
            };
        }

        private static INamedTypeSymbol? ResolveMemberType(INode node) =>
            node switch
            {
                FieldNode f => f.Symbol.Type as INamedTypeSymbol,
                PropertyNode p => p.Symbol.Type as INamedTypeSymbol,
                _ => null
            };
    }
}
