using JetBrains.Annotations;

namespace EHonda.Optional.Core;

/// <summary>
/// Provides factory methods for creating <see cref="NullableOption{T}"/> instances.
/// </summary>
[PublicAPI]
public static class NullableOption
{
    /// <summary>
    /// Creates a <see cref="NullableOption{T}"/> in the Some state containing the specified value.
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public static NullableOption<T> Some<T>(T? value)
    {
        return value;
    }

    /// <summary>
    /// Creates a <see cref="NullableOption{T}"/> in the None state (no value).
    /// </summary>
    [Pure]
    [MustUseReturnValue]
    public static NullableOption<T> None<T>()
    {
        return default;
    }
}
