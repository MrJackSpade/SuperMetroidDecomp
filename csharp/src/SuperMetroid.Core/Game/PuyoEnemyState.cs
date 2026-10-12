using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored in Puyo's ordinary enemy variable C. Keeping
/// the ROM addresses as enum values makes a debugger display directly comparable with
/// <c>$0FAE,x</c> rather than hiding cartridge state behind invented host ordinals.
/// </summary>
public enum PuyoEnemyFunction : ushort
{
    /// <summary>$A2:9B65, <c>Function_Puyo_Grounded</c>: decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 0x9b65,
    /// <summary>$A2:9B81, <c>Function_Puyo_Airborne</c>: dispatches the current hop or collision-recovery routine through the airborne function word.</summary>
    Airborne = 0x9b81,
}

/// <summary>
/// Literal indirect targets stored in Puyo's ordinary enemy variable D. Entries zero
/// through six in the ROM hop table select these functions.
/// </summary>
public enum PuyoAirborneFunction : ushort
{
    /// <summary>$A2:9D0B, <c>Function_Puyo_Airborne_Normal_ShortHop</c>: moves the small normal hop and selects the slow grounded animation on landing or terrain collision.</summary>
    NormalShortHop = 0x9d0b,
    /// <summary>$A2:9D2B, <c>Function_Puyo_Airborne_Normal_BigHop</c>: moves the taller normal hop and selects the medium grounded animation on landing or terrain collision.</summary>
    NormalBigHop = 0x9d2b,
    /// <summary>$A2:9D4B, <c>Function_Puyo_Airborne_Normal_LongHop</c>: moves hop-table record two with increased horizontal speed and selects the fast grounded animation on landing or terrain collision.</summary>
    NormalLongHop = 0x9d4b,
    /// <summary>$A2:9D6B, <c>Function_Puyo_Airborne_GiantHop</c>: moves the 128-pixel-height hop; normal landing restores small-hop selection, while wall or ceiling collision retains dropping recovery.</summary>
    GiantHop = 0x9d6b,
    /// <summary>$A2:9D98, <c>Function_Puyo_Airborne_Dropping</c>: falls vertically at the hop record's packed 8.8 speed until terrain contact selects a random small or big post-drop bounce.</summary>
    Dropping = 0x9d98,
    /// <summary>$A2:9DCD, <c>Function_Puyo_Airborne_Dropped</c>: moves either post-drop bounce and selects a giant hop for the next grounded launch when the hopping animation is cleared by terrain contact.</summary>
    Dropped = 0x9dcd,
}

/// <summary>
/// The seven concrete indexes into <c>PuyoHopTable</c> at $A2:9A07. This is a discriminant,
/// not a flags word: every value selects one eight-byte height/speed/function record.
/// </summary>
public enum PuyoHopType : ushort
{
    /// <summary>Records zero at $A2:9A07: 16-pixel height, packed 8.8 X speed $0100, and the short-hop function; ordinary distant launches may randomize to record one.</summary>
    NormalSmall = 0,
    /// <summary>Records one at $A2:9A0F: 32-pixel height, packed 8.8 X speed $0100, and the big-hop function; proximity to Samus permits random selection among normal records zero through two.</summary>
    NormalBig = 1,
    /// <summary>Records three at $A2:9A1F: 128-pixel height and packed 8.8 X speed $0140, whose fractional byte is discarded by NTSC movement; selected after a post-drop bounce.</summary>
    Giant = 3,
    /// <summary>Records four at $A2:9A27: no horizontal motion and packed 8.8 downward speed $0100; wall or ceiling collision selects this fall before a recovery bounce.</summary>
    Dropping = 4,
    /// <summary>Records five at $A2:9A2F: 16-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; one of two randomized recovery heights.</summary>
    DroppedSmall = 5,
    /// <summary>Records six at $A2:9A37: 21-pixel post-drop bounce with packed 8.8 X speed $0100 and curve-index delta $01C0; the taller randomized recovery height.</summary>
    DroppedBig = 6,
}

/// <summary>The two literal values written to Puyo's direction word.</summary>
public enum PuyoDirection : ushort
{
    /// <summary>Native direction word zero: positive X motion and right-facing hop poses; also selected when Samus and Puyo have equal X coordinates.</summary>
    Right = 0,
    /// <summary>Native direction word one: negative X motion and left-facing hop poses; terrain collision can latch its opposite for the next launch.</summary>
    Left = 1,
}

/// <summary>
/// Typed view of Puyo's five ordinary enemy variables and eight parallel extended-WRAM
/// words. Ordinary members remain backed by <see cref="RoomEnemySlot"/> so inspecting the
/// actor shows the same layout as <c>$0FAA-$0FB2</c> in the cartridge.
/// </summary>
public sealed class PuyoEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new puyo enemy state instance.</summary>
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
    public ushort HopCooldownTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
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
