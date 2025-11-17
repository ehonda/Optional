using EHonda.Optional.Core;

namespace Demonstration;

/// <summary>
/// Demonstrates the usage of the Option&lt;T&gt; type.
/// This file is for validation purposes and shows how the type works according to the specification.
/// </summary>
public static class OptionDemo
{
    public static void DemonstrateBasicUsage()
    {
        // Creating Some options
        Option<string> some = Option.Some("hello");
        Option<int> someInt = Option.Some(42);
        
        // Creating None options
        Option<string> none = Option.None<string>();
        _ = default(Option<string>); // Can also use default
        
        // Implicit conversion from T to Option<T>
        Option<string> implicitSome = "world";
        
        // Distinguishing unspecified from explicit null
        Option<string?> unspecified = default;                    // None
        Option<string?> explicitNull = Option.Some<string?>(null); // Some(null)
        Option<string?> someValue = Option.Some<string?>("hi");    // Some("hi")
        
        // Using Or methods
        string value1 = some.Or("default");           // "hello"
        string value2 = none.Or("default");           // "default"
        string value3 = none.Or(() => "lazy default"); // "lazy default"
        
        // Using OrDefault
        string? value4 = unspecified.OrDefault();     // null
        int value5 = someInt.OrDefault();             // 42
        
        // Using OrThrow
        try
        {
            string value6 = none.OrThrow();           // throws InvalidOperationException
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
        
        string value7 = some.OrThrow("Value required"); // "hello"
        
        // Pattern matching with deconstruction
        var result = some switch
        {
            (true, var v) => $"Some: {v}",
            (false, _) => "None"
        };
        
        // Property pattern matching
        var result2 = some switch
        {
            { HasValue: true, Value: var v } => $"Some: {v}",
            { HasValue: false } => "None"
        };
        
        // Explicit conversion
        string extracted = (string)some; // "hello"
    }
    
    /// <summary>
    /// Demonstrates the primary use case: distinguishing unspecified from explicit null in API parameters.
    /// </summary>
    public static void UpdateUser(int userId, Option<string?> name = default, Option<int> age = default)
    {
        // Unspecified (default): Don't update field
        if (name.HasValue)
        {
            // name.Value could be null (clear field) or non-null (update field)
            // This is how we distinguish "don't change" from "set to null"
        }
        
        if (age.HasValue)
        {
            // age.Value is the new age to set
        }
    }
    
    public static void DemonstratePrimaryUseCase()
    {
        // Don't update name or age
        UpdateUser(userId: 1);
        
        // Clear the name field (set to null), don't update age
        UpdateUser(userId: 1, name: Option.Some<string?>(null));
        
        // Update name to "Alice", don't update age
        UpdateUser(userId: 1, name: Option.Some<string?>("Alice"));
        
        // Update both name and age
        UpdateUser(userId: 1, name: Option.Some<string?>("Alice"), age: Option.Some(30));
    }
    
    /// <summary>
    /// Demonstrates equality semantics.
    /// </summary>
    public static void DemonstrateEquality()
    {
        // Value equality for Some
        Option<int> a = Option.Some(5);
        Option<int> b = Option.Some(5);
        bool equal1 = a == b;  // true
        
        // None equality
        Option<int> none1 = default;
        Option<int> none2 = Option.None<int>();
        bool equal2 = none1 == none2;  // true
        
        // Some(null) vs None
        Option<string?> someNull = Option.Some<string?>(null);
        Option<string?> none = default;
        bool equal3 = someNull == none;  // false (Some(null) != None)
        
        // Some(null) vs Some(null)
        Option<string?> someNull2 = Option.Some<string?>(null);
        bool equal4 = someNull == someNull2;  // true
    }
}
