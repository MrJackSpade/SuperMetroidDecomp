namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Ceres elevator shaft's last turn sets the pre-elevator hall's first wait for falling debris (#1470, #2572). The
/// shaft leaves each turn step it reaches, its tilt counted from the full anticlockwise tilt, marked while its sweep runs
/// backward; the hall, which sets up no wait of its own, reads what it finds as a countdown to its first falling tile, so
/// a step of the forward sweep waits that many updates and any step of the backward sweep reads as already run past
/// zero, dropping the first tile at once. The hall then leaves its own countdown, the updates before its next tile, which
/// a hall entered again reads the same way.
/// </summary>
/// <remarks>
/// Left by the shaft when its elevator door's setup starts the turn and on every update that turns it, and by the hall on
/// every update it counts; read by the hall when the room loads. The value is the one the room mains share
/// (<see cref="RoomMainCarryover"/>), so whatever main ran last sets the hall's first wait. Evidence: the Ceres
/// pre-elevator hall's debris timer (<c>$8F:E525</c>) starts from the room mains' shared word, which the shaft's turn
/// (<c>$89:AD41</c>) leaves as its turn index with the sign bit marking the backward sweep.
/// </remarks>
/// <param name="carryover">The value the room mains share.</param>
[CompatibilityService("the Ceres pre-elevator hall's first falling tile waits as long as the elevator shaft's last turn step reads as a countdown, and the hall leaves its own countdown for the next room's main")]
public sealed class ShaftTurnSetsDebrisWait(RoomMainCarryover carryover)
{
    /// <summary>The mark a turn step carries while the turn sweeps backward, which a countdown reads as run past zero.</summary>
    private const int BackwardMark = 1 << 15;

    /// <summary>The value the room mains share.</summary>
    private readonly RoomMainCarryover _carryover = Ensure.NotNull(carryover);

    /// <summary>Leaves the shaft's turn step for the next room's main.</summary>
    /// <param name="step">The tilt counted from the full anticlockwise tilt, 0 through 32767.</param>
    /// <param name="backward">Whether the turn is sweeping backward; the hall reads such a step as already run out.</param>
    public void LeaveTurnStep(int step, bool backward) =>
        _carryover.Leave(Ensure.BetweenInclusive(0, BackwardMark - 1, step) | (backward ? BackwardMark : 0));

    /// <summary>The hall's first wait: what the previous room's main left, read as a countdown.</summary>
    public WrappingCountdown FirstDebrisWait => WrappingCountdown.Start(_carryover.Value);

    /// <summary>Leaves the hall's countdown to its next falling tile for the next room's main.</summary>
    /// <param name="wait">The updates before the hall's next tile.</param>
    public void LeaveDebrisWait(WrappingCountdown wait) => _carryover.Leave(wait.RemainingFrames);
}
