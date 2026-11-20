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
    public GameSession(GameDefinition definition, Node host, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(content);
        _host = host;
        Definition = definition;
        Content = content;
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
    public Progress Progress { get; } = new();

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
            GameMode finished = Mode;
            _host.RemoveChild(finished);
            finished.Free();
            Mode = next;
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
