using System;
using System.Collections.Generic;
using System.Linq;
using CodeAnalysisService.Enums;
using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.PatternAnalyser.Queries
{
    /// <summary>
    /// Common reusable graph queries to simplify pattern checks.
    /// Instead of repeating Roslyn/graph traversals in every Check class,
    /// we centralize them here and compose them for different patterns.
    /// </summary>
    public static class GraphQueries
    {
        // ---- CLASS LEVEL QUERIES ----

        public static bool HasMethod(this ClassNode cls, string methodName) =>
            cls.OutgoingEdges.Any(e =>
                e.Type == EdgeType.HasMethod &&
                e.Target is MethodNode m &&
                string.Equals(m.Symbol.Name, methodName, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<MethodNode> GetMethods(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.HasMethod)
               .Select(e => e.Target)
               .OfType<MethodNode>();

        public static bool HasProperty(this ClassNode cls, string propertyName) =>
            cls.OutgoingEdges.Any(e =>
                e.Type == EdgeType.HasProperty &&
                e.Target is PropertyNode p &&
                string.Equals(p.Symbol.Name, propertyName, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<PropertyNode> GetProperties(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.HasProperty)
               .Select(e => e.Target)
               .OfType<PropertyNode>();

        public static bool ImplementsInterface(this ClassNode cls, string interfaceName) =>
            cls.OutgoingEdges.Any(e =>
                e.Type == EdgeType.Implements &&
                e.Target is InterfaceNode i &&
                i.Symbol.Name == interfaceName);

        public static IEnumerable<InterfaceNode> GetInterfaces(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.Implements)
               .Select(e => e.Target)
               .OfType<InterfaceNode>();

        public static bool InheritsFrom(this ClassNode cls, string baseClassName) =>
            cls.OutgoingEdges.Any(e =>
                e.Type == EdgeType.Inherits &&
                e.Target is ClassNode baseCls &&
                baseCls.Symbol.Name == baseClassName);

        // ---- FIELD QUERIES ----

        public static IEnumerable<FieldNode> GetFields(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.HasField)
               .Select(e => e.Target)
               .OfType<FieldNode>();

        public static bool HasFieldOfType(this ClassNode cls, string typeName) =>
            cls.GetFields().Any(f => f.Symbol.Type.Name == typeName);

        public static bool HasStaticFieldOfType(this ClassNode cls, string typeName) =>
            cls.GetFields().Any(f => f.Symbol.IsStatic && f.Symbol.Type.Name == typeName);

        // ---- CONSTRUCTOR QUERIES ----

        public static IEnumerable<ConstructorNode> GetConstructors(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.HasConstructor)
               .Select(e => e.Target)
               .OfType<ConstructorNode>();

        public static bool HasPrivateConstructor(this ClassNode cls) =>
            cls.GetConstructors().Any(c => c.Symbol.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private);

        // ---- METHOD CALL QUERIES ----

        public static IEnumerable<MethodNode> CalledMethods(this MethodNode method) =>
            method.OutgoingEdges
                  .Where(e => e.Type == EdgeType.Calls)
                  .Select(e => e.Target)
                  .OfType<MethodNode>();

        public static bool CallsMethod(this MethodNode method, string methodName) =>
            method.CalledMethods().Any(m => m.Symbol.Name == methodName);

        // ---- EVENT QUERIES ----

        public static bool HasEvent(this ClassNode cls) =>
            cls.OutgoingEdges.Any(e => e.Type == EdgeType.HasEvent);

        public static IEnumerable<EventNode> GetEvents(this ClassNode cls) =>
            cls.OutgoingEdges
               .Where(e => e.Type == EdgeType.HasEvent)
               .Select(e => e.Target)
               .OfType<EventNode>();
    }
}
