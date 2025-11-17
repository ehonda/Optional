using JetBrains.Annotations;

namespace EHonda.Optional.Core;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// An option can either be in a "Some" state (containing a value) or a "None" state (containing no value).
/// </summary>
/// <typeparam name="T">The type of the optional value. Can be nullable to distinguish between unspecified and explicit null.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="Option{T}"/> type provides a type-safe way to represent optional values,
/// distinguishing between "no value" (None) and "some value" (Some), including the ability
/// to differentiate between an unspecified value and an explicit <c>null</c>.
/// </para>
/// <para>
/// The default value <c>default(Option&lt;T&gt;)</c> represents None.
/// </para>
/// <para>
/// When <typeparamref name="T"/> is a nullable type (e.g., <c>string?</c>), the option can represent:
/// <list type="bullet">
/// <item><description>None: No value specified</description></item>
/// <item><description>Some(null): Value explicitly specified as null</description></item>
/// <item><description>Some(value): Value specified as non-null</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating options
/// Option&lt;string&gt; some = Option.Some("hello");
/// Option&lt;string&gt; none = Option.None&lt;string&gt;();
/// Option&lt;string&gt; defaultNone = default;
/// 
/// // Distinguishing unspecified from explicit null
/// Option&lt;string?&gt; unspecified = default;                    // None
/// Option&lt;string?&gt; explicitNull = Option.Some&lt;string?&gt;(null); // Some(null)
/// 
/// // Using values
/// string value = some.Or("default");           // "hello"
/// string value2 = none.Or("default");          // "default"
/// string? value3 = unspecified.OrDefault();    // null
/// 
/// // Pattern matching
/// var result = some switch
/// {
///     (true, var v) => $"Some: {v}",
///     (false, _) => "None"
/// };
/// </code>
/// </example>
[PublicAPI]
public readonly record struct Option<T>
{
    /// <summary>
    /// Gets a value indicating whether this option contains a value.
    /// </summary>
    /// <value>
    /// <c>true</c> if this option is in the Some state; otherwise, <c>false</c> for None.
    /// </value>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value contained in this option.
    /// </summary>
    /// <value>
    /// The contained value if <see cref="HasValue"/> is <c>true</c>; otherwise, <c>default(T)</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property should only be accessed when <see cref="HasValue"/> is <c>true</c>.
    /// When the option is None, accessing this property will return <c>default(T)</c>.
    /// </para>
    /// <para>
    /// When <typeparamref name="T"/> is a nullable type, this property can legitimately contain <c>null</c>
    /// when the option is Some(null), which is distinct from None.
    /// </para>
    /// <para>
    /// Consider using one of the safe accessor methods (<see cref="Or(T)"/>, <see cref="OrDefault"/>, 
    /// <see cref="OrThrow()"/>) instead of directly accessing this property.
    /// </para>
    /// </remarks>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Option{T}"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The value to contain in this option.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating options.
    /// Use <see cref="Option.Some{T}"/> to create an option with a value, or <see cref="Option.None{T}"/>
    /// or <c>default</c> to create a None option.
    /// </remarks>
    private Option(T value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise returns <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="defaultValue">The value to return if this option is None.</param>
    /// <returns>
    /// The contained value if <see cref="HasValue"/> is <c>true</c>, otherwise <paramref name="defaultValue"/>.
    /// </returns>
    /// <remarks>
    /// This method is eager - <paramref name="defaultValue"/> is always evaluated.
    /// If computing the default value is expensive, use <see cref="Or(Func{T})"/> instead.
    /// </remarks>
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
    {
        return HasValue ? Value! : defaultValue;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise invokes <paramref name="factory"/> and returns its result.
    /// </summary>
    /// <param name="factory">The factory function to invoke if this option is None.</param>
    /// <returns>
    /// The contained value if <see cref="HasValue"/> is <c>true</c>, 
    /// otherwise the result of invoking <paramref name="factory"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This method is lazy - <paramref name="factory"/> is only invoked if the option is None.
    /// This is useful for expensive computations that should be deferred until needed.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; some = Option.Some("hello");
    /// Option&lt;string&gt; none = default;
    /// 
    /// // Factory is not called for Some
    /// string value1 = some.Or(() => ExpensiveOperation()); // Returns "hello" without calling ExpensiveOperation
    /// 
    /// // Factory is called for None
    /// string value2 = none.Or(() => ExpensiveOperation()); // Calls ExpensiveOperation and returns its result
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public T Or(Func<T> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        return HasValue ? Value! : factory();
    }

    /// <summary>
    /// Returns the contained value if present, otherwise returns <c>default(T)</c>.
    /// </summary>
    /// <returns>
    /// The contained value if <see cref="HasValue"/> is <c>true</c>, otherwise <c>default(T)</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// For reference types and nullable value types, <c>default(T)</c> is <c>null</c>.
    /// For non-nullable value types, <c>default(T)</c> is the zero-initialized value.
    /// </para>
    /// <para>
    /// When <typeparamref name="T"/> is nullable, this method can return <c>null</c> for both
    /// None and Some(null) cases. Use <see cref="HasValue"/> if you need to distinguish between them.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;int&gt; someInt = Option.Some(5);
    /// Option&lt;int&gt; noneInt = default;
    /// 
    /// Console.WriteLine(someInt.OrDefault()); // Output: 5
    /// Console.WriteLine(noneInt.OrDefault()); // Output: 0
    /// 
    /// Option&lt;string&gt; someString = Option.Some("hello");
    /// Option&lt;string&gt; noneString = default;
    /// 
    /// Console.WriteLine(someString.OrDefault() ?? "null"); // Output: hello
    /// Console.WriteLine(noneString.OrDefault() ?? "null"); // Output: null
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public T? OrDefault()
    {
        return HasValue ? Value : default;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">
    /// This option is None (does not have a value).
    /// </exception>
    /// <remarks>
    /// Use this method when you expect the option to have a value and want to fail fast if it doesn't.
    /// If you want to provide a custom error message or exception type, use the other overloads.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; some = Option.Some("hello");
    /// Option&lt;string&gt; none = default;
    /// 
    /// string value1 = some.OrThrow(); // Returns "hello"
    /// string value2 = none.OrThrow(); // Throws InvalidOperationException: "Option has no value."
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public T OrThrow()
    {
        if (!HasValue)
        {
            throw new InvalidOperationException("Option has no value.");
        }

        return Value!;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws an <see cref="InvalidOperationException"/> 
    /// with the specified message.
    /// </summary>
    /// <param name="message">The error message to use if this option is None.</param>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">
    /// This option is None (does not have a value).
    /// </exception>
    /// <remarks>
    /// Use this method when you want to provide context about why the option was expected to have a value.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; config = GetConfiguration("timeout");
    /// string timeout = config.OrThrow("Configuration 'timeout' is required but was not provided.");
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public T OrThrow(string message)
    {
        if (!HasValue)
        {
            throw new InvalidOperationException(message);
        }

        return Value!;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws the exception created by 
    /// <paramref name="exceptionFactory"/>.
    /// </summary>
    /// <param name="exceptionFactory">
    /// The factory function that creates the exception to throw if this option is None.
    /// </param>
    /// <returns>The contained value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exceptionFactory"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="Exception">
    /// This option is None (does not have a value). The exception thrown is created by <paramref name="exceptionFactory"/>.
    /// </exception>
    /// <remarks>
    /// Use this method when you want to throw a custom exception type or include additional context in the exception.
    /// The factory is only invoked if the option is None.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;User&gt; user = GetUser(userId);
    /// User currentUser = user.OrThrow(() => new UserNotFoundException(userId));
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public T OrThrow(Func<Exception> exceptionFactory)
    {
        if (exceptionFactory is null)
        {
            throw new ArgumentNullException(nameof(exceptionFactory));
        }

        if (!HasValue)
        {
            throw exceptionFactory();
        }

        return Value!;
    }

    /// <summary>
    /// Deconstructs this option into its constituent parts for pattern matching.
    /// </summary>
    /// <param name="hasValue">
    /// When this method returns, contains <c>true</c> if this option has a value; otherwise, <c>false</c>.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the value if this option has a value; otherwise, <c>default(T)</c>.
    /// </param>
    /// <remarks>
    /// This method enables positional pattern matching using C# pattern matching syntax.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; option = GetOption();
    /// 
    /// // Using switch expression with deconstruction
    /// var result = option switch
    /// {
    ///     (true, var value) => $"Some: {value}",
    ///     (false, _) => "None"
    /// };
    /// 
    /// // Using if statement with deconstruction
    /// if (option is (true, var value))
    /// {
    ///     Console.WriteLine($"Got value: {value}");
    /// }
    /// </code>
    /// </example>
    [Pure]
    public void Deconstruct(out bool hasValue, out T? value)
    {
        hasValue = HasValue;
        value = Value;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to an <see cref="Option{T}"/> in the Some state.
    /// </summary>
    /// <param name="value">The value to wrap in an option.</param>
    /// <returns>An option containing <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// This conversion enables natural construction of Some options without explicit factory method calls.
    /// </para>
    /// <para>
    /// When <typeparamref name="T"/> is a nullable type, this includes converting <c>null</c> to Some(null),
    /// which is distinct from None. To create None, use <see cref="Option.None{T}"/> or <c>default</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Implicit conversion from non-nullable value
    /// Option&lt;string&gt; option = "hello"; // Creates Some("hello")
    /// 
    /// // Implicit conversion from nullable value (creates Some(null), not None)
    /// Option&lt;string?&gt; optionNull = (string?)null; // Creates Some(null)
    /// 
    /// // To create None, use explicit syntax
    /// Option&lt;string&gt; none = default; // None
    /// Option&lt;string&gt; none2 = Option.None&lt;string&gt;(); // None
    /// </code>
    /// </example>
    [Pure]
    public static implicit operator Option<T>(T value)
    {
        return new Option<T>(value);
    }

    /// <summary>
    /// Explicitly converts an <see cref="Option{T}"/> to its contained value.
    /// </summary>
    /// <param name="option">The option to extract the value from.</param>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="option"/> is None (does not have a value).
    /// </exception>
    /// <remarks>
    /// This conversion requires an explicit cast because it can throw an exception if the option is None.
    /// Consider using <see cref="Or(T)"/>, <see cref="OrDefault"/>, or <see cref="OrThrow()"/> instead
    /// for more explicit error handling.
    /// </remarks>
    /// <example>
    /// <code>
    /// Option&lt;string&gt; some = Option.Some("hello");
    /// string value = (string)some; // Returns "hello"
    /// 
    /// Option&lt;string&gt; none = default;
    /// string value2 = (string)none; // Throws InvalidOperationException
    /// </code>
    /// </example>
    [Pure]
    public static explicit operator T(Option<T> option)
    {
        return option.OrThrow();
    }
}
