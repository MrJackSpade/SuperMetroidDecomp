using Engine.Content;
using Engine.Core;
using Engine.World;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's waits for its sounds, which are part of how the game plays: a door waits for the sounds and the
/// music waiting to run out, and the speed booster reads how many sounds were waiting. They are kept here as a count
/// and nothing more. Every sound belongs to one of three libraries, told by the first part of its name ("samus",
/// "enemy", "room"), and is counted as waiting its turn in that library's queue (<see cref="SoundLibraryQueue"/>);
/// the sound program is counted as taking up what it is asked on its own clock (<see cref="SoundProgramClock"/>);
/// music waits in a queue of its own (<see cref="MusicQueue"/>), and a change of music withdraws what every library
/// asks for eight turns.
/// <para>
/// What is heard does not go through the queues. A sound is heard on the update it is asked for, on the channel of
/// its library, whatever its priority and however many wait; only a door's passage, a demonstration or a power
/// bomb's blast keeps it from being heard, as the game turns sounds away then. The queues say how long play waits,
/// never what a player hears.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// An update opens with the music's turn, the mode plays it, asking as it goes, and it closes with the sounds' turns
/// and the program's clock. What the program reports back is read by the next update's turns.
/// </para>
/// <para>
/// Music comes in sets, one loaded at a time. A room whose version names another set than the one loaded has the
/// music stopped and that set loaded; its own music is then asked for unless it keeps what is playing or that is
/// playing already. Through a door only the set is asked for as the room is set up, and the room's music when the
/// door's passage says so (<see cref="AskRoomMusic(Room)"/>).
/// </para>
/// </remarks>
public sealed class CartridgeSounds : ISoundFlow
{
    /// <summary>The turns of the sounds, after the music changes, on which what every library asks is withdrawn.</summary>
    private const int MusicChangeDowntime = 8;

    /// <summary>What ends the name of the sound that stops everything its library is sounding.</summary>
    private const string StopsAll = "_stop";

    /// <summary>The data file of the sets of music: the set loaded at power-on, and each set's theme.</summary>
    private const string SetsFile = "audio/music_sets.json";

    /// <summary>
    /// The data file of the sounds a set of music plays differently: for each such sound, the sets that have a file
    /// of it of their own, kept in a folder of the sounds named for the set.
    /// </summary>
    private const string VersionsFile = "audio/sound_versions.json";

    /// <summary>For each sound some set of music plays differently, the sets that do.</summary>
    private readonly Dictionary<string, HashSet<string>> _versions = new(StringComparer.Ordinal);

    /// <summary>
    /// The data file of the tracks a set of music keeps differently: for each such track, the sets that have a file
    /// of it of their own, kept in a folder of the music named for the set.
    /// </summary>
    private const string MusicVersionsFile = "audio/music_versions.json";

    /// <summary>For each track some set of music keeps differently, the sets that do.</summary>
    private readonly Dictionary<string, HashSet<string>> _musicVersions = new(StringComparer.Ordinal);

    /// <summary>The libraries, in the order their turns are taken and the program serves them.</summary>
    internal static readonly string[] Libraries = ["samus", "enemy", "room"];

    private readonly Dictionary<string, SoundLibraryQueue> _queues =
        Libraries.ToDictionary(library => library, _ => new SoundLibraryQueue(), StringComparer.Ordinal);

    /// <summary>The sound program the libraries' exchanges are with.</summary>
    private readonly ISoundProgram _program;

    /// <summary>How long a set of music takes to load.</summary>
    private readonly IMusicLoadTime _loads;
    private readonly DataNode _themes;
    private readonly string _setAtPowerOn;
    private readonly MusicQueue _music;
    private readonly List<(string Library, string? Sound)> _askedOfProgram = [];
    private readonly List<AcceptedSound> _started = [];
    private string _lastRoomAsked = "";
    private bool _programDropsAsked;
    private int _downtime;

