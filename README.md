# Optional

A lightweight, type-safe implementation of the Option pattern for .NET.

## Overview

This library provides two types to represent optional values:

1. **`Option<T>`**: Represents an optional value that, if present, is **guaranteed to be non-null**.
2. **`NullableOption<T>`**: Represents an optional value that **can be null** when present.

Both types can be in one of two states:

- **Some**: Contains a value.
- **None**: Contains no value.

## Quickstart

```csharp
using EHonda.Optional.Core;

// --- Option<T> (Non-nullable) ---

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
    Console.WriteLine(some.Value); // No null check needed
}

// Retrieve values with fallbacks
string v1 = some.Or("fallback"); // returns "hello"
string v2 = none.Or("fallback"); // returns "fallback"

// --- NullableOption<T> (Nullable) ---

// Create options
NullableOption<string> someNull = NullableOption.Some<string>(null);
NullableOption<string> implicitNull = null;
NullableOption<string> fromOption = some; // Implicit conversion from Option<T>

// Accessing value (can be null)
if (someNull.HasValue)
{
    Console.WriteLine(someNull.Value ?? "null");
}

// Retrieve values
string? v3 = someNull.Or("fallback"); // returns null (because it is Some(null))
string? v4 = implicitNull.Or("fallback"); // returns null
```

## Motivation & Use Cases

### Business Logic: `Option<T>`

In domain logic, you often want to avoid `null` entirely. `Option<T>` guarantees that if a value is present, it is not null. This removes the need for defensive null checks when accessing the underlying value.

```csharp
// Guaranteed non-null access
var value = option.Value; 
var s = option.Or(new S()); // No need for ?? new S()
```

### Test Infrastructure: `NullableOption<T>`

In testing scenarios, you often need to distinguish between "use the default value" and "explicitly use null" (e.g., to test null guards).

Without `NullableOption<T>`, using a nullable parameter makes it impossible to distinguish "unspecified" from "explicit null":

```csharp
// Problem: Can't distinguish between default (null) and explicit null
IService CreateService(IDependency? dependency = null) 
    => new Service(dependency ?? CreateDependency());
```

With `NullableOption<T>`, `None` is distinct from `Some(null)`:

```csharp
// Solution: NullableOption<T> distinguishes between None and Some(null)
IService CreateService(NullableOption<IDependency> dependency = default) 
    => new Service(dependency.Or(CreateDependency));

// Usage:
CreateService();      // dependency is None -> uses CreateDependency()
CreateService(null);  // dependency is Some(null) -> uses null (verifies null guard)
```

## Implicit Conversions

`Option<T>` supports implicit conversions from `T` (non-null).
`NullableOption<T>` supports implicit conversions from `T` (nullable) and `Option<T>`.

```csharp
Option<int> some = 42;             // Some(42)
// Option<string> fail = null;     // ❌ Throws ArgumentNullException

NullableOption<string> n1 = "hello"; // Some("hello")
NullableOption<string> n2 = null;    // Some(null)
NullableOption<string> n3 = some;    // Some(42)
```

## Explicit Conversions

Both types support explicit conversions to retrieve their values or convert between types.

```csharp
// Option<T> -> T
Option<int> some = 42;
int val = (int)some; // Returns 42
// int val2 = (int)Option.None<int>(); // ❌ Throws InvalidOperationException

// NullableOption<T> -> T?
NullableOption<int> nSome = 42;
int? nVal = (int?)nSome; // Returns 42

// NullableOption<T> -> Option<T>
NullableOption<int> nOpt = 42;
Option<int> opt = (Option<int>)nOpt; // Returns Some(42)

NullableOption<string> nNull = null;
// Option<string> opt2 = (Option<string>)nNull; // ❌ Throws InvalidOperationException (cannot contain null)
```

### ⚠️ Limitations with Interfaces

You will encounter a compiler error when implicitly converting an interface variable to `Option<Interface>` or `NullableOption<Interface>`.

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

**Workaround:** Use `Option.Some()` or `NullableOption.Some()` explicitly:

```csharp
Option<IService> fixed = Option.Some(interfaceRef);
NullableOption<IService> fixedNull = NullableOption.Some(interfaceRef);
```
