using Engine.Core;
using Engine.Input;

namespace Game.Tests.Movies;

/// <summary>
/// The controller a recorded movie plays the game with: the cartridge's buttons, given to the game as a controller's
/// buttons are. Each of the game's actions is down while a button bound to it is held, by the session's own options
/// (<see cref="Options.ButtonsOf"/>), so a movie that changes its controls in the options plays by them, and a menu's
/// confirming and cancelling come from the buttons the game binds them to.
/// </summary>
/// <remarks>
/// An update's input is made here from two of the cartridge's words, the buttons held and those of them it took as
/// newly pressed (<see cref="InputOf"/>), and is given to the session directly (<see cref="GameSession.Step"/>). The
/// movie says which buttons are new because the cartridge read its controller on frames the trace leaves out (a wait
/// on its sound chip), so the update before does not always tell.
/// </remarks>
public sealed class MovieController : IInputSource
{
    /// <summary>
    /// The cartridge's buttons, by their bit of its controller word, as the buttons of a controller laid out as the
    /// cartridge's is: the numbers the game's bindings name a controller's buttons by.
    /// </summary>
    private static readonly (int Bit, string Button)[] Buttons =
    [
        (0x8000, "pad:0"),  // B
        (0x4000, "pad:2"),  // Y
        (0x2000, "pad:4"),  // Select
        (0x1000, "pad:6"),  // Start
        (0x0800, "pad:11"), // Up
        (0x0400, "pad:12"), // Down
        (0x0200, "pad:13"), // Left
        (0x0100, "pad:14"), // Right
        (0x0080, "pad:1"),  // A
        (0x0040, "pad:3"),  // X
        (0x0020, "pad:9"),  // L
        (0x0010, "pad:10"), // R
    ];

    private readonly GameDefinition _definition;
    private readonly Options _options;

    /// <summary>A controller for a game of <paramref name="definition"/> played with <paramref name="options"/>.</summary>
    public MovieController(GameDefinition definition, Options options)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(options);
        (_definition, _options) = (definition, options);
    }

    /// <summary>The cartridge's buttons held now, as its controller word has them: what anything that reads the controller itself is given.</summary>
    public int Held { get; private set; }

    /// <summary>
    /// The input of an update on which the cartridge's buttons <paramref name="held"/> are held, of which it took
    /// <paramref name="pressed"/> as newly pressed: an action is newly pressed when it is down now and would not be
    /// with the newly pressed buttons let go.
    /// </summary>
    public GameInput InputOf(int held, int pressed)
    {
        Held = held;
        HashSet<GameAction> down = ActionsOf(held), before = ActionsOf(held & ~pressed);
        return new GameInput(down, down.Where(action => !before.Contains(action)));
    }

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown() => ActionsOf(Held);

    private HashSet<GameAction> ActionsOf(int buttons)
    {
        var held = new HashSet<string>(StringComparer.Ordinal);
        foreach ((int bit, string button) in Buttons)
        {
            if ((buttons & bit) != 0)
                held.Add(button);
        }

        var down = new HashSet<GameAction>();
        foreach (GameAction action in _definition.AllActions)
        {
            if (_options.ButtonsOf(action.Name).Any(held.Contains))
                down.Add(action);
        }

        return down;
    }
}
