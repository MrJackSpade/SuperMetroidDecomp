namespace Game.Compatibility;

/// <summary>
/// The cartridge's queue of music. Three things are asked of it: a piece to play, silence, and the load of a set of
/// music, since the cartridge's music comes in sets of which one is loaded at a time. Each waits behind those asked
/// for before it, and once it is at the front waits out its own wait, never fewer than eight turns, before it is
/// carried out. Loading a set stops the music.
/// </summary>
internal sealed class MusicQueue
{
    /// <summary>The shortest wait, in turns of the queue.</summary>
    public const int ShortestWait = 8;

    /// <summary>What a turn of the queue did.</summary>
    public enum Turn
    {
        /// <summary>Nothing was carried out.</summary>
        Nothing,

        /// <summary>A piece began, or the music was stopped.</summary>
        MusicChanged,

        /// <summary>A set was loaded, which stopped the music.</summary>
        SetLoaded,
    }

    private enum Kind
    {
        Play,
        Stop,
        LoadSet,
    }

    private readonly record struct Asked(Kind Kind, string Name, int Wait);

    private readonly Queue<Asked> _waiting = new();
    private bool _fetched;
    private int _wait;

    /// <summary>A queue with <paramref name="loadedSet"/> loaded, as at power-on.</summary>
    public MusicQueue(string loadedSet) => LoadedSet = loadedSet;

    /// <summary>The music playing, by name; empty for silence.</summary>
    public string Playing { get; private set; } = "";

    /// <summary>The set of music loaded, by name.</summary>
    public string LoadedSet { get; private set; }

    /// <summary>Whether anything asked for has yet to be carried out, what is waiting out its wait at the front included.</summary>
    public bool HasWaiting => _waiting.Count > 0;

    /// <summary>The set that will be loaded once everything waiting has been carried out.</summary>
    public string SetLoadedAfterWaiting => _waiting.LastOrDefault(asked => asked.Kind == Kind.LoadSet) is { Kind: Kind.LoadSet } last ? last.Name : LoadedSet;

    /// <summary>Asks for <paramref name="music"/>, empty for silence, behind what waits.</summary>
    /// <param name="music">The music, by name.</param>
    /// <param name="wait">Its wait in turns once at the front; none for the shortest.</param>
    public void AskMusic(string music, int wait) => Ask(music.Length == 0 ? Kind.Stop : Kind.Play, music, wait);

    /// <summary>Asks for the set <paramref name="set"/> to be loaded, behind what waits, with the shortest wait.</summary>
    public void AskSet(string set) => Ask(Kind.LoadSet, set, 0);

    /// <summary>Forgets everything waiting.</summary>
    public void Clear()
    {
        _waiting.Clear();
        _fetched = false;
    }

    /// <summary>
    /// Plays one turn. What is at the front waiting out its wait counts a turn, and on its last is carried out and
    /// leaves the queue. After a piece or a silence, or when nothing was at the front, the next takes its place and
    /// begins its wait on the same turn; after a set's load, only on the turn after.
    /// </summary>
    public Turn TakeTurn()
    {
        Turn did = Turn.Nothing;
        if (_fetched)
        {
            if (--_wait > 0)
                return Turn.Nothing;
            _fetched = false;
            Asked asked = _waiting.Dequeue();
            if (asked.Kind == Kind.LoadSet)
            {
                LoadedSet = asked.Name;
                Playing = "";
                return Turn.SetLoaded;
            }

            Playing = asked.Kind == Kind.Play ? asked.Name : "";
            did = Turn.MusicChanged;
        }

        if (_waiting.TryPeek(out Asked next))
        {
            _fetched = true;
            _wait = next.Wait;
        }

        return did;
    }

    private void Ask(Kind kind, string name, int wait)
    {
        if (wait == 0)
            wait = ShortestWait;
        if (wait < ShortestWait)
            throw new ArgumentOutOfRangeException(nameof(wait), wait, $"Music waits at least {ShortestWait} turns of its queue.");
        _waiting.Enqueue(new Asked(kind, name, wait));
    }
}
