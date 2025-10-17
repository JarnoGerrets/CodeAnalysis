using Microsoft.CodeAnalysis;
using CodeAnalysisService.GraphService.Nodes;
using CodeAnalysisService.GraphService.Context;

namespace CodeAnalysisService.GraphService.Printer
{
    public class GraphPrinter
    {
        private readonly string _outputPath;
        private readonly List<string> _lines = new();

        public GraphPrinter(string outputDirectory, string fileName = "graph-output.txt")
        {
            Directory.CreateDirectory(outputDirectory);
            _outputPath = Path.Combine(outputDirectory, fileName);
        }

        public void PrintGraph(NodeRegistry registry)
        {
            var visited = new HashSet<INode>();

            _lines.Add("--------------------------------------------------");
            _lines.Add("Graph Debug Output");
            _lines.Add("--------------------------------------------------");

            foreach (var cls in registry.GetAll<ClassNode>())
                PrintNode(cls, "", visited);

            foreach (var iface in registry.GetAll<InterfaceNode>())
                PrintNode(iface, "", visited);

            foreach (var meth in registry.GetAll<MethodNode>())
                PrintNode(meth, "", visited);

            foreach (var evt in registry.GetAll<EventNode>())
                PrintNode(evt, "", visited);

            File.WriteAllLines(_outputPath, _lines);
        }

        // ---------- Node formatters ----------

        private static string FormatNodeHeader(INode node) => node switch
        {
            ClassNode c => $"Class: {c.Symbol.Name} (Abstract={c.Symbol.IsAbstract})",
            InterfaceNode i => $"Interface: {i.Symbol.Name}",
            ConstructorNode k => $"Constructor: {k.Symbol.ContainingType.Name}..ctor",
            MethodNode m => $"Method: {m.Symbol.Name} (Returns {m.Symbol.ReturnType.Name}, Abstract={m.Symbol.IsAbstract})",
            PropertyNode p => $"Property: {p.Symbol.Name}",
            FieldNode f => FormatFieldHeader(f),
            EventNode e => $"Event: {e.Symbol.Name} (Type={e.Symbol.Type.Name})",
            _ => $"Unknown node: {node?.GetType().Name}"
        };

        private static string FormatFieldHeader(FieldNode f)
        {
            if (f.Symbol is not IFieldSymbol fs) return $"Field: {f.Symbol?.Name}";
            var mods = string.Join(" ", new[]
            {
                AccessibilityToString(fs.DeclaredAccessibility),
                fs.IsConst ? "const" : null,
                fs.IsStatic && !fs.IsConst ? "static" : null,
                fs.IsReadOnly && !fs.IsConst ? "readonly" : null
            }.Where(s => !string.IsNullOrEmpty(s)));

            return $"Field: {fs.ContainingType.Name}.{fs.Name} ({mods} {fs.Type.Name})";
        }

        private static string AccessibilityToString(Accessibility a) => a switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.ProtectedOrInternal => "private protected",
            _ => ""
        };

        // Short names for edges
        private static string FormatNodeShort(INode node) => node switch
        {
            ClassNode c => $"Class: {c.Symbol.Name}",
            InterfaceNode i => $"Interface: {i.Symbol.Name}",
            ConstructorNode k => $"Constructor: {k.Symbol.ContainingType.Name}..ctor",
            MethodNode m => $"Method: {m.Symbol.Name}",
            PropertyNode p => $"Property: {p.Symbol.Name}",
            FieldNode f => f.Symbol is IFieldSymbol fs ? $"Field: {fs.Name}" : $"Field: {f.Symbol?.Name}",
            EventNode e => $"Event: {e.Symbol.Name}",
            _ => "Unknown"
        };

        // ---------- Printing ----------

        private void PrintNode(INode node, string indent, HashSet<INode> visited)
        {
            if (node == null || visited.Contains(node)) return;
            visited.Add(node);

            // Print detailed header
            _lines.Add($"{indent}{FormatNodeHeader(node)}");

            foreach (var edge in node.Edges)
                PrintEdge(node, edge, indent + "  ", visited);
        }

        private void PrintEdge(INode source, EdgeNode edge, string indent, HashSet<INode> visited)
        {
            string sourceShort = FormatNodeShort(source);
            string targetShort = FormatNodeShort(edge.Target);

            _lines.Add($"{indent}{sourceShort} --[{edge.Type}]--> {targetShort}");

            PrintNode(edge.Target, indent + "  ", visited);
        }
    }
}
