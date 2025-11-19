using JetBrains.Annotations;

namespace EHonda.Optional.Core;

/// <summary>
/// Provides factory methods for creating <see cref="Option{T}"/> instances.
/// </summary>
/// <remarks>
/// This static class provides convenient methods for creating options in both Some and None states.
/// Use <see cref="Some{T}"/> to create an option with a value, and <see cref="None{T}"/>
/// to create an option without a value.
/// </remarks>
[PublicAPI]
public static class Option
{
    /// <summary>
    /// Creates an <see cref="Option{T}"/> in the Some state containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value. Can be nullable.</typeparam>
    /// <param name="value">The value to contain in the option. Can be <c>null</c> if <typeparamref name="T"/> is nullable.</param>
    /// <returns>An option containing <paramref name="value"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method creates an option with a value, even if that value is <c>null</c>.
    /// When <typeparamref name="T"/> is a nullable type, Some(null) is distinct from None.
    /// </para>
    /// <para>
    /// For non-nullable reference types, the compiler will enforce that <paramref name="value"/> is not <c>null</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Creating Some with non-nullable value
    /// Option&lt;string&gt; some = Option.Some("hello");
    /// 
    /// // Creating Some with nullable value (explicit type parameter required)
    /// Option&lt;string?&gt; someNull = Option.Some&lt;string?&gt;(null);
    /// 
    /// // Creating Some with value type
    /// Option&lt;int&gt; someInt = Option.Some(42);
    /// 
    /// // Using implicit conversion (alternative to Some)
    /// Option&lt;string&gt; some2 = "hello";
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public static Option<T> Some<T>(T value)
    {
        // The implicit conversion operator in Option<T> handles the actual construction
        return value;
    }

    /// <summary>
    /// Creates an <see cref="Option{T}"/> in the None state (no value).
    /// </summary>
    /// <typeparam name="T">The type parameter of the option.</typeparam>
    /// <returns>An option in the None state.</returns>
    /// <remarks>
    /// This method explicitly creates a None option. It's equivalent to using <c>default(Option&lt;T&gt;)</c>
    /// but more expressive in code.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using None factory method
    /// Option&lt;string&gt; none = Option.None&lt;string&gt;();
    /// 
    /// // Equivalent to using default
    /// Option&lt;string&gt; none2 = default;
    /// </code>
    /// </example>
    [Pure]
    [MustUseReturnValue]
    public static Option<T> None<T>()
    {
        return default;
    }
}
