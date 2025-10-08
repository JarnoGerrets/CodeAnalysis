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

## How the graph is built

The process of building the graph always follows the same steps:

### Step 1: Collect syntax and semantics
The `GraphBuilder` is given a Roslyn `Compilation` and a set of `SemanticModel`s.  
These give us both the structure of the code (syntax trees) and its meaning (symbols, types, references).

### Step 2: Create nodes
Specialized node builders run over the syntax trees and create nodes for all relevant program elements.  
A `ClassNode` represents a class declaration, a `MethodNode` represents a method, and so on.  
Each node carries its Roslyn `ISymbol` for identity, and has a place to store outgoing edges.

### Step 3: Connect edges
Once all nodes exist, edge builders run. Each edge builder knows how to connect a particular kind of node to others.  
For example, the `ClassEdgeBuilder` adds inheritance edges and “has-method” edges, while the `MethodEdgeBuilder`  
adds “calls” and “uses” edges. This step is parallelized for speed but uses locking to keep node edge lists consistent.

### Step 4: Store results
All nodes are stored in a central `NodeRegistry`. The registry allows analyzers and tools to quickly find  
“all methods,” “all classes,” or “all nodes of a certain type.” It also maps Roslyn symbols to their corresponding graph nodes.

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
var builder = new GraphBuilder(compilation, semanticModels);
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

All of these checks are much easier on a graph than by inspecting raw syntax.

---

## Example: Observer pattern in the graph

Take this code:
```csharp
public interface IObserver { void Update(); }

public class WeatherStation {
    public event Action OnChange;
    public void Notify() => OnChange?.Invoke();
}

public class PhoneDisplay : IObserver {
    public void Update() => Console.WriteLine("Phone updated");
}
```
The GraphService produces:

- A ClassNode for WeatherStation with a HasEvent edge to an EventNode (OnChange)
- A MethodNode for Notify with a Calls edge to the event invocation
- A ClassNode for PhoneDisplay with an Implements edge to the IObserver interface
- A MethodNode for Update with an Overrides edge to the interface method

The PatternAnalyser can then recognize the Observer structure because all necessary relationships are already in the graph.

---

## Extending the service

The GraphService is meant to be extensible. If you need to capture new concepts, you can:

- Add a new node builder to introduce new node types (e.g. delegate nodes).
- Add a new edge builder to track new relationships (e.g. async/await edges, LINQ usage).
- Register your builder in GraphBuilder.

Once registered, your builders will automatically participate when BuildGraph() runs.

---

## Summary

The GraphService is the translation layer between Roslyn and higher-level analysis.
It gives you a semantic graph of your code that is easy to query, print, or export.
Pattern detection, architecture checks, and visualizations are all built on top of this service.