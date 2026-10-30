namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The working value a room's main routine leaves behind for the next room's (#1470). Each room main keeps its own state;
/// a main that does not set its value up itself starts from what the previous room's main left, read in its own terms.
/// The Ceres hall's falling debris first waits as long as the elevator shaft's last turn step reads as a countdown, an
/// escape room whose setup does not clear its diagonal quake carries on the previous room's, and the Maridia elevatube
/// starts its tracked position from the previous room's leftover fraction.
/// </summary>
/// <remarks>
/// Consulted when a room main is set up for a room (it adopts the value) and whenever a room main changes its value (it
/// leaves the new one). Evidence: the original game's room mains share one working word that room loading never
/// clears; the Ceres pre-elevator hall's debris timer (<c>$8F:E525</c>) starts from the shaft's turn index
/// (<c>$89:AD41</c>), and the elevatube's door callbacks (<c>$8F:E26C/$E291</c>) leave its fraction as they find it.
/// The value is held in the word's form only here; every owner reads and writes it as its own typed value.
/// </remarks>
[CompatibilityService("a room main that does not set up its working value starts from the value the previous room's main left: the Ceres hall's debris timer, an escape room's diagonal quake and the elevatube's position fraction")]
[EncodingOwner("room main working value carried between rooms")]
public sealed class RoomMainCarryover
{
    /// <summary>The bit a turn step's value carries while the turn runs backward.</summary>
    private const int BackwardBit = 1 << 15;

    /// <summary>The value left behind, 0 through 65535.</summary>
    private int value;

    /// <summary>The value read as a countdown.</summary>
    public WrappingCountdown Countdown => WrappingCountdown.FromRaw(value);

    /// <summary>The value read as the fraction of a position, 0 through 65535.</summary>
    public int Fraction => value;

    /// <summary>Leaves a countdown.</summary>
    public void LeaveCountdown(WrappingCountdown countdown) => value = countdown.RawValue;

    /// <summary>Leaves the fraction of a position.</summary>
    /// <param name="fraction">The fraction, 0 through 65535.</param>
    public void LeaveFraction(int fraction) => value = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);

    /// <summary>Leaves a step of a turn that sweeps forward and back.</summary>
    /// <param name="step">The step, 0 through 32767.</param>
    /// <param name="backward">Whether the turn is sweeping backward; a later countdown reading sees it as already expired.</param>
    public void LeaveTurnStep(int step, bool backward) =>
        value = Ensure.BetweenInclusive(0, BackwardBit - 1, step) | (backward ? BackwardBit : 0);

    /// <summary>Leaves nothing: the value is zero.</summary>
    public void Clear() => value = 0;
}
