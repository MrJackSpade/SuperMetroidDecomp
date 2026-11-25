using Engine.Core;
using Engine.Input;
using Game.Data;

namespace Game;

/// <summary>
/// Which of the buttons held on the pause screen count as repeating: those held, and not newly pressed, once what is
/// held so has stayed the same for more updates than a delay, and on every update after until it changes. It is the
/// game's and not one pause's: what was held when the last pause was left is still what it remembers when the next is
/// brought up, as on the cartridge, and it starts afresh only when the game does.
/// </summary>
public sealed class PauseHeldButtons : ISessionService
{
    private HashSet<GameAction> _heldBefore = [];
    private HashSet<GameAction> _repeating = [];
    private int? _wait;

    /// <summary>The buttons that repeat on this update of the pause; none until the delay has passed.</summary>
    public IReadOnlySet<GameAction> Repeating => _repeating;

    /// <summary>Takes one update of the pause's input and works out what repeats on it.</summary>
    /// <param name="input">The update's input.</param>
    /// <param name="delay">How many updates the held buttons wait through unchanged; they repeat from the next.</param>
    public void Take(GameInput input, int delay)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(delay);
        HashSet<GameAction> held = [.. GameAction.Standard.Concat(Actions.All).Where(action => input.IsHeld(action) && !input.WasPressed(action))];
        bool unchanged = held.SetEquals(_heldBefore);
        _heldBefore = held;
        if (!unchanged)
            _wait = null;
        else if ((_wait ??= delay + 1) > 0)
            _wait--;
        _repeating = _wait == 0 ? held : [];
    }

    /// <inheritdoc/>
    public void BeginAgain() => (_heldBefore, _repeating, _wait) = ([], [], null);

    /// <summary>It has nothing to do in play.</summary>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
