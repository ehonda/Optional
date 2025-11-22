# Implicit vs. Explicit Conversions

This document outlines the design decisions regarding implicit and explicit conversions for the `Option<T>` and `NullableOption<T>` types.

## Summary

| Conversion | Type | Status | Rationale |
| :--- | :--- | :--- | :--- |
| `T` → `Option<T>` | Implicit | ✅ Allowed | Always safe; never throws. |
| `Option<T>` → `T` | Explicit | ⚠️ Restricted | Unsafe; throws on `None`. |

## Implicit Conversions (`T` → `Option<T>`)

We allow implicit conversion from a value `T` to an `Option<T>` (or `NullableOption<T>`).

```csharp
Option<int> opt = 42; // Implicitly creates Option.Some(42)
```

### Reasoning

1. **Safety**: Wrapping a value in an Option is a safe operation that will never throw an exception (assuming the constructor validation passes, e.g., non-null for `Option<T>`).
2. **Convenience**: It allows for cleaner syntax when returning values from methods defined to return `Option<T>`.
3. **Precedent**: This mirrors the behavior of `Nullable<T>` in C# (e.g., `int? x = 5;`).

## Explicit Conversions (`Option<T>` → `T`)

We **require** an explicit cast when converting from an `Option<T>` back to its underlying type `T`.

```csharp
Option<int> opt = Option.None<int>();

// ❌ Compile Error
int value = opt; 

// ✅ Compiles, but may throw at runtime
int value = (int)opt; 
```

### Rationale for Explicit Requirement

#### 1. Violation of Design Guidelines

Microsoft's [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads) explicitly state:

> **DO NOT** throw exceptions from implicit casts.

Converting a `None` state to `T` is impossible without providing a default value or throwing an exception. Since the cast operator cannot accept a default value argument, it must throw `InvalidOperationException` when the option is `None`. Therefore, making this conversion implicit would violate standard .NET design patterns.

#### 2. Safety and Intent

The primary purpose of `Option<T>` is to force the developer to handle the "missing value" case.

* **Explicit Cast**: Signals "I know this might fail, but I am asserting a value exists."
* **Implicit Cast**: Hides the potential failure. A simple assignment `T x = option;` looking perfectly safe could crash the application at runtime.

#### 3. Consistency with `Nullable<T>`

`Option<T>` is designed to be a more expressive counterpart to `Nullable<T>`.

* `Nullable<T>` requires an explicit cast to extract the value:

    ```csharp
    int? n = null;
    int i = (int)n; // Throws InvalidOperationException
    ```

* Making `Option<T>` implicit would break the mental model established by `Nullable<T>` and confuse developers expecting standard C# behavior.

#### 4. "Footgun" Prevention

If the conversion were implicit, it would be easy to accidentally pass an `Option<T>` to a method expecting `T`, leading to runtime crashes that are hard to spot during code review.

```csharp
// If implicit conversion were allowed:
public void Process(string input) { ... }

Option<string> maybeName = Option.None<string>();
Process(maybeName); // Looks safe, compiles, but crashes at runtime!
```

By requiring an explicit cast (or better, using `.Or()`, `.OrDefault()`, or `.Match()`), we ensure the developer consciously handles the control flow.
