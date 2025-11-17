# Option<T> Type Specification

**Version:** 1.1  
**Date:** November 18, 2025  
**Project:** EHonda.Optional.Core  
**Status:** Implemented

---

## 1. Overview

The `Option<T>` type provides a type-safe way to represent optional values, distinguishing between "no value" (None) and "some value" (Some), including the ability to differentiate between an unspecified value and an explicit `null`.

### 1.1 Primary Use Case

Enable functions to distinguish between:
- **Unspecified/missing value** → Should use default
- **Explicit `null`** → Should NOT use default
- **Non-null value** → Use as-is

```csharp
// Example: API parameter that distinguishes unset from explicit null
void UpdateUser(Option<string?> name = default)
{
    // Unspecified (default): Don't update name field
    if (!name.HasValue) return;
    
    // Explicit null: Clear the name field
    // Non-null: Update to new value
    user.Name = name.Value;
}

// Usage:
UpdateUser();                          // Don't update (Option is None)
UpdateUser(Option.Some<string?>(null)); // Clear name (Option is Some(null))
UpdateUser(Option.Some("Alice"));       // Set to "Alice"
```

---

## 2. Type Definition

### 2.1 Declaration

```csharp
public readonly record struct Option<T>
```

**Rationale:**
- `readonly`: Ensures immutability, which aligns with functional programming principles
- `record`: Provides auto-generated value equality, `with` expressions, and `ToString()`
- `struct`: Enables non-nullable default parameters (`Option<T> o = default`) where `o` is never null

### 2.2 Core Properties

```csharp
public bool HasValue { get; }
public T? Value { get; }
```

**Design Decisions:**
- `HasValue`: Primary indicator of whether a value is present
- `Value`: Nullable to represent both None (where accessing is unsafe) and Some(null) cases
- Private constructor to enforce factory method usage

---

## 3. Default Behavior

### 3.1 Default State

```csharp
Option<T> o = default;  // Represents None
```

**Specification:**
- `default(Option<T>)` MUST represent None
- `o.HasValue` MUST be `false`
- `o.Value` MUST be `default(T)` (but should not be accessed when `HasValue` is `false`)

**Rationale:** Ergonomic API design - developers expect `default` in optional contexts to mean "not provided"

---

## 4. Factory Methods

### 4.1 Creating Some

```csharp
// Static factory method
Option<string?> some = Option.Some<string?>(value);

// Implicit conversion
Option<string> some = "hello";
```

### 4.2 Creating None

```csharp
// Generic static method
Option<string> none = Option.None<string>();

// Default
Option<string> none = default;
```

**Specification:**
- `Option.Some<T>(T value)`: Creates Option with value (even if value is `null`)
- `Option.None<T>()`: Explicitly creates None option

**Note:** C# does not allow both a generic method `None<T>()` and a property `None` with the same identifier, even though one has type parameters. Therefore, only the generic method is provided. In contexts where type inference is needed, use explicit type parameters or rely on `default`.

---

## 5. Nullable Type Parameter Support

### 5.1 Explicit Design Decision

**The type `Option<T>` MUST support nullable type parameters (e.g., `Option<string?>`).**

### 5.2 Semantic Distinction

```csharp
Option<string?> none = default;              // No value specified
Option<string?> someNull = Option.Some<string?>(null);  // Value specified as null
Option<string?> someValue = Option.Some<string?>("hi"); // Value specified as "hi"

none.HasValue      // false
someNull.HasValue  // true - distinguishes from none!
someValue.HasValue // true
```

### 5.3 Constraint Decision

**No generic constraint on `T`** - we explicitly allow nullable reference types and nullable value types.

**Rationale:** Core use case requires differentiating unspecified from explicit null.

---

## 6. Equality Semantics

### 6.1 Value Equality

**Use `record struct` default value equality semantics.**

```csharp
Option<int> a = Option.Some(5);
Option<int> b = Option.Some(5);
a == b  // true

Option<int> none1 = default;
Option<int> none2 = Option.None<int>();
none1 == none2  // true
```

### 6.2 Nullable Value Equality

```csharp
Option<string?> a = Option.Some<string?>(null);
Option<string?> b = Option.Some<string?>(null);
a == b  // true (both are Some(null))

Option<string?> none = default;
a == none  // false (Some(null) != None)
```

