using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.GraphService.Helpers
{
    public static class UniqueEdgeAdder
    {
        public static void AddUniqueEdge(INode source, EdgeNode edge)
        {
            if (!source.OutgoingEdges.Any(e => e.Type == edge.Type && e.Target == edge.Target))
            {
                source.OutgoingEdges.Add(edge);
            }
        }
    }
}