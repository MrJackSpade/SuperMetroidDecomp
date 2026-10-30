using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The behaviour parameter of one room block. It has no single global meaning: the block's collision kind selects the
/// reaction that reads it. Callers therefore use the context-specific views below instead of treating unrelated meanings
/// that happen to share a value as one domain. A block whose collision kind changes keeps its parameter, and the new kind
/// reads it through its own view.
/// </summary>
/// <param name="Value">The parameter, 0 through 255.</param>
public readonly record struct RoomBlockBehavior(int Value)
{
    /// <summary>The parameter, 0 through 255; its kind-specific views below give it meaning.</summary>
    [PackedFields("block behaviour parameter whose meaning the block's collision kind decodes")]
    public int Value { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Value);

    /// <summary>
    /// The behaviour a downward gate's trigger block takes from its own picture: the low part of the trigger's block
    /// number, so the trigger's picture also selects the gate's colour and side.
    /// </summary>
    /// <param name="trigger">What the trigger block shows.</param>
    public static RoomBlockBehavior OfGateTrigger(RoomBlockAppearance trigger) => RoomBlockBehaviorCodec.GateTrigger(trigger);

    /// <summary>
    /// The behaviour of a horizontal or vertical extension that collides as the block <paramref name="offset"/> blocks
    /// (or rows) away; <see cref="ExtensionOffset"/> reads it back.
    /// </summary>
    /// <param name="offset">The signed displacement, -128 through 127.</param>
    public static RoomBlockBehavior OfExtensionOffset(int offset) => RoomBlockBehaviorCodec.ExtensionOffset(offset);

    /// <summary>The low five bits: a slope's shape.</summary>
    private const int SlopeShapeBits = (1 << 5) - 1;
    /// <summary>Bit six: a slope's horizontal reflection.</summary>
    private const int HorizontalFlipBit = 1 << 6;
    /// <summary>Bit seven: a slope's vertical reflection, or the area/grapple table selector.</summary>
    private const int HighBit = 1 << 7;
    /// <summary>The low seven bits: an area or grapple reaction index.</summary>
    private const int LowSevenBits = HighBit - 1;

    /// <summary>Signed block displacement used by horizontal and vertical extensions, -128 through 127.</summary>
    public int ExtensionOffset => unchecked((sbyte)Value);

    /// <summary>The slope's shape: the low five bits, an index into the slope-shape tables.</summary>
    public int SlopeShape => Value & SlopeShapeBits;

    /// <summary>Whether the slope selects the non-square height-table dispatcher.</summary>
    public bool IsNonSquareSlope => SlopeShape >= 5;

    /// <summary>Whether a slope block's shape is reflected horizontally.</summary>
    public bool SlopeFlipsHorizontally => (Value & HorizontalFlipBit) != 0;

    /// <summary>Whether a slope block's shape is reflected vertically.</summary>
    public bool SlopeFlipsVertically => (Value & HighBit) != 0;

    /// <summary>Two-bit slope orientation/quadrant selector stored in bits six and seven.</summary>
    public int SlopeOrientation => Value >> 6;

    /// <summary>
    /// Whether a projectile/bomb reaction selects the area-dependent table. This is the
    /// same physical bit as the slope vertical reflection bit, but only in this context.
    /// </summary>
    public bool UsesAreaReactionTable => (Value & HighBit) != 0;

    /// <summary>
    /// The entry of the room's door list a door block selects: the low seven bits. Bit seven is not part of the index;
    /// elevator and special-door behaviours share the same seven-bit lookup.
    /// </summary>
    public int DoorListIndex => Value & LowSevenBits;

    /// <summary>Low-seven-bit index used by area-dependent reaction tables.</summary>
    public int AreaReactionIndex => Value & LowSevenBits;

    /// <summary>Low-seven-bit index into the enemy spike-block reaction table; bit seven is not part of the index.</summary>
    public int EnemySpikeReactionIndex => Value & LowSevenBits;

    /// <summary>Whether this is a valid index into a normal table of the given size.</summary>
    public bool IsNormalReactionIndex(int entryCount) =>
        !UsesAreaReactionTable && Value < entryCount;

    /// <summary>Whether this is a valid index into an area table of the given size.</summary>
    public bool IsAreaReactionIndex(int entryCount) =>
        UsesAreaReactionTable && AreaReactionIndex < entryCount;

    /// <summary>Whether grapple collision rejects the indexed reaction via bit seven.</summary>
    public bool RejectsGrappleReaction => (Value & HighBit) != 0;

    /// <summary>Low-seven-bit grapple reaction-table index.</summary>
    [PackedFields("low seven bits of the behaviour, an index into the grapple reaction table")]
    public int GrappleReactionIndex => Value & LowSevenBits;

    /// <summary>Persistent grapple anchors are table entries zero and three.</summary>
    public bool IsPersistentGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 0 or 3;

    /// <summary>Breakable grapple anchors are table entries one and two.</summary>
    public bool IsBreakableGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 1 or 2;

    /// <summary>Validated direct index for a non-area projectile/bomb reaction table.</summary>
    [PackedFields("the behaviour as an index into the block reaction table")]
    public int NormalReactionIndex => Value;

    /// <summary>Entries zero through three restore after their break animation.</summary>
    public bool IsRespawningReaction => IsNormalReactionIndex(4);

    /// <summary>Entries four through seven remain cleared after their break animation.</summary>
    public bool IsPermanentReaction =>
        IsNormalReactionIndex(8) && !IsRespawningReaction;

    /// <summary>Shot-block entries eight and nine require the Power Bomb family.</summary>
    public bool RequiresPowerBombReaction =>
        !UsesAreaReactionTable && Value is 8 or 9;

    /// <summary>Shot-block entries ten and eleven require the Super Missile family.</summary>
    public bool RequiresSuperMissileReaction =>
        !UsesAreaReactionTable && Value is 10 or 11;

    /// <summary>
    /// Shootable-table entry 16: the collision probe that publishes its collision result before spawning a one-frame
    /// object that does nothing.
    /// </summary>
    public bool IsShootableCollisionProbe => Value == RoomBlockBehaviorValues.ShootableCollisionProbe.Value;

    /// <summary>Shootable-table entries 17 through 63 and 78 spawn the object that does nothing.</summary>
    public bool IsUnusedShootableReaction => Value is >= 17 and <= 63 or 78;

    /// <summary>Decodes the four contiguous blue-door cap values.</summary>
    public bool TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation) =>
        RoomBlockBehaviorCodec.TryBlueDoorOrientation(this, out orientation);

    /// <summary>Decodes a map, recharge, or save-station access behaviour.</summary>
    public bool TryGetStationAccess(out StationAccessBehavior access) =>
        RoomBlockBehaviorCodec.TryStationAccess(this, out access);

    /// <summary>
    /// Decodes the eight shootable-block values reserved for downward gate triggers.
    /// These values overlap unrelated meanings in other collision families, so the caller
    /// must first establish that the block is a shootable block.
    /// </summary>
    public bool TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger) =>
        RoomBlockBehaviorCodec.TryDownwardGateTrigger(this, out trigger);

    /// <summary>Formats the parameter as a two-digit hexadecimal value.</summary>
    public override string ToString() => $"${Value:X2}";
}
