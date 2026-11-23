using Engine.Actors;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
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
        Camera = new Camera(
            ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32(),
            ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32());
        Mode = definition.CreateFirstMode();
        Show(Mode);
    }

    /// <summary>The game being played.</summary>
    public GameDefinition Definition { get; }

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

    /// <summary>
    /// Starts the session's game afresh, as after a death or on loading a save: what has happened is forgotten and the
    /// player is a new one.
    /// </summary>
    public void BeginAgain()
    {
        Progress = new Progress();
        // A player left in a room went with the room; one that is in no room is done with here.
        if (_player is not null && GodotObject.IsInstanceValid(_player) && _player.GetParent() is null)
            _player.Free();
        _player = null;
        _messages.Clear();
        SaveRequested = null;
    }

    /// <summary>The view onto the room being played.</summary>
    public Camera Camera { get; }

    /// <summary>What gameplay has asked to hear.</summary>
    public SoundRequests Sounds { get; } = new();

    /// <summary>The player. One lasts the whole session, going from room to room.</summary>
    public Player Player => _player ??= Definition.CreatePlayer();

    /// <summary>The node modes and rooms are shown under.</summary>
    public Node Host => _host;

    /// <summary>The messages asked for and not yet shown, by name, in the order they were asked for.</summary>
    private readonly Queue<string> _messages = new();

    /// <summary>Asks for a message to be shown. The mode in control shows it.</summary>
    public void RequestMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _messages.Enqueue(message);
    }

    /// <summary>Takes the next message waiting to be shown, or null when none is.</summary>
    public string? TakeMessage() => _messages.Count > 0 ? _messages.Dequeue() : null;

    /// <summary>The question asked and not yet shown, with who is to be given its answer.</summary>
    private (string Message, Action<bool> Answered)? _question;

    /// <summary>
    /// Asks for a message that is answered yes or no to be shown. The mode in control shows it and gives the answer to
    /// <paramref name="answered"/>.
    /// </summary>
    public void RequestQuestion(string message, Action<bool> answered)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(answered);
        if (_question is not null)
            throw new InvalidOperationException($"The question \"{message}\" was asked while \"{_question.Value.Message}\" was still waiting to be shown.");
        _question = (message, answered);
    }

    /// <summary>Takes the question waiting to be shown, or null when none is.</summary>
    public (string Message, Action<bool> Answered)? TakeQuestion()
    {
        (string, Action<bool>)? question = _question;
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
    public GameClock Clock { get; } = new();

    /// <summary>The gameplay random generator.</summary>
    public GameRandom Random { get; }

    /// <summary>What has happened that must not be forgotten.</summary>
    public Progress Progress { get; private set; } = new();

    /// <summary>The mode in control.</summary>
    public GameMode Mode { get; private set; }

    /// <summary>
    /// Plays one update: the mode in control runs, the mode it names takes over if it is a different one, and the clock
    /// moves on.
    /// </summary>
    public void Step(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        GameMode next = Mode.Step(input)
            ?? throw new InvalidOperationException($"{Mode.GetType().Name} named no mode to be in control next.");
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

            foreach (GameMode mode in done)
            {
                _host.RemoveChild(mode);
                mode.Free();
            }

            Mode = next;
            if (next.HasTakenControl)
                next.Resume();
            else
                Show(next);
        }

        Sounds.Step();
        Clock.Advance();
    }

    /// <summary>Puts a mode on screen and gives it control.</summary>
    private void Show(GameMode mode)
    {
        _host.AddChild(mode);
        mode.TakeControl(this);
    }
}
