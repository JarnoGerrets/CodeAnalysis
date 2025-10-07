using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace CodeAnalysisService.GraphService.Helpers
{
    public class CallResolver
    {
        private readonly Dictionary<IMethodSymbol, List<MethodNode>> _interfaceToImplementations  = new(SymbolEqualityComparer.Default);
        private readonly ConcurrentDictionary<IMethodSymbol, IEnumerable<MethodNode>> _lookupCache  = new(SymbolEqualityComparer.Default);

        public CallResolver(IEnumerable<ClassNode> classNodes, IEnumerable<MethodNode> methodNodes)
        {
            BuildInterfaceImplementationMap(classNodes, methodNodes);
        }

        private void BuildInterfaceImplementationMap(IEnumerable<ClassNode> classNodes, IEnumerable<MethodNode> methodNodes)
        {
            var methodLookup = new Dictionary<ISymbol, MethodNode>(SymbolEqualityComparer.Default);
            foreach (var m in methodNodes)
                methodLookup[m.Symbol] = m;

            foreach (var classNode in classNodes)
            {
                var classSymbol = classNode.Symbol;

                foreach (var methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    if (methodLookup.TryGetValue(methodSymbol, out var methodNode))
                    {
                        foreach (var iface in classSymbol.AllInterfaces)
                        {
                            foreach (var ifaceMethod in iface.GetMembers().OfType<IMethodSymbol>())
                            {
                                var impl = classSymbol.FindImplementationForInterfaceMember(ifaceMethod);
                                if (SymbolEqualityComparer.Default.Equals(impl, methodSymbol))
                                {
                                    if (!_interfaceToImplementations.TryGetValue(ifaceMethod, out var list))
                                    {
                                        list = new List<MethodNode>();
                                        _interfaceToImplementations[ifaceMethod] = list;
                                    }
                                    list.Add(methodNode);
                                }
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<MethodNode> GetImplementations(IMethodSymbol ifaceMethod)
        {
        return _lookupCache.GetOrAdd(ifaceMethod, _ =>
        {
            if (_interfaceToImplementations.TryGetValue(ifaceMethod, out var impls)) return impls;
            return Array.Empty<MethodNode>();
        });
        }
    }
}
