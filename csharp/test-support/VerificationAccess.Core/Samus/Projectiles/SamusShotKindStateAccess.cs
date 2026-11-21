namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A check's reach to the state a shot's kind keeps (#2554), which production hands only to the shot's own behaviour.</summary>
internal static class SamusShotKindStateAccess
{
    /// <summary>The <typeparamref name="T"/> the shot holding <paramref name="shot"/>'s place keeps.</summary>
    /// <typeparam name="T">The kind state.</typeparam>
    /// <param name="shot">The shot place.</param>
    /// <exception cref="InvalidOperationException">The place's shot keeps no such state.</exception>
    internal static T KindStateAs<T>(this SamusProjectile shot) where T : SamusShotKindState =>
        Ensure.NotNull(shot).Occupant?.KindState as T
            ?? throw new InvalidOperationException($"Shot place {shot.Place} keeps no {typeof(T).Name}.");
}
