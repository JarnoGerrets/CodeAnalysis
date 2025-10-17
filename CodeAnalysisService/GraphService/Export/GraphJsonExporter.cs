using System.Linq;
using System.Text.Json;
using CodeAnalysisService.GraphService.Context;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.Export
{
    public static class GraphJsonExporter
    {
        public static void Export(NodeRegistry registry, string path)
        {
            string MakeId(INode n)
            {
                var sym = n.Symbol;
                if (sym != null)
                    return sym.ToDisplayString();

                return $"{n.GetType().Name}:{n.GetHashCode()}";
            }

            string? MakeParentId(INode n)
            {
                switch (n)
                {
                    case MethodNode m:
                        return m.Symbol?.ContainingType?.ToDisplayString();
                    case PropertyNode p:
                        return p.Symbol?.ContainingType?.ToDisplayString();
                    case FieldNode f:
                        return f.Symbol?.ContainingType?.ToDisplayString();
                    case EventNode e:
                        return e.Symbol?.ContainingType?.ToDisplayString();
                    default:
                        return null;
                }
            }

            var nodeRecords = registry.GetAll<INode>().Select(n =>
            {
                var id = MakeId(n);
                var label = n switch
                {
                    ClassNode c      => c.Symbol.Name,
                    InterfaceNode i  => i.Symbol.Name,
                    MethodNode m     => $"{m.Symbol.ContainingType?.Name}.{m.Symbol.Name}",
                    PropertyNode p   => $"{p.Symbol.ContainingType?.Name}.{p.Symbol.Name}",
                    FieldNode f      => $"{f.Symbol.ContainingType?.Name}.{f.Symbol.Name}",
                    EventNode e      => $"{e.Symbol.ContainingType?.Name}.{e.Symbol.Name}",
                    _                => n.GetType().Name
                };

                var type = n switch
                {
                    ClassNode      => "Class",
                    InterfaceNode  => "Interface",
                    MethodNode     => "Method",
                    PropertyNode   => "Property",
                    FieldNode      => "Field",
                    EventNode      => "Event",
                    _              => "Other"
                };

                var parent = MakeParentId(n);

                return new { id, label, type };
            }).ToList();

            var edgeRecords = registry.GetAll<INode>()
                .SelectMany(n => n.Edges.Select(e => new
                {
                    source = MakeId(n),
                    target = MakeId(e.Target),
                    label  = e.Type.ToString(),
                    type   = e.Type.ToString()
                }))
                .ToList();

            var graph = new { nodes = nodeRecords, edges = edgeRecords };

            var json = JsonSerializer.Serialize(graph, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
