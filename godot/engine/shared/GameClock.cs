namespace Engine.Shared;

/// <summary>
/// The count of updates a session has played. Anything that acts every so many updates, or alternates from one update
/// to the next, reads this one clock; nothing keeps a global counter of its own. One per session.
/// </summary>
public sealed class GameClock
{
    /// <summary>The updates played so far. The first update of a session reads zero.</summary>
    public long Updates { get; private set; }

    /// <summary>Whether this update is one of every <paramref name="interval"/> updates, counted from the session's first.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The interval is not positive.</exception>
    public bool IsEvery(int interval)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(interval);
        return Updates % interval == 0;
    }

    /// <summary>Moves the clock on by one update. The game loop does this once, at the end of each update.</summary>
    internal void Advance() => Updates++;
}
