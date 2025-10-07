namespace CodeAnalysisService.Enums
{
    /// <summary>
    /// Defines the types of edges in the code analysis graph, representing
    /// relationships and interactions between nodes such as classes, methods,
    /// fields, and properties.
    /// </summary>
    public enum EdgeType
    {
        // Class-level relations
        Inherits,           // class derives from another class
        Implements,         // class implements an interface
        Aggregates,         // class has a field/property of another type
        Uses,               // class uses another class (type usage)
        Composes,           // aggregates multiple objects (composition)
        Wraps,              // wraps another object (decorator pattern)
        Notifies,           // class raises/raises events to another class
        DependsOn,          // general dependency
        HasConstructor,     // class has a constructor of a type
        HasMethod,          // class has a method of a type
        HasFieldElement,    // class has a field that is a collection of a type
        HasPropertyElement, // class has a property that is a collection of a type
        HasEvent,           // class has an event that it sends or subscribes to

        // Method-level relations
        Calls,              // method calls another method
        Overrides,          // method overrides a base method
        Returns,            // method returns an instance of another type
        Creates,            // method constructs a new object
        SubscribesTo,       // method subscribes to events of another type
        HandlesEvent,       // method handles events from another type
        DelegatesTo,        // method delegates to another object (via delegate)
        ConfirmsContract,   // ensures a method adheres to an interface or template
        ImplementedBy,      // method is an implementation of an interface method

        // Field/property relations
        HasField,           // class has a field of a type
        HasProperty,        // class has a property of a type
        ReferencesField,    // method or property references a specific field
        ReferencesProperty, // method or property references another property

        // Optional/advanced
        Observes,           // observes another object (observer pattern)
        RaisesEvent,        // method or property raises an event
    }
}
