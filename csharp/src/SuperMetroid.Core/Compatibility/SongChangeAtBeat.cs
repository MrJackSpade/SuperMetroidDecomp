namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The song change that waits for the music's next beat (#2562): a later change replaces it, the playback's step at the
/// beat takes it, and switching the clip set forgets it.
/// </summary>
/// <remarks>
/// Evidence: the game's sound processor starts a requested song only at its music engine's next tick, so a request made
/// between ticks waits for it, and a later request replaces the earlier.
/// Lifetime: the clip playback's.
/// </remarks>
/// <typeparam name="TChange">The song change.</typeparam>
[CompatibilityService("a song change waits for the music's next beat")]
public sealed class SongChangeAtBeat<TChange>
    where TChange : class
{
    /// <summary>The change waiting for the beat, or null when none waits.</summary>
    public TChange? Waiting { get; private set; }

    /// <summary>Holds <paramref name="change"/> until the next beat, replacing any change already waiting.</summary>
    internal void Hold(TChange change) => Waiting = Ensure.NotNull(change);

    /// <summary>Switching the clip set forgets the waiting change.</summary>
    internal void Forget() => Waiting = null;

    /// <summary>The playback's step at the beat takes the waiting change, leaving none.</summary>
    internal TChange? Take()
    {
        TChange? change = Waiting;
        Waiting = null;
        return change;
    }
}
