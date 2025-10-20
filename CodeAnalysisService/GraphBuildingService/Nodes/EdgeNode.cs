using CodeAnalysisService.Enums;


namespace CodeAnalysisService.GraphBuildingService.Nodes
{
    /// <summary>
    /// Represents a directed edge in the code graph.
    /// </summary>
    public class EdgeNode
    {
        public object SyncRoot { get; } = new object();
        public required INode Target { get; set; }
        public NodeType NodeType => Target.NodeType; 
        public required EdgeType Type { get; set; }
    }
}   