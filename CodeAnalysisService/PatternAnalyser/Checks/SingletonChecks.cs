using CodeAnalysisService.Enums;
using CodeAnalysisService.PatternAnalyser.RuleSteps;
using CodeAnalysisService.GraphService.Nodes;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.PatternAnalyser.PatternRoles;
using System.Collections.Generic;
using System.Linq;

namespace CodeAnalysisService.PatternAnalyser.Checks
{
    /// <summary>
    /// Singleton detection: private ctor, static instance field (direct/lazy/nested/generic),
    /// static accessor (property/method/field).
    /// </summary>
    public static class SingletonChecks
    {
        private static class Roles
        {
            public const string Singleton = "Singleton";
        }

        public static RuleStep HasPrivateConstructor()
        {
            return new RuleStep
            {
                Description = "Has private constructor",
                MustPass = true,
                Check = classNode =>
                {
                    var ctors = classNode.OutgoingEdges
                        .Where(e => e.Type == EdgeType.HasConstructor)
                        .Select(e => e.Target)
                        .OfType<ConstructorNode>();

                    bool hasPrivate = ctors.Any(c =>
                        c.Symbol.DeclaredAccessibility == Accessibility.Private);

                    return hasPrivate
                        ? new RuleStepResult(100, true, new[] { new PatternRole(Roles.Singleton, classNode) })
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep HasStaticInstanceField()
        {
            return new RuleStep
            {
                Description = "Class has a static field of its own type (direct, Lazy<T>, nested holder, or generic type param)",
                MustPass = true,
                Check = node =>
                {
                    bool hasStaticField = false;

                    hasStaticField |= GetStaticFields(node)
                        .Any(f => SymbolEqualityComparer.Default.Equals(f.Symbol.Type, node.Symbol));

                    hasStaticField |= GetStaticFields(node)
                        .Any(f =>
                            f.Symbol.Type is INamedTypeSymbol named &&
                            named.ConstructedFrom?.ToString() == "System.Lazy<T>" &&
                            SymbolEqualityComparer.Default.Equals(named.TypeArguments[0], node.Symbol));

                    hasStaticField |= node.Symbol.GetTypeMembers()
                        .SelectMany(nested => nested.GetMembers().OfType<IFieldSymbol>())
                        .Any(f => f.IsStatic &&
                                  SymbolEqualityComparer.Default.Equals(f.Type, node.Symbol));

                    var typeParams = node.Symbol.TypeParameters;
                    hasStaticField |= GetStaticFields(node)
                        .Any(f => typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(f.Symbol.Type, tp)));

                    return hasStaticField
                        ? new RuleStepResult(100, true, new[] { new PatternRole(Roles.Singleton, node) })
                        : RuleStepResult.Empty;
                }
            };
        }

        public static RuleStep HasStaticAccessor()
        {
            return new RuleStep
            {
                Description = "Class has a static property/method/field returning its own type",
                MustPass = true,
                Check = node =>
                {
                    var typeParams = node.Symbol.TypeParameters;

                    var hasStaticProperty = node.OutgoingEdges
                        .Where(e => e.Type == EdgeType.HasProperty && e.Target is PropertyNode)
                        .Select(e => (PropertyNode)e.Target)
                        .Any(p => p.Symbol.IsStatic &&
                                  (SymbolEqualityComparer.Default.Equals(p.Symbol.Type, node.Symbol) ||
                                   typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(p.Symbol.Type, tp))));

                    var hasStaticMethod = node.OutgoingEdges
                        .Where(e => e.Type == EdgeType.HasMethod && e.Target is MethodNode)
                        .Select(e => (MethodNode)e.Target)
                        .Any(m => m.Symbol.IsStatic &&
                                  (SymbolEqualityComparer.Default.Equals(m.Symbol.ReturnType, node.Symbol) ||
                                   typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(m.Symbol.ReturnType, tp))));

                    var hasStaticFieldAccessor = GetStaticFields(node)
                        .Any(f =>
                            (f.Symbol.DeclaredAccessibility == Accessibility.Public ||
                             f.Symbol.DeclaredAccessibility == Accessibility.Internal) &&
                            (SymbolEqualityComparer.Default.Equals(f.Symbol.Type, node.Symbol) ||
                             typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(f.Symbol.Type, tp))));

                    var passed = hasStaticProperty || hasStaticMethod || hasStaticFieldAccessor;

                    return passed
                        ? new RuleStepResult(100, true, new[] { new PatternRole(Roles.Singleton, node) })
                        : RuleStepResult.Empty;
                }
            };
        }

        private static IEnumerable<FieldNode> GetStaticFields(IAnalyzerNode node) =>
            node.OutgoingEdges
                .Where(e => e.Type == EdgeType.HasField && e.Target is FieldNode)
                .Select(e => (FieldNode)e.Target);
    }
}
