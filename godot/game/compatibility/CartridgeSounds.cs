using Engine.Core;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's way with sound, which is part of how the game plays: a door waits for the sounds and the music
/// waiting to run out, and the speed booster reads how many sounds were waiting. Every sound belongs to one of three
/// libraries, told by the first part of its name ("samus", "enemy", "room"), and waits its turn in that library's
/// queue (<see cref="SoundLibraryQueue"/>); the sound program takes up what it is asked on its own clock
/// (<see cref="SoundProgramClock"/>); music waits in a queue of its own (<see cref="MusicQueue"/>), and a change of
/// music withdraws what every library asks for eight turns. A sound is heard when the program starts it, on the
/// channel of its library.
/// </summary>
/// <remarks>
/// An update opens with the music's turn, the mode plays it, asking as it goes, and it closes with the sounds' turns
/// and the program's clock. What the program reports back is read by the next update's turns.
/// </remarks>
public sealed class CartridgeSounds : ISoundFlow
{
    /// <summary>The turns of the sounds, after the music changes, on which what every library asks is withdrawn.</summary>
    private const int MusicChangeDowntime = 8;

    /// <summary>What ends the name of the sound that stops everything its library is sounding.</summary>
    private const string StopsAll = "_stop";

    /// <summary>The libraries, in the order their turns are taken and the program serves them.</summary>
    private static readonly string[] Libraries = ["samus", "enemy", "room"];

    private readonly Dictionary<string, SoundLibraryQueue> _queues =
        Libraries.ToDictionary(library => library, _ => new SoundLibraryQueue(), StringComparer.Ordinal);

    private readonly SoundProgramClock _clock = new(Libraries);
    private readonly MusicQueue _music = new();
    private readonly List<(string Library, string? Sound)> _askedOfProgram = [];
    private readonly List<AcceptedSound> _started = [];
    private int _downtime;

    /// <summary>The flow of <paramref name="session"/>, which is this game's.</summary>
    public static CartridgeSounds Of(GameSession session) => (CartridgeSounds)session.Sounds.Flow;

    /// <summary>
    /// Whether a door's passage is turning sounds away: from its stops being asked for until the next room has faded
    /// in. Sounds and music already waiting still take their turns.
    /// </summary>
    public bool DoorTurnsSoundsAway { get; set; }

    /// <summary>Whether a demonstration is playing, which turns every sound away.</summary>
    public bool DemoPlaying { get; set; }

    /// <summary>Whether a power bomb's blast, or the crystal flash's, is under way, which turns every sound away.</summary>
    public bool PowerBombExploding { get; set; }

    /// <inheritdoc/>
    public string Music => _music.Playing;

    /// <inheritdoc/>
    public bool HasWaitingSounds => _queues.Values.Any(queue => queue.Waiting > 0);

    /// <inheritdoc/>
    public bool HasWaitingMusic => _music.HasWaiting;

    /// <summary>
    /// Takes a sound into its library's queue. Its priority is tried first, against how many wait; one let in is
    /// then dropped all the same while the game turns sounds away. The channel its asker named is not used: its
    /// library is its channel.
    /// </summary>
    public SoundAdmission Ask(string sound, string channel, SoundPriority priority) =>
        _queues[LibraryOf(sound)].Admit(sound, priority, DoorTurnsSoundsAway || DemoPlaying || PowerBombExploding);

    /// <summary>Asks each library, in order, for the sound that stops everything it is sounding.</summary>
    public void AskSilence(SoundPriority priority)
    {
        foreach (string library in Libraries)
            Ask(library + StopsAll, library, priority);
    }

    /// <summary>Music is never turned away.</summary>
    public void AskMusic(string music, int wait) => _music.Ask(music, wait);

    /// <inheritdoc/>
    public void OpenUpdate(bool musicTurn)
    {
        if (musicTurn && _music.TakeTurn())
            _downtime = MusicChangeDowntime;
    }

    /// <inheritdoc/>
    public void CloseUpdate(int soundTurns)
    {
        for (int turn = 0; turn < soundTurns; turn++)
            TakeSoundTurn();

        // The program acts on what this update asked during the update's own samples; what it reports is read by
        // the next update's turns.
        foreach ((string library, string? sound) in _askedOfProgram)
            _clock.Ask(library, sound);
        _askedOfProgram.Clear();
        _clock.Advance((library, sound) => _started.Add(new AcceptedSound(sound, library)));
    }

    /// <inheritdoc/>
    public IReadOnlyList<AcceptedSound> TakeStarted()
    {
        var taken = _started.ToList();
        _started.Clear();
        return taken;
    }

    /// <summary>Nothing falls silent but by a stop taking its turn.</summary>
    public bool TakeSilence() => false;

    /// <summary>One turn of the three libraries: withdrawn alike after a change of music, and otherwise each its own exchange.</summary>
    private void TakeSoundTurn()
    {
        if (_downtime > 0)
        {
            _downtime--;
            foreach (string library in Libraries)
            {
                _askedOfProgram.Add((library, null));
                _queues[library].DropAwaited();
            }

            return;
        }

        foreach (string library in Libraries)
        {
            if (_queues[library].TakeTurn(_clock.Reported(library), out string? asked))
                _askedOfProgram.Add((library, asked));
        }
    }

    /// <summary>The library a sound belongs to: the part of its name before the first underscore.</summary>
    /// <exception cref="InvalidOperationException">The name begins with none of the three libraries'.</exception>
    private static string LibraryOf(string sound)
    {
        int end = sound.IndexOf('_');
        string library = end < 0 ? sound : sound[..end];
        return Array.IndexOf(Libraries, library) >= 0
            ? library
            : throw new InvalidOperationException($"The sound \"{sound}\" is in none of the cartridge's libraries: its name begins with none of {string.Join(", ", Libraries)}.");
    }
}
