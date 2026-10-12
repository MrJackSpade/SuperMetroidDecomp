using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Selects one of Boyon's two ROM movement functions at <c>$A2:8718</c>. The values are
/// table indexes, not invented host states: zero dispatches falling and one dispatches
/// rising.
/// </summary>
public enum BoyonBounceMovement : ushort
{
    /// <summary>Native table index zero: advance down the speed curve until the stored spawn-height baseline is reached.</summary>
    Falling = 0,
    /// <summary>Native table index one: retreat up the speed curve until the wrapped index selects falling.</summary>
    Rising = 1,
}

/// <summary>
/// Typed view of Boyon's six ordinary enemy variables plus its six parallel extended-WRAM
/// words. The ordinary words deliberately remain backed by <see cref="RoomEnemySlot"/> so
/// a debugger shows the same storage layout as <c>$0FA8-$0FB2</c>.
/// </summary>
public sealed class BoyonEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BoyonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Low-byte multiplier selected by parameter-one's low byte.</summary>
    public ushort SpeedMultiplier
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Latest unsigned product of the curve byte and speed multiplier.</summary>
    public ushort Speed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Target triangular distance in 1/256-pixel units.</summary>
    public ushort JumpHeight
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Wrapped accumulator used only while deriving the initial curve index.</summary>
    public ushort DistanceAccumulator
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Current byte index into the 23-entry triangular speed curve.</summary>
    public ushort SpeedTableIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native one-word latch preventing a second animation restart mid-bounce.</summary>
    public bool Bouncing
    {
        get => _slot.VariableF != 0;
        internal set => _slot.VariableF = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Curve index at which falling reaches the spawn-height baseline.</summary>
    public ushort InitialBounceSpeedTableIndex { get; internal set; }

    /// <summary>Current indirect movement-table selection.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7804</c>. Set at the baseline and cleared while Samus is close enough
    /// to request another bounce.
    /// </summary>
    public bool BounceDisabled { get; internal set; }

    /// <summary>Per-frame copy restored before movement, matching <c>$7E:7806</c>.</summary>
    public ushort SpeedMultiplierMirror { get; internal set; }

    /// <summary>Prevents the idle instruction list from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }
}
