using System.Reflection;

namespace SuperMetroid.Core;

/// <summary>
/// Reaches state only its owner's own updates write or read (#2107, #2129): a check, or a native snapshot's import, sets a
/// property through its private setter or reads a private field, by name, so production keeps no member wider than its
/// writers for verification's sake.
/// </summary>
internal static class PrivateState
{
    /// <summary>Sets <paramref name="property"/> of <paramref name="owner"/> to <paramref name="value"/> through its setter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static void Set<TOwner>(TOwner owner, string property, object? value) where TOwner : class =>
        (typeof(TOwner).GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetMethod
            ?? throw new MissingMemberException(typeof(TOwner).Name, property))
        .Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);

    /// <summary>The value of <paramref name="owner"/>'s instance property <paramref name="property"/>, read through its getter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no readable property of that name.</exception>
    /// <exception cref="InvalidCastException">The property does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Get<TOwner, TValue>(TOwner owner, string property) where TOwner : class =>
        (TValue)(typeof(TOwner).GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetMethod
            ?? throw new MissingMemberException(typeof(TOwner).Name, property))
        .Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, [], culture: null)!;

    /// <summary>Calls <paramref name="owner"/>'s instance method <paramref name="method"/> with <paramref name="arguments"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance method of that name.</exception>
    internal static void Call<TOwner>(TOwner owner, string method, params object?[] arguments) where TOwner : class =>
        (typeof(TOwner).GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, method))
        .Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null);

    /// <summary>Replaces the value of <paramref name="owner"/>'s instance field <paramref name="field"/>, read-only or not.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    internal static void SetField<TOwner>(TOwner owner, string field, object? value) where TOwner : class =>
        (typeof(TOwner).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, field))
        .SetValue(Ensure.NotNull(owner), value);

    /// <summary>The value of <paramref name="owner"/>'s instance field <paramref name="field"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    /// <exception cref="InvalidCastException">The field does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue Field<TOwner, TValue>(TOwner owner, string field) where TOwner : class =>
        (TValue)(typeof(TOwner).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, field))
        .GetValue(Ensure.NotNull(owner))!;

    /// <summary>The value of the instance field <paramref name="field"/> of the value <paramref name="owner"/>.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no instance field of that name.</exception>
    /// <exception cref="InvalidCastException">The field does not hold a <typeparamref name="TValue"/>.</exception>
    internal static TValue ValueField<TOwner, TValue>(TOwner owner, string field) where TOwner : struct =>
        (TValue)(typeof(TOwner).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(typeof(TOwner).Name, field))
        .GetValue(owner)!;
}
