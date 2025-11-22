using JetBrains.Annotations;

namespace EHonda.Optional.Core;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/> that allows null values.
/// An option can either be in a "Some" state (containing a value, which can be null) or a "None" state (containing no value).
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
[PublicAPI]
public readonly record struct NullableOption<T>
{
    /// <summary>
    /// Gets a value indicating whether this option contains a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value contained in this option.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NullableOption{T}"/> struct with the specified value.
    /// </summary>
    private NullableOption(T? value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise returns <paramref name="defaultValue"/>.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? Or(T? defaultValue)
    {
        return HasValue ? Value : defaultValue;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise invokes <paramref name="factory"/> and returns its result.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? Or(Func<T?> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        return HasValue ? Value : factory();
    }

    /// <summary>
    /// Returns the contained value if present, otherwise returns <c>default(T)</c>.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? OrDefault()
    {
        return HasValue ? Value : default;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? OrThrow()
    {
        if (!HasValue)
        {
            throw new InvalidOperationException("Option has no value.");
        }

        return Value;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws an <see cref="InvalidOperationException"/> 
    /// with the specified message.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? OrThrow(string message)
    {
        if (!HasValue)
        {
            throw new InvalidOperationException(message);
        }

        return Value;
    }

    /// <summary>
    /// Returns the contained value if present, otherwise throws the exception created by 
    /// <paramref name="exceptionFactory"/>.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public T? OrThrow(Func<Exception> exceptionFactory)
    {
        if (exceptionFactory is null)
        {
            throw new ArgumentNullException(nameof(exceptionFactory));
        }

        if (!HasValue)
        {
            throw exceptionFactory();
        }

        return Value;
    }

    /// <summary>
    /// Deconstructs this option into its constituent parts for pattern matching.
    /// </summary>
    [Pure]
    public void Deconstruct(out bool hasValue, out T? value)
    {
        hasValue = HasValue;
        value = Value;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="NullableOption{T}"/> in the Some state.
    /// </summary>
    [Pure]
    public static implicit operator NullableOption<T>(T? value)
    {
        return new NullableOption<T>(value);
    }

    /// <summary>
    /// Implicitly converts an <see cref="Option{T}"/> to a <see cref="NullableOption{T}"/>.
    /// </summary>
    [Pure]
    public static implicit operator NullableOption<T>(Option<T> option)
    {
        return option.HasValue ? new NullableOption<T>(option.Value) : default;
    }

    /// <summary>
    /// Explicitly converts a <see cref="NullableOption{T}"/> to an <see cref="Option{T}"/>.
    /// Throws if the value is null.
    /// </summary>
    [Pure]
    public static explicit operator Option<T>(NullableOption<T> option)
    {
        if (!option.HasValue)
        {
            return Option.None<T>();
        }

        if (option.Value is null)
        {
            throw new InvalidOperationException("Cannot convert NullableOption containing null to Option.");
        }

        return Option.Some(option.Value);
    }

    /// <summary>
    /// Explicitly converts a <see cref="NullableOption{T}"/> to its contained value.
    /// </summary>
    /// <param name="option">The option to extract the value from.</param>
    /// <returns>The contained value.</returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="option"/> is None (does not have a value).
    /// </exception>
    [Pure]
    public static explicit operator T?(NullableOption<T> option)
    {
        return option.OrThrow();
    }
}
