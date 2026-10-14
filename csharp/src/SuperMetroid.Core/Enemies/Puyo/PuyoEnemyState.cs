namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Typed view of Puyo's five ordinary enemy variables and eight parallel extended-WRAM
/// words. Ordinary members remain backed by <see cref="RoomEnemySlot"/> so inspecting the
/// actor shows the same layout as <c>$0FAA-$0FB2</c> in the cartridge.
/// </summary>
public sealed class PuyoEnemyState
{
    /// <summary>The actor slot that owns Puyo's five ordinary native variable words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Exposes an enemy slot's native Puyo variables through named state properties.</summary>
    internal PuyoEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Unsigned 8.8 index accumulator used to choose a row of the shared quadratic speed
    /// table. It is variable A at <c>$0FAA,x</c>, despite its misleading native name.
    /// </summary>
    public ushort YSpeedTableIndex
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Grounded-frame countdown copied from population parameter one.</summary>
    public NativeCountdown HopCooldownTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
    }

    /// <summary>Current top-level bank-$A2 function pointer.</summary>
    public PuyoEnemyFunction Function
    {
        get => (PuyoEnemyFunction)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Current airborne indirect function pointer.</summary>
    public PuyoAirborneFunction AirborneFunction
    {
        get => (PuyoAirborneFunction)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Byte offset into the seven-record ROM hop table.</summary>
    public ushort HopTableIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native <c>$7E:7800,x</c>; the record selected for the next hop.</summary>
    public PuyoHopType HopType { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7802,x</c>. The name is inherited from the disassembly: one means the
    /// hopping pose is still active, while zero permits the grounded-list handoff.
    /// </summary>
    public bool HoppingAnimationActive { get; internal set; }

    /// <summary>Native <c>$7E:7804,x</c>; zero is right and one is left.</summary>
    public PuyoDirection Direction { get; internal set; }

    /// <summary>Native <c>$7E:7806,x</c>; selects positive gravity-table rows.</summary>
    public bool Falling { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7808,x</c>. A terrain collision requests that the next hop use the
    /// precomputed opposite direction instead of re-aiming at Samus.
    /// </summary>
    public bool InvertDirection { get; internal set; }

    /// <summary>Opposite direction captured when Puyo strikes a wall or ceiling.</summary>
    public PuyoDirection InvertedDirection { get; internal set; }

    /// <summary>Three-quarter point of the initial vertical speed-table index.</summary>
    public ushort InitialYSpeedTableIndexThreeQuarters { get; internal set; }

    /// <summary>One-half point of the initial vertical speed-table index.</summary>
    public ushort InitialYSpeedTableIndexHalf { get; internal set; }
}
