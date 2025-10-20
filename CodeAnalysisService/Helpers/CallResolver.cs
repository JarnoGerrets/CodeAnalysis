using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Concurrent;

namespace CodeAnalysisService.GraphService.Helpers
{
    /// <summary>
    /// Maps interface methods to their concrete class implementations.
    /// </summary>
    public class CallResolver
    {
        private readonly ConcurrentDictionary<IMethodSymbol, List<MethodNode>> _interfaceToImplementations = new(SymbolEqualityComparer.Default);

        private readonly ConcurrentDictionary<IMethodSymbol, IEnumerable<MethodNode>> _lookupCache = new(SymbolEqualityComparer.Default);

        public CallResolver(IEnumerable<ClassNode> classNodes, IEnumerable<MethodNode> methodNodes)
        {
            BuildImplementationMap(classNodes, methodNodes);
        }

        private void BuildImplementationMap(IEnumerable<ClassNode> classNodes, IEnumerable<MethodNode> methodNodes)
        {
            var methodLookup = BuildMethodLookup(methodNodes);

            Parallel.ForEach(classNodes, classNode =>
            {
                if (classNode.Symbol.AllInterfaces.Length == 0)
                    return;

                var classImpls = ResolveImplementationsForClass(classNode.Symbol, methodLookup);
                MergeImplementations(classImpls);
            });
        }

        private static Dictionary<ISymbol, MethodNode> BuildMethodLookup(IEnumerable<MethodNode> methodNodes)
        {
            var lookup = new Dictionary<ISymbol, MethodNode>(SymbolEqualityComparer.Default);
            foreach (var m in methodNodes)
                lookup[m.Symbol] = m;
            return lookup;
        }

        private static Dictionary<IMethodSymbol, List<MethodNode>> ResolveImplementationsForClass(
            INamedTypeSymbol classSymbol,
            Dictionary<ISymbol, MethodNode> methodLookup)
        {
            var result = new Dictionary<IMethodSymbol, List<MethodNode>>(SymbolEqualityComparer.Default);

            foreach (var methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (!methodLookup.TryGetValue(methodSymbol, out var methodNode))
                    continue;

                foreach (var iface in classSymbol.AllInterfaces)
                {
                    foreach (var ifaceMethod in iface.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>())
                    {
                        var impl = classSymbol.FindImplementationForInterfaceMember(ifaceMethod);
                        if (SymbolEqualityComparer.Default.Equals(impl, methodSymbol))
                            AddToList(result, ifaceMethod, methodNode);
                    }
                }
            }

            return result;
        }

        private void MergeImplementations(Dictionary<IMethodSymbol, List<MethodNode>> classImpls)
        {
            foreach (var (ifaceMethod, methods) in classImpls)
            {
                _interfaceToImplementations.AddOrUpdate(
                    ifaceMethod,
                    methods,
                    (_, existing) =>
                    {
                        lock (existing)
                            existing.AddRange(methods);
                        return existing;
                    });
            }
        }


        public IEnumerable<MethodNode> GetImplementations(IMethodSymbol ifaceMethod)
        {
            return _lookupCache.GetOrAdd(ifaceMethod, _ =>
                _interfaceToImplementations.TryGetValue(ifaceMethod, out var impls) ? impls : Array.Empty<MethodNode>());
        }

        private static void AddToList<TKey, TValue>(
            Dictionary<TKey, List<TValue>> dict,
            TKey key,
            TValue value)
            where TKey : notnull
        {
            if (!dict.TryGetValue(key, out var list))
                dict[key] = list = new List<TValue>();
            list.Add(value);
        }
    }
}