**Specification:**
- Two None options are equal
- Two Some options are equal if their values are equal (per `T`'s equality)
- Some and None are never equal
- `Option<T>` uses structural equality provided by `record struct`

---

## 7. Convenience Methods

### 7.1 Or (Immediate Value)

```csharp
public T Or(T defaultValue)
```

Returns the contained value if Some, otherwise returns `defaultValue`.

```csharp
Option<string> some = Option.Some("hello");
Option<string> none = default;

some.Or("default")  // "hello"
none.Or("default")  // "default"
```

### 7.2 Or (Lazy Factory)

```csharp
public T Or(Func<T> factory)
```

Returns the contained value if Some, otherwise invokes `factory` and returns its result.

```csharp
Option<string> none = default;
none.Or(() => ExpensiveOperation())  // Only calls ExpensiveOperation if None
```

**Rationale:** Deferred computation avoids unnecessary work when value exists.

### 7.3 OrDefault

```csharp
public T? OrDefault()
```

Returns the contained value if Some, otherwise returns `default(T)`.

```csharp
Option<int> some = Option.Some(5);
Option<int> none = default;

some.OrDefault()  // 5
none.OrDefault()  // 0
```

### 7.4 OrThrow

```csharp
public T OrThrow()
public T OrThrow(string message)
public T OrThrow(Func<Exception> exceptionFactory)
```

Returns the contained value if Some, otherwise throws an exception.

```csharp
Option<string> none = default;
none.OrThrow()  // throws InvalidOperationException
none.OrThrow("Value required")  // throws with custom message
none.OrThrow(() => new CustomException())  // throws custom exception
```

---

## 8. Conversions

### 8.1 Implicit Conversion from T

```csharp
public static implicit operator Option<T>(T value)
```

Enables natural Some construction:

```csharp
Option<string> opt = "hello";  // Implicitly creates Some("hello")
```

**Note:** For nullable `T`, this includes `null`:
```csharp
Option<string?> opt = null;  // Creates Some(null), NOT None
```

### 8.2 Explicit Conversion to T

```csharp
public static explicit operator T(Option<T> option)
```

Extracts value, throws if None:

```csharp
Option<string> some = Option.Some("hello");
string value = (string)some;  // "hello"

Option<string> none = default;
string value = (string)none;  // throws InvalidOperationException
```

---

## 9. Pattern Matching Support

### 9.1 Deconstruct Method

```csharp
public void Deconstruct(out bool hasValue, out T? value)
```

Enables positional pattern matching:

```csharp
Option<string> opt = GetOption();

var result = opt switch
{
    (true, var value) => $"Some: {value}",
    (false, _) => "None"
};

// Or with deconstruction
if (opt is (true, var value))
{
    Console.WriteLine($"Got value: {value}");
}
```

### 9.2 Property Pattern Matching

```csharp
Option<string> opt = GetOption();

var result = opt switch
{
    { HasValue: true, Value: var v } => $"Some: {v}",
    { HasValue: false } => "None"
};
```

---

## 10. JetBrains Annotations

### 10.1 Required Annotations

All methods returning values MUST be annotated with:
- `[Pure]`: Method has no side effects
- `[MustUseReturnValue]`: Caller should use the return value (where applicable)

Example:
```csharp
[Pure]
[MustUseReturnValue]
public T Or(T defaultValue)
```

### 10.2 Rationale

Enhances IDE support with warnings for:
- Unused return values
- Side-effect-free method calls
- Improved code analysis

---

## 11. XML Documentation

### 11.1 Requirements

All public APIs MUST include:
- `<summary>`: Brief description
- `<param>`: Description of each parameter
- `<returns>`: Description of return value
- `<exception>`: Documented exceptions
- `<example>`: Usage examples for primary methods
- `<remarks>`: Additional context, edge cases, nullable behavior

### 11.2 Example

```csharp
/// <summary>
/// Returns the contained value if present, otherwise returns <paramref name="defaultValue"/>.
/// </summary>
/// <param name="defaultValue">The value to return if this option is None.</param>
/// <returns>
/// The contained value if <see cref="HasValue"/> is <c>true</c>, otherwise <paramref name="defaultValue"/>.
/// </returns>
/// <example>
/// <code>
/// Option&lt;string&gt; some = Option.Some("hello");
/// Option&lt;string&gt; none = default;
/// 
/// Console.WriteLine(some.Or("default")); // Output: hello
/// Console.WriteLine(none.Or("default")); // Output: default
/// </code>
/// </example>
[Pure]
[MustUseReturnValue]
public T Or(T defaultValue)
```

---

## 12. Nullable Reference Type Annotations

### 12.1 Compiler Context

Project has `<Nullable>enable</Nullable>` - all nullable annotations are enforced.

### 12.2 Annotation Strategy

```csharp
public readonly record struct Option<T>
{
    public bool HasValue { get; }
    
    // Nullable because:
    // 1. None case: no value to return
    // 2. Some case with T = string?: we have null value
    public T? Value { get; }
    
    // Factory allows null for nullable T
    public static Option<T> Some(T value) { ... }
    
    // Or accepts T which may be nullable
    public T Or(T defaultValue) { ... }
}
```

**Note:** When `T` is non-nullable (e.g., `int`, `string`), the compiler ensures type safety. When `T` is nullable (e.g., `string?`), we explicitly support it.

---

## 13. Future Considerations (Out of Scope for Initial Implementation)

The following features are explicitly deferred:

### 13.1 Monadic Operations
- `Map<U>(Func<T, U> mapper)`
- `Bind<U>(Func<T, Option<U>> binder)`
- `Filter(Func<T, bool> predicate)`

### 13.2 Switch/Match Method
```csharp
U Switch<U>(Func<T, U> some, Func<U> none)
void Switch(Action<T> some, Action none)
```

### 13.3 LINQ Integration
- `SelectMany` for LINQ query syntax
- `Where` for filtering

### 13.4 Async Support
- `OrAsync(Func<Task<T>> factory)`
- `MapAsync<U>(Func<T, Task<U>> mapper)`

**Rationale:** Initial implementation focuses on core functionality. These features can be added incrementally based on user needs.

---

## 14. Open Questions (Resolved)

### 14.1 ~~Default behavior~~
**RESOLVED:** `default(Option<T>)` represents None.

### 14.2 ~~Nullable T support~~
**RESOLVED:** Explicitly supported - this is a core use case.

### 14.3 ~~Equality semantics~~
**RESOLVED:** Use `record struct` value equality.

### 14.4 ~~Lazy Or method~~
**RESOLVED:** Include `Or(Func<T> factory)` in initial implementation.

---

## 15. Design Principles

1. **Type Safety:** Leverage C# type system to prevent null reference exceptions
2. **Immutability:** All instances are immutable
3. **Explicitness:** Distinguish between None and Some(null)
4. **Ergonomics:** Natural syntax with implicit conversions and default support
5. **Interoperability:** Works seamlessly with nullable reference types
6. **Performance:** Value type (struct) avoids heap allocations for common scenarios
7. **Discoverability:** Rich IDE support via JetBrains annotations and XML docs

### 15.1 C# Language Limitations

During implementation, we discovered that C# does not allow a class to contain both a generic method `None<T>()` and a property `None` with the same identifier, even though one has type parameters. This is a compile-time error in C#. Therefore, only the generic method `Option.None<T>()` is provided. Users must use explicit type parameters where needed, or rely on `default(Option<T>)` for type-inferred None construction.

---

## 16. Implementation Checklist

- [x] Define `Option<T>` readonly record struct
- [x] Implement private constructor, `HasValue`, and `Value` properties
- [x] Create static factory class `Option` with `Some<T>()` and `None<T>()` methods
- [x] Implement `Or(T)`, `Or(Func<T>)`, `OrDefault()`, `OrThrow()` methods
- [x] Add implicit conversion from `T` to `Option<T>`
- [x] Add explicit conversion from `Option<T>` to `T`
- [x] Implement `Deconstruct` for pattern matching
- [x] Apply JetBrains annotations (`[Pure]`, `[MustUseReturnValue]`)
- [x] Add comprehensive XML documentation with examples
- [x] Ensure nullable reference type annotations are correct
- [ ] Write unit tests covering all scenarios
- [x] Document nullable `T` behavior in remarks

---

## 17. References

- **Project:** EHonda.Optional.Core v0.1.0
- **Target Framework:** .NET 10.0
- **Language Version:** C# 13
- **Dependencies:** JetBrains.Annotations 2025.2.2
- **License:** MIT

---

## Appendix A: Complete API Surface

```csharp
namespace EHonda.Optional.Core;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// </summary>
public readonly record struct Option<T>
{
    public bool HasValue { get; }
    public T? Value { get; }
    
    public T Or(T defaultValue);
    public T Or(Func<T> factory);
    public T? OrDefault();
    public T OrThrow();
    public T OrThrow(string message);
    public T OrThrow(Func<Exception> exceptionFactory);
    
    public void Deconstruct(out bool hasValue, out T? value);
    
    public static implicit operator Option<T>(T value);
    public static explicit operator T(Option<T> option);
}

/// <summary>
/// Provides factory methods for creating <see cref="Option{T}"/> instances.
/// </summary>
public static class Option
{
    public static Option<T> Some<T>(T value);
    public static Option<T> None<T>();
}
```

---

**End of Specification**
