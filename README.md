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

// Creation from interfaces
IService service = new Service();
Option<IService> interfaceSome = Option.Some(service); // ✅ Use Some for interfaces
// Option<IService> interfaceImplicit = service; // ❌ Does not compile (see Limitations below)

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
    => new Service(dependency ?? CreateDependency());

// Both calls look the same to the method:
CreateService();      // Intention: Use default dependency
CreateService(null);  // Intention: Use null dependency (e.g. for null guard tests)
```

With `Option<T>`, `None` is distinct from `null`:

```csharp
// Solution: Option<T> distinguishes between None and Some(null)
IService CreateService(Option<IDependency> dependency = default) 
    => new Service(dependency.Or(CreateDependency()));

// Now the behavior is clear:
CreateService();      // dependency is None -> uses CreateDependency()
CreateService(null);  // dependency is Some(null) -> uses null
```

## Implicit Conversions

`Option<T>` supports implicit conversions from `T` to `Option<T>`, enabling natural syntax:

```csharp
Option<int> some = 42;             // Some(42)
Option<string?> someNull = null;   // Some(null)
```

### ⚠️ Limitations with Interfaces

You will encounter a compiler error when implicitly converting an interface variable to `Option<Interface>`.

```csharp
public interface IService { }
public class Service : IService { }

Service service = new Service();
Option<IService> works = service;      // ✅ Works

IService interfaceRef = new Service();
Option<IService> fails = interfaceRef; // ❌ Compiler Error
```

This is due to how the C# compiler resolves [User-defined implicit conversions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/conversions#1054-user-defined-implicit-conversions) (Section 10.5.4).

The compiler looks for conversion operators that convert from a type **encompassing** the source type. However, the definition of "encompassing" in [Evaluation of user-defined conversions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/conversions#1053-evaluation-of-user-defined-conversions) (Section 10.5.3) explicitly excludes interfaces:

> "If a standard implicit conversion ... exists from a type A to a type B, and if **neither A nor B are interface_types**, then A is said to be encompassed by B"

Since `IService` is an interface, it is not considered to be encompassed by the operator's parameter type, so the conversion is not found.

**Workaround:** Use `Option.Some()` explicitly:

```csharp
Option<IService> fixed = Option.Some(interfaceRef);
```
