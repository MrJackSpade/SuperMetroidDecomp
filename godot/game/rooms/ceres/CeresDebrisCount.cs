using Engine.Core;

namespace Game.Rooms.Ceres;

/// <summary>
/// The count the Ceres hall's falling pieces go by. It is the station's and no room's: the hall sets up no wait of
/// its own, and reads what it finds. The elevator shaft leaves each step of its turn as it reaches it, so a hall
/// entered from a shaft swinging forward waits that many updates for its first piece, and from one swinging back
/// drops it at once; and the hall leaves the updates before its next piece, which a hall entered again goes on from.
/// </summary>
public sealed class CeresDebrisCount : ISessionService
{
    /// <summary>What was last left: the updates before the hall's next piece, or the step of the shaft's turn.</summary>
    private int _left;

    /// <summary>Whether it was left by the shaft swinging back, which the hall reads as already run out.</summary>
    private bool _runOut;

    /// <summary>
    /// How many updates the hall waits for its first piece, counting the one it falls on: one more than what was
    /// left, or the very first when the shaft was swinging back.
    /// </summary>
    public int FirstWait => _runOut ? 1 : _left + 1;

    /// <summary>The shaft leaves a step of its turn: its tilt counted from full the anticlockwise way, and whether it swings back.</summary>
    public void LeftByShaft(int step, bool swingingBack)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(step);
        (_left, _runOut) = (step, swingingBack);
    }

    /// <summary>The hall leaves the updates before its next piece.</summary>
    public void LeftByHall(int updatesBeforeNext)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updatesBeforeNext);
        (_left, _runOut) = (updatesBeforeNext, false);
    }

    /// <inheritdoc/>
    public void BeginAgain() => (_left, _runOut) = (0, false);

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
