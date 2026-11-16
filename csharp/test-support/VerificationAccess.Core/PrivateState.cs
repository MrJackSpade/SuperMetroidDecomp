using System.Reflection;

namespace SuperMetroid.Core;

/// <summary>
/// Writes a property through the private setter only its owner's own updates use, so a check or an imported snapshot can
/// start an owner in a state it would otherwise take many updates to reach (#2107).
/// </summary>
internal static class PrivateState
{
    /// <summary>Sets <paramref name="property"/> of <paramref name="owner"/> to <paramref name="value"/> through its setter.</summary>
    /// <exception cref="MissingMemberException">The owner's type has no settable property of that name.</exception>
    internal static void Set<TOwner>(TOwner owner, string property, object? value) where TOwner : class =>
        (typeof(TOwner).GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetMethod
            ?? throw new MissingMemberException(typeof(TOwner).Name, property))
        .Invoke(Ensure.NotNull(owner), BindingFlags.DoNotWrapExceptions, binder: null, [value], culture: null);
}