    /// <summary>
    /// The cartridge's flow at power-on, with the sets of music read from the game's data, its libraries' exchanges
    /// made with <paramref name="program"/> and its sets of music loaded in the time <paramref name="loads"/> gives.
    /// </summary>
    public CartridgeSounds(ContentLibrary content, ISoundProgram program, IMusicLoadTime loads)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(loads);
        _program = program;
        _loads = loads;
        DataNode sets = content.ReadData(SetsFile);
        _themes = sets.Get("themes");
        _setAtPowerOn = sets.Text("loaded_at_start");
        _setNamed = _setAtPowerOn;
        _music = new MusicQueue(_setAtPowerOn);
        foreach ((string sound, DataNode setsWithOne) in content.ReadData(VersionsFile).Entries())
            _versions[sound] = [.. setsWithOne.Items().Select(set => set.AsText())];
        foreach ((string track, DataNode setsWithOne) in content.ReadData(MusicVersionsFile).Entries())
            _musicVersions[track] = [.. setsWithOne.Items().Select(set => set.AsText())];
    }

    /// <summary>
    /// The set of music the room being played, or the scene, last named: the power-on set until one does. It is what
    /// a sound's version is chosen by, and is no part of the queues: it changes as the set is named, not as its
    /// load takes its turn.
    /// </summary>
    private string _setNamed;

    /// <summary>
    /// The file of a sound as it is heard where it is asked for: some sounds have another version for a set of
    /// music, kept in a folder named for the set, and the version heard is that of the set the room or the scene has
    /// named.
    /// </summary>
    private string AsHeardHere(string sound) =>
        _versions.TryGetValue(sound, out HashSet<string>? sets) && sets.Contains(_setNamed) ? $"{_setNamed}/{sound}" : sound;

    /// <summary>
    /// Puts the queues back as at power-on, as the game does when it starts over at its title: every sound and
    /// piece of music waiting is forgotten, each library is ready, the music is stopped with the power-on set taken
    /// as loaded, and the room last asked about is forgotten. The sound program's own clock goes on; on its next
    /// update it drops what it was asked for, and the enemies' library withdraws what it asked.
    /// </summary>
    public void Reset()
    {
        foreach (SoundLibraryQueue queue in _queues.Values)
            queue.Reset();
        _music.Reset(_setAtPowerOn);
        _setNamed = _setAtPowerOn;
        _downtime = 0;
        _lastRoomAsked = "";
        SavedGameAppears = false;
        _askedOfProgram.Clear();
        _programDropsAsked = true;
        _askedOfProgram.Add(("enemy", null));
    }

    /// <summary>The flow of <paramref name="session"/>, which is this game's.</summary>
    public static CartridgeSounds Of(GameSession session) => (CartridgeSounds)session.Sounds.Flow;

    /// <summary>
    /// Whether a door's passage is turning sounds away: from its stops being asked for until the next room has faded
    /// in. Sounds and music already waiting still take their turns.
    /// </summary>
    public bool DoorTurnsSoundsAway { get; set; }

    /// <summary>Whether a demonstration is playing, which turns every sound away.</summary>
    public bool DemoPlaying { get; set; }

    /// <summary>The room last entered, whose power bombs say whether a blast is under way.</summary>
    private Room? _room;

    /// <summary>
    /// Whether a power bomb's blast, or the crystal flash's, is under way in the room being played, which turns every
    /// sound away: from the blast's start, after its own sound is asked for, until its last reach is done, before the
    /// sounds it turned away are asked for again. A blast that goes with its room is over with it.
    /// </summary>
    public bool PowerBombExploding =>
        _room is { } room && Godot.GodotObject.IsInstanceValid(room) && room.Projectiles.OfType<PowerBomb>().Any(bomb => bomb.IsExploding);

    /// <inheritdoc/>
    /// <remarks>
    /// A track the set of music loaded keeps differently from the rest is named with the set's folder before it, which
    /// is where its file for that set is: the cartridge plays a track as the set loaded holds it.
    /// </remarks>
    public string Music =>
        _musicVersions.TryGetValue(_music.Playing, out HashSet<string>? sets) && sets.Contains(_music.LoadedSet) ? $"{_music.LoadedSet}/{_music.Playing}" : _music.Playing;

    /// <inheritdoc/>
    public bool HasWaitingSounds => _queues.Values.Any(queue => queue.Waiting > 0);

    /// <inheritdoc/>
    public bool HasWaitingMusic => _music.HasWaiting;

    /// <summary>
    /// A sound asked for. It is heard at once, on its library's channel, unless the game is turning sounds away; and
    /// it is counted into its library's queue for the waits play keeps, its priority tried against how many wait and
    /// one let in dropped all the same while sounds are turned away. What the count answers is what is returned. The
    /// channel its asker named is not used: its library is its channel.
    /// </summary>
    public SoundAdmission Ask(string sound, string channel, SoundPriority priority)
    {
        string library = LibraryOf(sound);
        bool turnedAway = DoorTurnsSoundsAway || DemoPlaying || PowerBombExploding;
        if (!turnedAway)
            _started.Add(new AcceptedSound(AsHeardHere(sound), library));
        return _queues[library].Admit(sound, priority, turnedAway);
    }

    /// <summary>Asks each library, in order, for the sound that stops everything it is sounding.</summary>
    public void AskSilence(SoundPriority priority)
    {
        foreach (string library in Libraries)
            Ask(library + StopsAll, library, priority);
    }

    /// <summary>Music is never turned away.</summary>
    public void AskMusic(string music, int wait) => _music.AskMusic(music, wait);

    /// <summary>How many of the music's turns a fanfare is given before silence is asked for behind it.</summary>
    private const int FanfareTurns = 360;

    /// <summary>
    /// The cartridge's fanfare: the piece playing is remembered and everything waiting in the music's queue forgotten;
    /// then the fanfare is asked for, silence once it has had its turns, and the remembered piece behind that. The
    /// music comes back by that count, whatever becomes of the message the fanfare came with.
    /// </summary>
    public void AskFanfare(string fanfare)
    {
        string playing = _music.Playing;
        _music.Clear();
        _music.AskMusic(fanfare, 0);
        _music.AskMusic("", FanfareTurns);
        _music.AskMusic(playing, 0);
    }

    /// <summary>The fanfare's own count brings the music back: a message's closing does nothing to it.</summary>
    public void EndFanfare()
    {
    }

    /// <summary>
    /// Asks for the set of music <paramref name="set"/> to be loaded, behind the music waiting. Loading it stops
    /// the music.
    /// </summary>
    public void AskMusicSet(string set)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(set);
        ThemeOf(set);
        _setNamed = set;
        _music.AskSet(set);
    }

    /// <summary>
    /// Whether the next room entered is the one a saved game appears in: Samus's fanfare is asked for there, and the
    /// room's own music behind it (<see cref="EnterRoom"/>). Whoever loads the saved game says so before its room
    /// is set up.
    /// </summary>
    public bool SavedGameAppears { get; set; }

    /// <summary>How many turns of the music's queue Samus's fanfare waits as a saved game appears, and the room's music behind it.</summary>
    private const int FanfareWait = 14, MusicAfterFanfareWait = 365;

    /// <summary>
    /// A room's version changes the music only when it is another room's or another version than the last one asked
    /// about, and never during a demonstration, whose music is the title's. Through a door only its set is asked for
    /// here; otherwise its set and then its music.
    /// </summary>
    public void EnterRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _room = room;
        bool appears = SavedGameAppears;
        SavedGameAppears = false;
        if (DemoPlaying || room.Version is not { } version)
            return;
        if (version.MusicSet.Length > 0)
            _setNamed = version.MusicSet;
        string asked = $"{room.SceneFilePath}#{room.Versions.IndexOf(version)}";
        if (appears)
        {
            // The room's set is asked for where it is not the one loaded, then her fanfare, and behind it what the
            // room chooses: its music, or silence where it keeps or silences its own. The room's version is taken
            // as asked about, so its music does not take the fanfare's place at once.
            _lastRoomAsked = asked;
            AskRoomSet(version);
            _music.AskMusic(Data.Music.SamusFanfare, FanfareWait);
            _music.AskMusic(version.KeepsMusic ? "" : version.MusicIsTheme ? ThemeOf(_music.SetLoadedAfterWaiting) : version.Music, MusicAfterFanfareWait);
            return;
        }

        if (asked == _lastRoomAsked)
            return;
        _lastRoomAsked = asked;
        bool setChanges = AskRoomSet(version);
        if (!DoorTurnsSoundsAway && RoomMusicIsAsked(version, setChanges))
            AskRoomMusic(version);
    }

    /// <summary>
    /// Asks for the music of the room a door has led to, as the door's passage finishes its wait for the music:
    /// unless the room keeps what is playing, or its music is playing already in the set it names. A room that
    /// names no set is taken to differ from the one loaded, as the cartridge takes it.
    /// </summary>
    public void AskRoomMusic(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (room.Version is { } version && RoomMusicIsAsked(version, version.MusicSet != _music.LoadedSet))
            AskRoomMusic(version);
    }

    /// <inheritdoc/>
    public void OpenUpdate(bool musicTurn)
    {
        if (musicTurn)
            TakeMusicTurn();
    }

    /// <summary>One turn of the music's queue, with what a change of music or a set's load does to the sounds.</summary>
    private void TakeMusicTurn()
    {
        switch (_music.TakeTurn())
        {
            case MusicQueue.Turn.MusicChanged:
                _downtime = MusicChangeDowntime;
                break;
            case MusicQueue.Turn.SetLoaded:
                // Loading a set stops the sound program: what it was asked for is dropped. The time the load takes
                // passes on the clock, with no update played.
                _programDropsAsked = true;
                _downtime = MusicChangeDowntime;
                CountLoad(_loads.UpdatesToLoad(_music.LoadedSet));
                break;
        }
    }

    /// <summary>Passes the time a set's load took on the clock of the session whose room is being played.</summary>
    private void CountLoad(int updates)
    {
        if (updates == 0)
            return;
        Room room = _room is { } entered && Godot.GodotObject.IsInstanceValid(entered)
            ? entered
            : throw new InvalidOperationException($"The set of music \"{_music.LoadedSet}\" took {updates} updates to load before any room was entered: there is no clock to pass them on.");
        room.Session.Clock.Pass(updates);
    }

    /// <inheritdoc/>
    public void CloseUpdate(ClosingSoundWork work)
    {
        if (work.MusicTurn)
            TakeMusicTurn();
        for (int turn = 0; turn < work.SoundTurns; turn++)
            TakeSoundTurn();

        // The program acts on what this update asked during the update's own samples; what it reports is read by
        // the next update's turns. A set's load came first, on the music's turn.
        if (_programDropsAsked)
            _program.DropAsked();
        _programDropsAsked = false;
        foreach ((string library, string? sound) in _askedOfProgram)
            _program.Ask(library, sound);
        _askedOfProgram.Clear();
        _program.Advance();
    }

    /// <inheritdoc/>
    public IReadOnlyList<AcceptedSound> TakeStarted()
    {
        var taken = _started.ToList();
        _started.Clear();
        return taken;
    }

    /// <summary>Nothing falls silent but by a stop being asked for.</summary>
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
            if (_queues[library].TakeTurn(_program.Reported(library), out string? asked))
                _askedOfProgram.Add((library, asked));
        }
    }

    /// <summary>
    /// Asks for a room's set where it names one that is not loaded: the music is stopped and the set loaded, each
    /// in its turn. True when it was asked for.
    /// </summary>
    private bool AskRoomSet(RoomVersion version)
    {
        if (version.MusicSet.Length == 0 || version.MusicSet == _music.LoadedSet)
            return false;
        _music.AskMusic("", 0);
        AskMusicSet(version.MusicSet);
        return true;
    }

    /// <summary>
    /// Whether a room's music is asked for: never where it keeps what is playing, always where it is silent, and
    /// otherwise when its set differs or its music, or the loaded set's theme where it asks for the theme, is not
    /// what is playing.
    /// </summary>
    private bool RoomMusicIsAsked(RoomVersion version, bool setDiffers)
    {
        if (version.KeepsMusic)
            return false;
        if (version.MusicIsTheme)
            return setDiffers || ThemeOf(_music.LoadedSet) != _music.Playing;
        return version.Music.Length == 0 || setDiffers || version.Music != _music.Playing;
    }

    /// <summary>
    /// Asks for a room's music with the shortest wait: its piece, or silence, or where it asks for the theme, the
    /// theme of the set that will be loaded when its turn comes.
    /// </summary>
    private void AskRoomMusic(RoomVersion version) =>
        _music.AskMusic(version.MusicIsTheme ? ThemeOf(_music.SetLoadedAfterWaiting) : version.Music, 0);

    /// <summary>The theme of a set of music, by the set's name.</summary>
    private string ThemeOf(string set) => _themes.Text(set);

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
