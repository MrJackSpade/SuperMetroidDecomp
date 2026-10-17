using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Typed view of Boyon's six ordinary enemy variables plus its six parallel extended-WRAM
/// words. The ordinary words deliberately remain backed by <see cref="RoomEnemySlot"/> so
/// a debugger shows the same storage layout as <c>$0FA8-$0FB2</c>.
/// </summary>
public sealed class BoyonEnemyState
{
    /// <summary>Physical Boyon slot backing its six ordinary enemy variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Boyon's common and extended bounce state from its occupied slot.</summary>
    internal BoyonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BoyonProgram> Program { get; set; }

    /// <summary>
    /// Variable A: the speed multiplier selected by parameter one's low byte, 1 through 20 from the spawn table; only
    /// its low byte reaches the multiply. The slot variable keeps the exact value for other enemies' aliased
    /// enemy-RAM reads.
    /// </summary>
    public int SpeedMultiplier
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = checked((ushort)value);
    }

    /// <summary>
    /// Variable B: the latest product of the curve sample and the multiplier's low byte, in 256ths of a pixel per
    /// update (0 through 65025). The slot variable keeps the exact value for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int Speed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = checked((ushort)value);
    }

    /// <summary>
    /// Variable C: the arc's target height in 256ths of a pixel, 12288 through 45056 from the spawn table. It is
    /// compared with <see cref="DistanceAccumulator"/> by the sign of their word difference (<see cref="HasReachedJumpHeight"/>),
    /// so heights of 32769 and above behave as the cartridge's do. The slot variable keeps the exact value for other
    /// enemies' aliased enemy-RAM reads.
    /// </summary>
    public int JumpHeight
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>
    /// Variable D: the distance accumulated while deriving the initial curve index, in 256ths of a pixel, as the exact
    /// word the cartridge keeps (0 through 65535). It advances only through <see cref="AccumulateBounceDistance"/>.
    /// </summary>
    public int DistanceAccumulator
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = checked((ushort)value);
    }

    /// <summary>
    /// Variable E: the current entry of the triangular speed curve, as the exact word the cartridge keeps. Rising
    /// retreats it with <see cref="RetreatSpeedTableIndex"/>, which takes it from zero to <see cref="LargestSpeedTableIndex"/>
    /// at the top of the arc; falling advances it with <see cref="AdvanceSpeedTableIndex"/>, which takes it from there back
    /// to zero.
    /// </summary>
    public int SpeedTableIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = checked((ushort)value);
    }

    /// <summary>The largest curve index the word holds: rising past entry zero leaves this value.</summary>
    internal const int LargestSpeedTableIndex = 65535;

    /// <summary>
    /// Whether the accumulated distance has reached the jump height. The cartridge subtracts the height from the
    /// distance and tests the sign of the word result, so any height above 32768 is reached by a zero distance.
    /// </summary>
    internal bool HasReachedJumpHeight => !EnemyMath.IsNegative16(DistanceAccumulator - JumpHeight);

    /// <summary>Adds one update's speed to the accumulated distance, wrapping as the cartridge's word addition does.</summary>
    internal void AccumulateBounceDistance(int speed) =>
        _slot.VariableD = unchecked((ushort)(_slot.VariableD + speed));

    /// <summary>Moves one entry down the curve (falling), wrapping from <see cref="LargestSpeedTableIndex"/> to zero.</summary>
    internal void AdvanceSpeedTableIndex() =>
        _slot.VariableE = checked((ushort)(SpeedTableIndex == LargestSpeedTableIndex ? 0 : SpeedTableIndex + 1));

    /// <summary>
    /// Moves one entry up the curve (rising). The index wraps from zero to <see cref="LargestSpeedTableIndex"/>, and the
    /// result reports whether the decremented word is negative, the cartridge's signal that the top of the arc was passed.
    /// </summary>
    internal bool RetreatSpeedTableIndex()
    {
        NativeWordCounterStep step = NativeWordCounter.Decrement(_slot.VariableE);
        _slot.VariableE = step.Value;
        return step.IsNegative;
    }

    /// <summary>Native one-word latch preventing a second animation restart mid-bounce.</summary>
    public bool Bouncing
    {
        get => _slot.VariableF != 0;
        internal set => _slot.VariableF = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Curve index at which falling reaches the spawn-height baseline; the derivation ends within the curve's first 200 entries.</summary>
    public int InitialBounceSpeedTableIndex { get; internal set; }

    /// <summary>Current indirect movement-table selection.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7804</c>. Set at the baseline and cleared while Samus is close enough
    /// to request another bounce.
    /// </summary>
    public bool BounceDisabled { get; internal set; }

    /// <summary>Per-frame copy of <see cref="SpeedMultiplier"/> restored before movement, matching <c>$7E:7806</c>.</summary>
    public int SpeedMultiplierMirror { get; internal set; }

    /// <summary>Prevents the idle instruction list from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }
}
