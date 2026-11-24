namespace Game.Compatibility;

/// <summary>
/// The cartridge's queue of music: each piece asked for, or each silence, waits behind those asked for before it,
/// and once it is at the front waits out its own wait, never fewer than eight turns, before it plays.
/// </summary>
internal sealed class MusicQueue
{
    /// <summary>The shortest wait, in turns of the queue.</summary>
    public const int ShortestWait = 8;

    private readonly Queue<(string Music, int Wait)> _waiting = new();
    private bool _fetched;
    private int _wait;

    /// <summary>The music playing, by name; empty for silence.</summary>
    public string Playing { get; private set; } = "";

    /// <summary>Whether anything asked for has yet to play, what is waiting out its wait at the front included.</summary>
    public bool HasWaiting => _waiting.Count > 0;

    /// <summary>Asks for <paramref name="music"/>, empty for silence, behind what waits.</summary>
    /// <param name="music">The music, by name.</param>
    /// <param name="wait">Its wait in turns once at the front; none for the shortest.</param>
    /// <exception cref="ArgumentOutOfRangeException">The wait is shorter than the shortest.</exception>
    public void Ask(string music, int wait)
    {
        if (wait == 0)
            wait = ShortestWait;
        if (wait < ShortestWait)
            throw new ArgumentOutOfRangeException(nameof(wait), wait, $"Music waits at least {ShortestWait} turns of its queue.");
        _waiting.Enqueue((music, wait));
    }

    /// <summary>
    /// Plays one turn. What is at the front waiting out its wait counts a turn, and on its last plays and leaves the
    /// queue; then, or when nothing was at the front, the next takes its place and begins its wait.
    /// </summary>
    /// <returns>Whether the music changed on this turn.</returns>
    public bool TakeTurn()
    {
        bool changed = false;
        if (_fetched)
        {
            if (--_wait > 0)
                return false;
            _fetched = false;
            Playing = _waiting.Dequeue().Music;
            changed = true;
        }

        if (_waiting.TryPeek(out (string Music, int Wait) next))
        {
            _fetched = true;
            _wait = next.Wait;
        }

        return changed;
    }
}
