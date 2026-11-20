using Engine.Input;
using Engine.Shared;
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

    /// <summary>Starts a session of <paramref name="definition"/>'s game, showing its modes under <paramref name="host"/>.</summary>
    public GameSession(GameDefinition definition, Node host)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(host);
        _host = host;
        Definition = definition;
        Random = new GameRandom(definition.CreateRandomSequence());
        Mode = definition.CreateFirstMode();
        Show(Mode);
    }

    /// <summary>The game being played.</summary>
    public GameDefinition Definition { get; }

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

        Clock.Advance();
    }

    /// <summary>Puts a mode on screen and gives it control.</summary>
    private void Show(GameMode mode)
    {
        _host.AddChild(mode);
        mode.TakeControl(this);
    }
}
