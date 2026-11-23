namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// How long a moving shutter rests at an end before it moves again (#1818): a number of AI updates, or for good, when it
/// stays where it stopped until a reaction starts it again.
/// </summary>
public readonly partial record struct ShutterRest
{
    /// <summary>The updates a timed rest lasts; null for a permanent rest.</summary>
    private readonly int? _updates;

    /// <summary>Keeps a timed rest of <paramref name="updates"/>, or a permanent one when it is null.</summary>
    private ShutterRest(int? updates) => _updates = updates;

    /// <summary>A rest that never ends: the shutter stays at the end it reached until a reaction starts it again.</summary>
    public static ShutterRest Permanent { get; } = new(null);

    /// <summary>A rest of <paramref name="updates"/> AI updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    public static ShutterRest For(int updates) => updates >= 0
        ? new(updates)
        : throw new ArgumentOutOfRangeException(nameof(updates), updates, "A shutter rest cannot be negative.");

    /// <summary>Whether the shutter stops at the end for good instead of resting a number of updates.</summary>
    public bool IsPermanent => _updates is null;

    /// <summary>Finds the AI updates a timed rest lasts; a permanent rest has no length (#2554).</summary>
    public bool TryGetUpdates(out int updates)
    {
        updates = _updates.GetValueOrDefault();
        return _updates.HasValue;
    }
}
