using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphBuildingService.Nodes;
using Microsoft.CodeAnalysis;
using CodeAnalysisService.Helpers;
using CodeAnalysisService.GraphBuildingService.Registry;

namespace CodeAnalysisService.PatternAnalyser.Queries
{
    /// <summary>
    /// Common reusable graph queries to simplify pattern checks.
    /// </summary>
    public static class GraphQueries
    {
        // ---------------------------------------------------------------------------------------------------------------
        // Class Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static bool HasMethod(this ClassNode cls, string methodName) =>
            cls.Edges.Any(e =>
                e.Type == EdgeType.HasMethod &&
                e.Target is MethodNode m &&
                string.Equals(m.Symbol.Name, methodName, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<MethodNode> GetMethods(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.HasMethod)
               .Select(e => e.Target)
               .OfType<MethodNode>();

        public static bool HasProperty(this ClassNode cls, string propertyName) =>
            cls.Edges.Any(e =>
                e.Type == EdgeType.HasProperty &&
                e.Target is PropertyNode p &&
                string.Equals(p.Symbol.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<PropertyNode> GetProperties(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.HasProperty)
               .Select(e => e.Target)
               .OfType<PropertyNode>();

        public static bool ImplementsInterface(this ClassNode cls, string interfaceName) =>
            cls.Edges.Any(e =>
                e.Type == EdgeType.Implements &&
                e.Target is InterfaceNode i &&
                i.Symbol.Name == interfaceName);

        public static IEnumerable<InterfaceNode> GetInterfaces(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.Implements)
               .Select(e => e.Target)
               .OfType<InterfaceNode>();

        public static bool InheritsFrom(this ClassNode cls, string baseClassName) =>
            cls.Edges.Any(e =>
                e.Type == EdgeType.Inherits &&
                e.Target is ClassNode baseCls &&
                baseCls.Symbol.Name == baseClassName);

        public static bool InheritsFrom(this ClassNode cls, INamedTypeSymbol potentialBase)
        {
            var current = cls.Symbol.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, potentialBase))
                    return true;
                current = current.BaseType;
            }
            return false;
        }


        // ---------------------------------------------------------------------------------------------------------------
        // Field Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static IEnumerable<FieldNode> GetFields(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.HasField)
               .Select(e => e.Target)
               .OfType<FieldNode>();

        public static bool HasFieldOfType(this ClassNode cls, string typeName) =>
            cls.GetFields().Any(f => f.Symbol.Type.Name == typeName);

        public static bool HasStaticFieldOfType(this ClassNode cls, string typeName) =>
            cls.GetFields().Any(f => f.Symbol.IsStatic && f.Symbol.Type.Name == typeName);

        // ---------------------------------------------------------------------------------------------------------------
        // Constructor Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static IEnumerable<ConstructorNode> GetConstructors(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.HasConstructor)
               .Select(e => e.Target)
               .OfType<ConstructorNode>();

        public static bool HasPrivateConstructor(this ClassNode cls) =>
            cls.GetConstructors().Any(c => c.Symbol.DeclaredAccessibility == Accessibility.Private);

        // ---------------------------------------------------------------------------------------------------------------
        // Method CALL Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static IEnumerable<MethodNode> CalledMethods(this MethodNode method) =>
            method.Edges
                  .Where(e => e.Type == EdgeType.Calls)
                  .Select(e => e.Target)
                  .OfType<MethodNode>();

        public static bool CallsMethod(this MethodNode method, string methodName) =>
            method.CalledMethods().Any(m => m.Symbol.Name == methodName);

        // ---------------------------------------------------------------------------------------------------------------
        // Event Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static bool HasEvent(this ClassNode cls) =>
            cls.Edges.Any(e => e.Type == EdgeType.HasEvent);

        public static IEnumerable<EventNode> GetEvents(this ClassNode cls) =>
            cls.Edges
               .Where(e => e.Type == EdgeType.HasEvent)
               .Select(e => e.Target)
               .OfType<EventNode>();

        // ---------------------------------------------------------------------------------------------------------------
        // Collection Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static IEnumerable<ITypeSymbol> GetCollectionElementTypes(this ClassNode cls, bool requireInterface = false)
        {
            var types = new List<ITypeSymbol>();

            foreach (var edge in cls.Edges
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
                    element = TypeHelper.GetInnerMostElementType(element);
                    if (element != null && (!requireInterface || element.TypeKind == TypeKind.Interface))
                        types.Add(element);
                }
            }

            return types.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default);
        }

