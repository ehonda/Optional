# Optional

A lightweight, type-safe implementation of the Option pattern for .NET.

## Overview

`Option<T>` represents an optional value that can be in one of two states:

- **Some**: Contains a value (which can be `null` if `T` is nullable).
- **None**: Contains no value.

This distinction is particularly useful when you need to differentiate between a value that was never specified (None) and a value that was explicitly set to null (Some(null)).

## Quickstart

```csharp
using EHonda.Optional.Core;

// Create options
Option<string> some = Option.Some("hello");
Option<string> none = Option.None<string>();
Option<string> implicitSome = "hello";
Option<string> defaultNone = default;

// Check state
if (some.HasValue) 
{
    Console.WriteLine(some.Value);
}

// Retrieve values with fallbacks
string v1 = some.Or("fallback"); // returns "hello"
string v2 = none.Or("fallback"); // returns "fallback"
```

## Motivation: The "Explicit Null" Use Case

This library was built to solve a specific problem in test infrastructure: distinguishing between "use the default value" and "use null".

Consider a helper method for creating a service in a test. You want parameters to be optional so tests only specify what's relevant.

Without `Option<T>`, using a nullable parameter makes it impossible to distinguish "unspecified" from "explicit null":

```csharp
// Problem: Can't distinguish between default (null) and explicit null
IService CreateService(IDependency? dependency = null) 
    => new(dependency ?? CreateDependency());

// Both calls look the same to the method:
CreateService();      // Intention: Use default dependency
CreateService(null);  // Intention: Use null dependency (e.g. for null guard tests)
```

With `Option<T>`, `None` is distinct from `null`:

```csharp
// Solution: Option<T> distinguishes between None and Some(null)
IService CreateService(Option<IDependency> dependency = default) 
    => new(dependency.Or(CreateDependency()));

// Now the behavior is clear:
CreateService();      // dependency is None -> uses CreateDependency()
CreateService(null);  // dependency is Some(null) -> uses null
```
