using System.Reflection;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Writes a frontend screen's state through the private setter only the screen's own updates use, so a check can start a
/// screen at a step it would otherwise take many updates to reach (#2082).
/// </summary>
internal static class PrivateFrontendState
{
    /// <summary>Sets <paramref name="property"/> of <paramref name="screen"/> to <paramref name="value"/> through its setter.</summary>
    /// <exception cref="MissingMemberException">The screen's type has no settable property of that name.</exception>
    internal static void Set<TScreen>(TScreen screen, string property, object? value) where TScreen : class =>
        (typeof(TScreen).GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetMethod
            ?? throw new MissingMemberException(typeof(TScreen).Name, property))
        .Invoke(Ensure.NotNull(screen), [value]);
}
