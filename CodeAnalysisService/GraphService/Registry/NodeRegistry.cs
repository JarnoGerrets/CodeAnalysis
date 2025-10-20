using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.Registry
{
    /// <summary>
    /// Central registry that maps Roslyn ISymbol to graph <see cref="INode"/>.
    /// </summary>
    public class NodeRegistry
    {
        private readonly Dictionary<ISymbol, INode> _nodes = new(SymbolEqualityComparer.Default);

        private readonly Dictionary<System.Type, List<INode>> _byType = new();

        public void AddNode(ISymbol symbol, INode node)
        {
            if (symbol == null || node == null) return;

            _nodes[symbol] = node;

            var allTypes = node.GetType().GetInterfaces().Concat(GetBaseTypes(node.GetType())).Append(node.GetType());

            foreach (var t in allTypes.Distinct())
            {
                if (!_byType.TryGetValue(t, out var list))
                {
                    list = new List<INode>();
                    _byType[t] = list;
                }
                list.Add(node);
            }
        }

        private IEnumerable<System.Type> GetBaseTypes(System.Type type)
        {
            while (type.BaseType != null && type.BaseType != typeof(object))
            {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }

        public T? GetNode<T>(ISymbol s) where T : class, INode
        {
            if (s == null) return null;
            return _nodes.TryGetValue(s, out var node) ? node as T : null;
        }

        public IEnumerable<T> GetAll<T>() where T : class, INode
        {
            if (_byType.TryGetValue(typeof(T), out var list))
            {
                foreach (var node in list)
                    yield return (T)node;
            }
            else
            {
                yield break;
            }
        }

    }
}
