namespace Engine.Shared;

/// <summary>
/// The count of updates a session has played. Anything that acts every so many updates, or alternates from one update
/// to the next, reads this one clock; nothing keeps a global counter of its own. One per session.
/// </summary>
public sealed class GameClock
{
    private readonly long _period;
    private readonly int _shortPeriod;

    /// <summary>A clock whose counts never come round again.</summary>
    public GameClock()
    {
    }

    /// <summary>
    /// A clock whose count comes round to zero after <paramref name="period"/> updates, and whose short count after
    /// <paramref name="shortPeriod"/>; nothing for either means it never does.
    /// </summary>
    public GameClock(long period, int shortPeriod)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(period);
        ArgumentOutOfRangeException.ThrowIfNegative(shortPeriod);
        (_period, _shortPeriod) = (period, shortPeriod);
    }

    /// <summary>The updates played so far that count. The first update of a session reads zero.</summary>
    public long Updates { get; private set; }

    /// <summary>
    /// A second count of the same updates, which a game may start over by itself (<see cref="RestartShort"/>), as a
    /// passage between rooms does so that what is timed by it in the next room begins together.
    /// </summary>
    public int ShortUpdates { get; private set; }

    /// <summary>Whether this update is one of every <paramref name="interval"/> updates, counted from the session's first.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The interval is not positive.</exception>
    public bool IsEvery(int interval)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(interval);
        return Updates % interval == 0;
    }

    /// <summary>Moves the clock on by one update. The game loop does this once, at the end of each update that counts.</summary>
    internal void Advance()
    {
        Updates = _period > 0 ? (Updates + 1) % _period : Updates + 1;
        ShortUpdates = _shortPeriod > 0 ? (ShortUpdates + 1) % _shortPeriod : ShortUpdates + 1;
    }

    /// <summary>
    /// Moves the clock on by updates that were not played: time that passed for the game with no update of it, as
    /// while its host loaded something. Both counts move.
    /// </summary>
    public void Pass(int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        Updates = _period > 0 ? (Updates + updates) % _period : Updates + updates;
        ShortUpdates = _shortPeriod > 0 ? (ShortUpdates + updates) % _shortPeriod : ShortUpdates + updates;
    }

    /// <summary>
    /// Starts both counts over, for a game that starts over within a session: the update this is done on reads zero
    /// from here on, and the next reads one.
    /// </summary>
    public void Restart() => (Updates, ShortUpdates) = (0, 0);

    /// <summary>Starts the short count over by itself: the update this is done on reads zero from here on, and the next reads one.</summary>
    public void RestartShort() => ShortUpdates = 0;
}
