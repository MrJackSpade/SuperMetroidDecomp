namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// How long a moving shutter rests at an end before it moves again (#1818): a number of AI updates, or for good, when it
/// stays where it stopped until a reaction starts it again.
/// </summary>
public readonly record struct ShutterRest
{
    /// <summary>The updates a timed rest lasts.</summary>
    private readonly int _updates;

    /// <summary>Keeps a timed rest of <paramref name="updates"/>, or a permanent one.</summary>
    private ShutterRest(int updates, bool isPermanent)
    {
        _updates = updates;
        IsPermanent = isPermanent;
    }

    /// <summary>A rest that never ends: the shutter stays at the end it reached until a reaction starts it again.</summary>
    public static ShutterRest Permanent { get; } = new(0, isPermanent: true);

    /// <summary>A rest of <paramref name="updates"/> AI updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is negative.</exception>
    public static ShutterRest For(int updates) => updates >= 0
        ? new(updates, isPermanent: false)
        : throw new ArgumentOutOfRangeException(nameof(updates), updates, "A shutter rest cannot be negative.");

    /// <summary>Whether the shutter stops at the end for good instead of resting a number of updates.</summary>
    public bool IsPermanent { get; }

    /// <summary>The AI updates a timed rest lasts.</summary>
    /// <exception cref="InvalidOperationException">The rest is permanent and has no length.</exception>
    public int Updates => IsPermanent
        ? throw new InvalidOperationException("A permanent shutter rest has no length.")
        : _updates;

    /// <summary>Prints the rest as permanent or as its length, without reading a permanent rest's length.</summary>
    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append(IsPermanent ? "Permanent" : $"Updates = {_updates}");
        return true;
    }
}
