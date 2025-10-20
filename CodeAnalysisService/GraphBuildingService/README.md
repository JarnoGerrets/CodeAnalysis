# GraphService

The GraphService is the part of CodeAnalysis that translates C# source code into a graph model.  
Instead of working directly with Roslyn syntax trees, we work with a graph made up of **nodes**  
(classes, methods, properties, etc.) and **edges** (inheritance, calls, ownership, etc.).

This model is easier to query and reason about, and it forms the foundation for the pattern analysis  
layer, which can detect common design patterns in a project.

---

## Table of Contents
- [How the graph is built](#how-the-graph-is-built)
- [Working with the graph](#working-with-the-graph)
- [Why a graph?](#why-a-graph)
- [Example: Observer pattern in the graph](#example-observer-pattern-in-the-graph)
- [Extending the service](#extending-the-service)
- [Summary](#summary)

---

## Design goal

The main goal of the GraphService is to minimize direct Roslyn queries. 
Roslyn provides powerful syntax and semantic analysis, but repeated queries are expensive and slow.
Instead, the graph is built once and contains all relevant relationships, making it much faster to traverse. 
To avoid losing detail, each node still keeps references to its Roslyn syntax and symbol. 
This way, the analyzer can always dive down into full Roslyn data when needed, without re-running heavy queries.
The result is a graph that is fast to build, easy to understand, and complete: you can query high-level relationships directly from the graph, and still access every Roslyn detail if deeper inspection is required.

---

## How the graph is built

The process of building the graph always follows the same steps:

### Step 1: Collect syntax and semantics
The `GraphBuilder` is given a Dictionairy of `SyntaxTree`s and `SemanticModel`s.  
This give us both the structure of the code (syntax trees) and its meaning (symbols, types, references).

---

### Step 2: Create nodes
Specialized node builders run over the syntax trees and create nodes for all relevant program elements.  
A `ClassNode` represents a class declaration, a `MethodNode` represents a method, and so on.  
Each node carries its Roslyn `ISymbol` for identity, and has a place to store outgoing edges.

`ClassNodeBuilder` as example:

```csharp
    public IEnumerable<(ISymbol Symbol, INode Node)> BuildNode(SyntaxNode node, SemanticModel model)
    {
        if (node is not ClassDeclarationSyntax classDecl)
            yield break;

        if (model.GetDeclaredSymbol(classDecl) is INamedTypeSymbol symbol)
        {
            yield return (symbol, new ClassNode
            {
                ClassSyntax = classDecl,
                Symbol = symbol,
            });
        }

    }
```
The above shown `ClassNodeBuilder` retrieves the semantic `INamedTypeSymbol` using Roslyn’s `SemanticModel`.
If the symbol exists, it creates a `ClassNode` that records the syntax, symbol, and whether the class is abstract.
The (symbol, node) pair is then yielded back so the `GraphBuilder` can register it in the graph.

---

### Step 3: Connect edges
Once all nodes exist, edge builders run. Each edge builder knows how to connect a particular kind of node to others.  
For example, the `ClassEdgeBuilder` adds inheritance edges and “has-method” edges, while the `MethodEdgeBuilder`  
adds “calls” and “uses” edges. This step is parallelized for speed but uses locking to keep node edge lists consistent.

`FieldEdgeBuilder` as example:

```csharp
    public IEnumerable<EdgeNode> BuildEdges(INode node, NodeRegistry registry, SemanticModel model)
    {
        if (node is not FieldNode fieldNode) return Enumerable.Empty<EdgeNode>();

        var edges = new List<EdgeNode>();
        var fieldType = fieldNode.Symbol.Type as INamedTypeSymbol;
        if (fieldType == null) return edges;

        // Uses
        if (registry.GetNode<ClassNode>(fieldType) is ClassNode classNode)
        {
            edges.Add(new EdgeNode { Target = classNode, Type = EdgeType.Uses });
        }

        // Has Field Element
        if (TypeHelper.GetElementType(fieldType) is INamedTypeSymbol elemNamed)
        {
            var elemNode = registry.GetNode<ClassNode>(elemNamed);
            if (elemNode != null)
            {
                edges.Add(new EdgeNode { Target = elemNode, Type = EdgeType.HasFieldElement });
            }
        }

        return edges;
    }
```
The above shown `FieldEdgeBuilder` inspects `FieldNodes` to connect them to their types.
It adds a `Uses` edge when the field’s type matches a known class, and a `HasFieldElement` edge when the field is a collection or array type.
This allows the graph to capture both direct dependencies and element-type dependencies of fields.

---

### Step 4: Store results
All nodes are stored in a central `NodeRegistry`. The registry allows analyzers and tools to quickly find  
“all methods,” “all classes,” or “all nodes of a certain type.” It also maps Roslyn symbols to their corresponding graph nodes.

---

### Step 5: Use or export
Once the graph is complete, it can be used in different ways:

- The `PatternAnalyser` queries the graph to detect patterns.  
- The `GraphPrinter` produces a simple text view of the graph.  
- The `GraphJsonExporter` writes the graph to JSON, which can be opened in `viewer.html` for interactive exploration.

---

## Working with the graph

After building the graph, you usually interact with it through the `NodeRegistry`.

Here’s a minimal example:

```csharp
var builder = new GraphBuilder(semanticModels);
builder.BuildGraph();

var registry = builder.Registry;

// Example: list which methods call which
foreach (var method in registry.GetAll<MethodNode>())
{
    foreach (var edge in method.OutgoingEdges.Where(e => e.EdgeType == EdgeType.Calls))
    {
        Console.WriteLine($"{method.Symbol.Name} calls {edge.Target.Symbol.Name}");
    }
}
```
This shows how easy it is to ask architectural questions once the graph exists.
You don’t have to traverse syntax trees manually; you just query nodes and edges.

---

## Why a graph?

The reason we build a graph instead of working directly on Roslyn data is because patterns and architecture are relational.

For example:

- To detect an Observer, you need to know that one class has an event,
another class implements an interface, and certain methods call into the event.
- To detect a Singleton, you need to know that a class has a private constructor,
holds a static reference, and a property returns that reference.

All of these checks are much easier on a graph than by inspecting raw syntax and also much faster to perform.

---

## Example: Observer pattern in the graph

Take this code:
```csharp
//Subject
public class TrafficLight
{
    private List<IObserver1> _observers = new List<IObserver1>();

    public IEnumerable<IObserver1> Observers => _observers;

    public void Attach(IObserver1 observer) => _observers.Add(observer);

    public void NotifyChange(string signal)
    {
        foreach (var obs in Observers)
            obs.Update(signal);
    }
}

// Observer interface
public interface IObserver1
{
    void Update(string signal);
}
// Observer
public class CarDisplay : IObserver1
{
    public void Update(string signal) => Console.WriteLine($"CarDisplay: {signal}");
}
```
The GraphService produces:

- A `ClassNode` for `TrafficLight` with a `HasField` edge to its _observers list
- A `MethodNode` for `Attach()` with a `Calls`/`Uses` edge to add observers
- A `MethodNode` for `NotifyChange()` with `Calls` edges to the Update method on each observer
- An `InterfaceNode` for `IObserver1`
- A `ClassNode` for `CarDisplay` with an `Implements` edge to IObserver1
- A `MethodNode` for `Update()` with an `Overrides` edge to the interface method

From these relationships, the `PatternAnalyser` can recognize the Observer pattern:
a subject (`TrafficLight`) maintains references to observers, notifies them on change, and observer (`CarDisplay`) implement a common interface.

---

## Extending the service

The GraphService is meant to be extensible. If you need to capture new concepts, you can:

- Add a new node builder to introduce new node types (e.g. delegate nodes).
- Add a new edge builder to track new relationships (e.g. async/await edges, LINQ usage).
- Register your builder in `GraphBuilder`.

Once registered, your builders will automatically participate when `BuildGraph()` runs.

---

## Summary

The GraphService is the translation layer between Roslyn and higher-level analysis.
It gives you a semantic graph of your code that is easy to query, print, or export.
Pattern detection, architecture checks, and visualizations are all built on top of this service.