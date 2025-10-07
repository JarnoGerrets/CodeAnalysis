namespace CodeAnalysisService.Enums
{
    /// <summary>
    /// Defines the types of nodes in the code analysis graph, representing
    /// language constructs such as classes, interfaces, methods, constructors,
    /// properties, and fields.
    /// </summary>
    public enum NodeType
    {
        Interface,
        Class,
        Constructor,
        Method,
        Property,
        Field,
        Event
    }
}