namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads the kind state a Samus shot carries for checks (#2554).</summary>
internal static class SamusShotKindStateAccess
{
    /// <summary>The kind state a Samus shot carries.</summary>
    extension(SamusProjectile self)
    {
        /// <summary>The shot's kind state, which must be a <typeparamref name="T"/>.</summary>
        /// <exception cref="InvalidOperationException">The shot carries no <typeparamref name="T"/>.</exception>
        internal T KindStateAs<T>() where T : SamusShotKindState => self.KindState as T
            ?? throw new InvalidOperationException($"The shot carries {self.KindState?.GetType().Name ?? "no kind state"}, not {typeof(T).Name}.");
    }
}