        // ---------------------------------------------------------------------------------------------------------------
        // Symbol Relation Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static bool IsAssignableToAny(this ITypeSymbol type, IEnumerable<ITypeSymbol> targets)
        {
            foreach (var t in targets)
            {
                if (SymbolEqualityComparer.Default.Equals(type, t)) return true;
                if (type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, t))) return true;
            }
            return false;
        }

        public static IEnumerable<INamedTypeSymbol> GetImplementingTypes(this MethodNode method)
        {
            return method.Edges
                .Where(e => e.Type == EdgeType.ImplementedBy && e.Target is MethodNode)
                .Select(e => (e.Target as MethodNode)?.Symbol.ContainingType)
                .OfType<INamedTypeSymbol>();
        }

        public static bool ImplementsOrInherits(this INamedTypeSymbol? type, INamedTypeSymbol target)
        {
            if (type == null) return false;
            return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, target)) ||
                   SymbolEqualityComparer.Default.Equals(type.BaseType, target);
        }

        // ---------------------------------------------------------------------------------------------------------------
        // Singleton-Specific Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static bool HasMethodWithParameterAssignableTo(this ClassNode cls, IEnumerable<ITypeSymbol> targetTypes) =>
            cls.GetMethods()
               .Any(m => m.Symbol.Parameters.Any(p => p.Type.IsAssignableToAny(targetTypes)));

        public static bool HasStaticFieldOfOwnType(this ClassNode cls) =>
            cls.GetFields().Any(f =>
                f.Symbol.IsStatic &&
                SymbolEqualityComparer.Default.Equals(f.Symbol.Type, cls.Symbol));

        public static bool HasLazyStaticFieldOfOwnType(this ClassNode cls) =>
            cls.GetFields().Any(f =>
                f.Symbol.IsStatic &&
                f.Symbol.Type is INamedTypeSymbol named &&
                named.ConstructedFrom?.ToString() == "System.Lazy<T>" &&
                SymbolEqualityComparer.Default.Equals(named.TypeArguments[0], cls.Symbol));

        public static bool HasNestedStaticHolderOfOwnType(this ClassNode cls) =>
            cls.Symbol.GetTypeMembers()
                .SelectMany(nested => nested.GetMembers().OfType<IFieldSymbol>())
                .Any(f => f.IsStatic &&
                          SymbolEqualityComparer.Default.Equals(f.Type, cls.Symbol));

        public static bool HasGenericStaticFieldOfOwnType(this ClassNode cls)
        {
            var typeParams = cls.Symbol.TypeParameters;
            return cls.GetFields().Any(f =>
                typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(f.Symbol.Type, tp)));
        }

        public static bool HasStaticPropertyOfOwnType(this ClassNode cls)
        {
            var typeParams = cls.Symbol.TypeParameters;
            return cls.GetProperties().Any(p =>
                p.Symbol.IsStatic &&
                (SymbolEqualityComparer.Default.Equals(p.Symbol.Type, cls.Symbol) ||
                 typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(p.Symbol.Type, tp))));
        }

        public static bool HasStaticMethodOfOwnType(this ClassNode cls)
        {
            var typeParams = cls.Symbol.TypeParameters;
            return cls.GetMethods().Any(m =>
                m.Symbol.IsStatic &&
                (SymbolEqualityComparer.Default.Equals(m.Symbol.ReturnType, cls.Symbol) ||
                 typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(m.Symbol.ReturnType, tp))));
        }

        public static bool HasStaticFieldAccessorOfOwnType(this ClassNode cls)
        {
            var typeParams = cls.Symbol.TypeParameters;
            return cls.GetFields().Any(f =>
                (f.Symbol.DeclaredAccessibility == Accessibility.Public ||
                 f.Symbol.DeclaredAccessibility == Accessibility.Internal) &&
                (SymbolEqualityComparer.Default.Equals(f.Symbol.Type, cls.Symbol) ||
                 typeParams.Any(tp => SymbolEqualityComparer.Default.Equals(f.Symbol.Type, tp))));
        }

        // ---------------------------------------------------------------------------------------------------------------
        // Held Types / Injection Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static INamedTypeSymbol? ResolveMemberType(this INode node) =>
            node switch
            {
                FieldNode f => f.Symbol.Type as INamedTypeSymbol,
                PropertyNode p => p.Symbol.Type as INamedTypeSymbol,
                _ => null
            };

        public static IEnumerable<ITypeSymbol> GetHeldTypes(this ClassNode cls) =>
            cls.GetFields().Select(f => f.Symbol.Type)
               .Concat(cls.GetProperties().Select(p => p.Symbol.Type));

        public static bool HasInjectionOf(this ClassNode cls, INamedTypeSymbol type) =>
            cls.GetConstructors().Any(ctor =>
                ctor.Symbol.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, type)))
            || cls.GetProperties().Any(p =>
                SymbolEqualityComparer.Default.Equals(p.Symbol.Type, type));

        // ---------------------------------------------------------------------------------------------------------------
        // Implementor Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static IEnumerable<ClassNode> GetImplementorsOf(this INamedTypeSymbol target, NodeRegistry registry)
        {
            return registry.GetAll<ClassNode>()
                .Select(c => new { Node = c, Type = c.Symbol as INamedTypeSymbol })
                .Where(x => x.Type != null &&
                            (x.Type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, target)) ||
                             SymbolEqualityComparer.Default.Equals(x.Type.BaseType, target)))
                .Select(x => x.Node);
        }

        // ---------------------------------------------------------------------------------------------------------------
        // Adapter-Specific Queries
        // ---------------------------------------------------------------------------------------------------------------

        public static bool DelegatesToType(
            this ClassNode adapter,
            INamedTypeSymbol adapteeType,
            IEnumerable<IMethodSymbol> ifaceMethods,
            NodeRegistry registry)
        {
            return ifaceMethods.Any(ifaceMethod =>
            {
                var impl = adapter.Symbol.FindImplementationForInterfaceMember(ifaceMethod) as IMethodSymbol;
                if (impl == null) return false;

                var implNode = registry.GetNode<MethodNode>(impl);
                return implNode?.CalledMethods()
                               .Any(m => SymbolEqualityComparer.Default.Equals(m.Symbol.ContainingType, adapteeType))
                       ?? false;
            });
        }

        // ---------------------------------------------------------------------------------------------------------------
        // Strategy/State-Specific Queries
        // ---------------------------------------------------------------------------------------------------------------

        private static bool MatchesTypeOrElement(ITypeSymbol candidate, INamedTypeSymbol target) =>
            SymbolEqualityComparer.Default.Equals(TypeHelper.GetElementType(candidate) ?? candidate, target);

        public static bool HoldsReferenceTo(this ClassNode node, INamedTypeSymbol type) =>
            node.GetHeldTypes().Any(t => MatchesTypeOrElement(t, type)) || node.HasInjectionOf(type);

        public static bool DelegatesTo(this ClassNode context, INamedTypeSymbol strategy, NodeRegistry registry) =>
            context.GetMethods()
                   .Any(m => m.CalledMethods()
                              .Any(c => MatchesTypeOrElement(c.Symbol.ContainingType, strategy)));

        public static bool HasStateSwitchMethod(this ClassNode ctx, INamedTypeSymbol strategy) =>
            ctx.GetMethods().Any(m =>
                m.Symbol.Name.Contains("State", StringComparison.OrdinalIgnoreCase) ||
                m.Symbol.Parameters.Any(p => MatchesTypeOrElement(p.Type, strategy)));

    }
}
