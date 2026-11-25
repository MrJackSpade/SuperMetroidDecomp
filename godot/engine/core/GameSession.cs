using Engine.Actors;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Engine.Core;

/// <summary>
/// One running game: the things that last for as long as it is played, built from the <see cref="GameDefinition"/> and
/// handed to whatever needs them. It holds no rules of play and names nothing of a particular game.
/// </summary>
public sealed class GameSession
{
    /// <summary>The node the mode in control is shown under.</summary>
    private readonly Node _host;

    /// <summary>The player, once one has been asked for.</summary>
    private Player? _player;

    /// <summary>
    /// Starts a session of <paramref name="definition"/>'s game, showing its modes under <paramref name="host"/> and
    /// loading its assets through <paramref name="content"/>.
    /// </summary>
    public GameSession(GameDefinition definition, Node host, ContentLibrary content, SaveFile saves, Options options)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(saves);
        ArgumentNullException.ThrowIfNull(options);
        _host = host;
        Definition = definition;
        Content = content;
        Saves = saves;
        Options = options;
        Random = new GameRandom(definition.CreateRandomSequence());
        Services = definition.CreateServices();
        ScrollCells = definition.CreateScrollCells(this);
        Clock = new GameClock(definition.ClockPeriod, definition.ShortClockPeriod);
        Sounds = new SoundRequests(definition.CreateSoundFlow(content));
        int viewHeight = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
        Camera = new Camera(ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(), viewHeight)
        {
            LowerRest = Camera.Screen - viewHeight - definition.ViewRestsShortBy,
            ClosedScreenDrift = definition.ClosedScreenDrift,
        };
        Darkness = new ScreenDarkness(host, definition.ScreenDarknessLevels);
        Mode = definition.CreateFirstMode();
        Show(Mode);
        Darkness.DrawWhile(KeepsPlay(Mode));
    }

    /// <summary>Whether <paramref name="mode"/> is play, or keeps play beneath it.</summary>
    private static bool KeepsPlay(GameMode mode)
    {
        for (GameMode? kept = mode; kept is not null; kept = kept.Keeps)
        {
            if (kept is PlayingMode)
                return true;
        }

        return false;
    }

    /// <summary>The game being played.</summary>
    public GameDefinition Definition { get; }

    /// <summary>The session-long services the game adds.</summary>
    public IReadOnlyList<ISessionService> Services { get; }

    /// <summary>The game's service of type <typeparamref name="T"/>.</summary>
    /// <exception cref="InvalidOperationException">The game has no such service.</exception>
    public T Service<T>()
        where T : class, ISessionService =>
        Services.OfType<T>().FirstOrDefault()
        ?? throw new InvalidOperationException($"The game has no session service of type {typeof(T).Name}.");

    /// <summary>Where the game's assets are loaded from.</summary>
    public ContentLibrary Content { get; }

    /// <summary>The save slots.</summary>
    public SaveFile Saves { get; }

    /// <summary>The player's settings.</summary>
    public Options Options { get; }

    /// <summary>The save slot this session's game is saved to, numbered from 1.</summary>
    public int Slot { get; set; } = 1;

    /// <summary>The game's words in the language they were last asked for in.</summary>
    private GameText? _text;

    /// <summary>The language <see cref="_text"/> is in.</summary>
    private string _textLanguage = "";

    /// <summary>The game's words in the player's language, read from the language's file in the game's text folder.</summary>
    public GameText Text
    {
        get
        {
            if (_text is null || _textLanguage != Options.Language)
            {
                _text = GameText.Read(Content.ReadData($"{Definition.TextFolder}/{Options.Language}.json"));
                _textLanguage = Options.Language;
            }

            return _text;
        }
    }

    /// <summary>The font the game's words are drawn in, once it has been asked for.</summary>
    private PixelFont? _font;

    /// <summary>
    /// The font the game's words are drawn in where nothing says otherwise: the one the game definition names, or
    /// none for a game that names none, whose words cannot be drawn.
    /// </summary>
    public PixelFont? Font => Definition.FontFile.Length == 0 ? null : _font ??= PixelFont.Read(Content, Definition.FontFile);

    /// <summary>
    /// Starts the session's game afresh, as after a death or on loading a save: what has happened is forgotten and the
    /// player is a new one.
    /// </summary>
    public void BeginAgain()
    {
        Progress = new Progress();
        EnemyPasses = 0;
        // A player left in a room went with the room; one that is in no room is done with here.
        if (_player is not null && GodotObject.IsInstanceValid(_player) && _player.GetParent() is null)
            _player.Free();
        _player = null;

        // The view's measure of how far the player last moved across starts again with the new player.
        Camera.TakePlayerAsNew();

        // The screen is fully lit again, its fade at no pace, whatever the game before left it.
        Darkness.Restart(0, 0);
        _messages.Clear();
        (_pauseRequested, _whenPauseOver) = (false, null);
        SaveRequested = null;
        foreach (ISessionService service in Services)
            service.BeginAgain();
    }

    /// <summary>
    /// Starts the session's game afresh and, when the session's save slot holds a save, puts the saved game into it.
    /// Returns the save, which says where play resumes, or null when the slot holds none.
    /// </summary>
    public SaveData? LoadSlot()
    {
        BeginAgain();
        if (!Saves.Has(Slot))
            return null;
        SaveData save = Saves.Read(Slot);
        save.ApplyTo(this);
        return save;
    }

    /// <summary>The view onto the room being played.</summary>
    public Camera Camera { get; }

    /// <summary>
    /// How dark the screen is beneath whatever shows play. It is the session's, so it outlasts whichever mode set it;
    /// it is drawn only while play is on screen or kept beneath the mode in control.
    /// </summary>
    public ScreenDarkness Darkness { get; }

    /// <summary>
    /// Opens an update: each session service that opens one plays its part (<see cref="IUpdateOpener"/>). A room opens
    /// its own updates with this; a mode whose update is no room's calls it where its update opens as one does.
    /// </summary>
    public void OpenUpdate()
    {
        foreach (IUpdateOpener opener in Services.OfType<IUpdateOpener>())
            opener.OpenUpdate(this);
    }

    /// <summary>How each screen of the room being played lets the view in.</summary>
    public IScrollCells ScrollCells { get; }

    /// <summary>What gameplay has asked to hear.</summary>
    public SoundRequests Sounds { get; }

    /// <summary>The player. One lasts the whole session, going from room to room.</summary>
    public Player Player => _player ??= Definition.CreatePlayer();

    /// <summary>The node modes and rooms are shown under.</summary>
    public Node Host => _host;

    /// <summary>The messages asked for and not yet shown, by name, in the order they were asked for.</summary>
    private readonly Queue<(string Message, string Fanfare, int HoldUpdates)> _messages = new();

    /// <summary>Asks for a message to be shown. The mode in control shows it.</summary>
    /// <param name="message">The message, by name.</param>
    /// <param name="fanfare">Music played while it shows, by name, after which the music there was comes back; empty for none.</param>
    /// <param name="holdUpdates">How many updates it stays up before it can be dismissed.</param>
    public void RequestMessage(string message, string fanfare = "", int holdUpdates = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(fanfare);
        ArgumentOutOfRangeException.ThrowIfNegative(holdUpdates);
        _messages.Enqueue((message, fanfare, holdUpdates));
    }

    /// <summary>Whether a message or a question has been asked for and not yet shown.</summary>
    public bool HasMessageWaiting => _messages.Count > 0 || _question is not null;

    /// <summary>Takes the next message waiting to be shown, with its fanfare and how long it is held, or null when none is.</summary>
    public (string Message, string Fanfare, int HoldUpdates)? TakeMessage() => _messages.Count > 0 ? _messages.Dequeue() : null;

    private bool _pauseRequested;

    /// <summary>What whoever asked for the pause left to be done as it ends; null for nothing.</summary>
    private Action? _whenPauseOver;

    /// <summary>
    /// Asks for the game to be paused, on its pause screen, as soon as no message is waiting to be shown.
    /// <paramref name="whenOver"/> is done as that pause ends, before play shows again: a station that holds the
    /// player through the pause it asks for lets them go so.
    /// </summary>
    public void RequestPause(Action? whenOver = null)
    {
        _pauseRequested = true;
        _whenPauseOver = whenOver;
    }

    /// <summary>
    /// Says a pause has ended, before play shows again: what whoever asked for it left to be done then is done. The
    /// mode that was the pause says so, whoever asked for it.
    /// </summary>
    public void EndPause()
    {
        Action? whenOver = _whenPauseOver;
        _whenPauseOver = null;
        whenOver?.Invoke();
    }

    /// <summary>Whether a pause was asked for; asking takes the request.</summary>
    public bool TakePauseRequest()
    {
        bool requested = _pauseRequested;
        _pauseRequested = false;
        return requested;
    }

    /// <summary>The question asked and not yet shown, with who is to be given its answer.</summary>
    private (string Message, Action<bool> Answered, MessageAfterYes? AfterYes)? _question;

    /// <summary>
    /// Asks for a message that is answered yes or no to be shown. The mode in control shows it and gives the answer to
    /// <paramref name="answered"/>, after what <paramref name="afterYes"/> says follows a yes, if anything does.
    /// </summary>
    public void RequestQuestion(string message, Action<bool> answered, MessageAfterYes? afterYes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(answered);
        if (_question is not null)
            throw new InvalidOperationException($"The question \"{message}\" was asked while \"{_question.Value.Message}\" was still waiting to be shown.");
        _question = (message, answered, afterYes);
    }

    /// <summary>Takes the question waiting to be shown, with what follows a yes to it, or null when none is.</summary>
    public (string Message, Action<bool> Answered, MessageAfterYes? AfterYes)? TakeQuestion()
    {
        (string, Action<bool>, MessageAfterYes?)? question = _question;
        _question = null;
        return question;
    }

    /// <summary>Where the game was last asked to be saved: the room's scene and the entry point in it. Null until it is asked.</summary>
    public (string Room, string EntryPoint)? SaveRequested { get; private set; }

    /// <summary>Asks for the game to be saved, to resume in <paramref name="room"/> at <paramref name="entryPoint"/>.</summary>
    public void RequestSave(string room, string entryPoint) => SaveRequested = (room, entryPoint);

    /// <summary>Takes the request to save, if there is one.</summary>
    public (string Room, string EntryPoint)? TakeSaveRequest()
    {
        (string, string)? request = SaveRequested;
        SaveRequested = null;
        return request;
    }

    /// <summary>The count of updates played.</summary>
    public GameClock Clock { get; }

    /// <summary>The gameplay random generator.</summary>
    public GameRandom Random { get; }

    /// <summary>What has happened that must not be forgotten.</summary>
    public Progress Progress { get; private set; } = new();

    /// <summary>The mode in control.</summary>
    public GameMode Mode { get; private set; }

    /// <summary>
    /// How many passes the rooms' enemies have had since the game was last begun: one for each update of a room, and
    /// one for each update of a passage between rooms on which the enemies take their turns. It is kept from room to
    /// room, and read by what is timed by the enemies' passes and not by the clock.
    /// </summary>
    public long EnemyPasses { get; private set; }

    /// <summary>Counts an enemies' pass that has just ended. The room does this.</summary>
    internal void CountEnemyPass() => EnemyPasses++;

    /// <summary>
    /// The number of the update being played, counted from the session's start whatever the clock counts: for telling
    /// one update from another, not for timing anything.
    /// </summary>
    public long UpdateNumber { get; private set; }

    /// <summary>
    /// Plays one update: the mode in control runs, the mode it names takes over if it is a different one, and the clock
    /// moves on.
    /// </summary>
    public void Step(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // What of the sounds' own work the update does is the mode's to say: the music's turn as it begins, and
        // what closes it once the mode has played it.
        Sounds.OpenUpdate(Mode.GivesMusicItsTurn);
        Darkness.OpenUpdate();
        GameMode next = Mode.Step(input)
            ?? throw new InvalidOperationException($"{Mode.GetType().Name} named no mode to be in control next.");
        ClosingSoundWork closing = Mode.SoundWorkAtClose;
        bool counts = Mode.CountsOnClock;
        if (!ReferenceEquals(next, Mode))
        {
            // A mode the next one is laid over is kept, on screen, to be handed back to. The mode that finished, and
            // anything it was laid over, is done with unless the next mode is it or keeps it.
            var kept = new List<GameMode>();
            for (GameMode? mode = next; mode is not null; mode = mode.Keeps)
                kept.Add(mode);
            var done = new List<GameMode>();
            for (GameMode? mode = Mode; mode is not null; mode = mode.Keeps)
            {
                if (!kept.Contains(mode))
                    done.Add(mode);
            }

            // The player lasts the session. One standing in a room that goes with a mode done with is taken out of
            // it first, as on leaving any room, to go on to whatever room comes next; a player the session has already
            // replaced goes with its room.
            if (_player is not null && GodotObject.IsInstanceValid(_player) && _player.GetParent() is Room left
                && done.Any(mode => mode.IsAncestorOf(left)))
            {
                left.Teardown();
            }

            foreach (GameMode mode in done)
            {
                _host.RemoveChild(mode);
                mode.Free();
            }

            Mode = next;
            if (next.HasTakenControl)
            {
                next.Resume();
            }
            else
            {
                // A new mode laid over modes that have not had control either brings them with it: each is put on
                // screen and enters, from the one at the bottom up, and the mode handed to last.
                for (int index = kept.Count - 1; index >= 0; index--)
                {
                    if (!kept[index].HasTakenControl)
                        Show(kept[index]);
                }
            }

            Darkness.DrawWhile(KeepsPlay(Mode));
        }

        Sounds.CloseUpdate(closing);
        if (counts)
            Clock.Advance();
        UpdateNumber++;
    }

    /// <summary>Puts a mode on screen and gives it control.</summary>
    private void Show(GameMode mode)
    {
        _host.AddChild(mode);
        mode.TakeControl(this);

        // Said on the console, so that whoever plays can name where they were when they report something.
        GD.Print($"[update {UpdateNumber}] in control: {mode.GetType().Name}");
    }
}
