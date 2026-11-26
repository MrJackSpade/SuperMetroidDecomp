using Engine.Core;
using Engine.Input;
using Engine.Tests.Collision;
using Engine.Tests.Content;
using Engine.Tests.Input;

namespace Engine.Tests.Core;

/// <summary>
/// A whole stand-in game played through the loop, for checks of the engine's modes. It has two rooms, "a" and "b",
/// each an open floor: "a" has the entry point "Start" and an exit "East" leading to "b"'s exit "West", which leads
/// back. Its words are "hello" and "ask".
/// </summary>
public sealed class StandInGame : IDisposable
{
    private static readonly string[] OpenRoom =
    [
        "................", "................", "................", "................", "................", "################",
    ];

    private readonly GameLoop _loop = new();

    /// <summary>A game whose first mode is made by <paramref name="firstMode"/>; play itself when none is given.</summary>
    public StandInGame(Func<GameMode>? firstMode = null)
    {
        StandInRoom.WriteAssets(Assets, OpenRoom);
        Assets.With("text/english.json", """{ "hello": "HELLO", "ask": "REALLY?" }""");
        Saves = new SaveFile(Path.Combine(Assets.Folder, "saves"), 3);
        var definition = new TwoRoomDefinition
        {
            First = firstMode ?? (() => new PlayingMode()),
            FirstRoom = "a",
            FirstEntryPoint = "Start",
        };
        _loop.Start(definition, Input, Assets.Library, Saves, new Options());
    }

    /// <summary>The game's assets.</summary>
    public TempAssets Assets { get; } = new();

    /// <summary>The game's save slots.</summary>
    public SaveFile Saves { get; }

    /// <summary>What is down on the controls.</summary>
    public SteeredInputSource Input { get; } = new();

    /// <summary>The loop.</summary>
    public GameLoop Loop => _loop;

    /// <summary>The session.</summary>
    public GameSession Session => _loop.Session;

    /// <summary>The mode in control.</summary>
    public GameMode Mode => Session.Mode;

    /// <summary>Plays <paramref name="updates"/> updates with the given actions down throughout, and nothing down afterwards.</summary>
    public void Step(int updates, params GameAction[] down)
    {
        Input.Down.Clear();
        Input.Down.UnionWith(down);
        for (int update = 0; update < updates; update++)
            _loop.Step();
        Input.Down.Clear();
    }

    /// <summary>Plays one update in which the given actions are newly pressed, having first played one with nothing down.</summary>
    public void Press(params GameAction[] pressed)
    {
        Step(1);
        Step(1, pressed);
    }

    /// <summary>Plays updates, with the given actions down, until <paramref name="reached"/> holds or a limit is passed.</summary>
    public void StepUntil(Func<bool> reached, params GameAction[] down)
    {
        for (int update = 0; update < 600; update++)
        {
            if (reached())
                return;
            Step(1, down);
        }

        throw new CheckFailedException("What the check was waiting for did not happen.");
    }

    /// <summary>Frees the game's nodes and deletes its assets.</summary>
    public void Dispose()
    {
        _loop.Free();
        Assets.Dispose();
    }
}
