using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Lossless typed view of one room block's BTS byte. BTS has no single global enum:
/// the collision nibble selects the routine that gives this byte its meaning. Callers
/// must therefore use the context-specific views below instead of treating unrelated
/// meanings that happen to share a numeric value as one domain.
/// </summary>
/// <param name="Value">The unchanged block-type-specific BTS byte from the room's behavior data.</param>
public readonly record struct RoomBlockBehavior(int Value)
{
    /// <summary>The unchanged block-type-specific BTS byte, 0 through 255.</summary>
    [NativeWord("block BTS byte whose meaning the block type decodes")]
    public int Value { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Value);

    /// <summary>The low five bits: a slope's shape.</summary>
    private const int SlopeShapeBits = (1 << 5) - 1;
    /// <summary>Bit six: a slope's horizontal reflection.</summary>
    private const int HorizontalFlipBit = 1 << 6;
    /// <summary>Bit seven: a slope's vertical reflection, or the area/grapple table selector.</summary>
    private const int HighBit = 1 << 7;
    /// <summary>The low seven bits: an area or grapple reaction index.</summary>
    private const int LowSevenBits = HighBit - 1;

    /// <summary>Signed block displacement used by horizontal and vertical extensions.</summary>
    public sbyte ExtensionOffset => unchecked((sbyte)Value);

    /// <summary>Low-five-bit index into the bank-$94 slope-shape tables.</summary>
    public int SlopeShape => Value & SlopeShapeBits;

    /// <summary>Whether the slope selects the non-square height-table dispatcher.</summary>
    public bool IsNonSquareSlope => SlopeShape >= 5;

    /// <summary>Horizontal reflection bit when this byte belongs to a slope block.</summary>
    public bool SlopeFlipsHorizontally => (Value & HorizontalFlipBit) != 0;

    /// <summary>Vertical reflection bit when this byte belongs to a slope block.</summary>
    public bool SlopeFlipsVertically => (Value & HighBit) != 0;

    /// <summary>Two-bit slope orientation/quadrant selector stored in bits six and seven.</summary>
    public int SlopeOrientation => Value >> 6;

    /// <summary>
    /// Whether a projectile/bomb reaction selects the area-dependent table. This is the
    /// same physical bit as the slope vertical reflection bit, but only in this context.
    /// </summary>
    public bool UsesAreaReactionTable => (Value & HighBit) != 0;

    /// <summary>Low-seven-bit index used by area-dependent reaction tables.</summary>
    public int AreaReactionIndex => Value & LowSevenBits;

    /// <summary>Whether this is a valid index into a normal table of the given size.</summary>
    public bool IsNormalReactionIndex(int entryCount) =>
        !UsesAreaReactionTable && Value < entryCount;

    /// <summary>Whether this is a valid index into an area table of the given size.</summary>
    public bool IsAreaReactionIndex(int entryCount) =>
        UsesAreaReactionTable && AreaReactionIndex < entryCount;

    /// <summary>Whether grapple collision rejects the indexed reaction via bit seven.</summary>
    public bool RejectsGrappleReaction => (Value & HighBit) != 0;

    /// <summary>Low-seven-bit grapple reaction-table index.</summary>
    [NativeWord("low seven bits of the BTS byte, an index into the grapple reaction table")]
    public int GrappleReactionIndex => Value & LowSevenBits;

    /// <summary>Persistent grapple anchors are table entries zero and three.</summary>
    public bool IsPersistentGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 0 or 3;

    /// <summary>Breakable grapple anchors are table entries one and two.</summary>
    public bool IsBreakableGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 1 or 2;

    /// <summary>Validated direct index for a non-area projectile/bomb reaction table.</summary>
    [NativeWord("the BTS byte as an index into the block reaction table")]
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
    /// Shootable-table entry $10 selects header <c>$84:B974</c>. Bank $94 also publishes
    /// its collision-result sentinel before spawning that one-frame no-op PLM.
    /// </summary>
    public bool IsShootableCollisionProbe => Value == RoomBlockBehaviorValues.ShootableCollisionProbe.Value;

    /// <summary>$94:9EA6 entries $11..$3F and $4E point to the native nothing PLM.</summary>
    public bool IsUnusedShootableReaction => Value is >= 17 and <= 63 or 78;

    /// <summary>Decodes the four contiguous blue-door cap values.</summary>
    public bool TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation)
    {
        int index = Value - RoomBlockBehaviorValues.BlueDoorFacingLeft.Value;
        if ((uint)index <= (uint)ColoredDoorOrientation.Down)
        {
            orientation = (ColoredDoorOrientation)index;
            return true;
        }

        orientation = default;
        return false;
    }

    /// <summary>Decodes a map, recharge, or save-station access byte.</summary>
    public bool TryGetStationAccess(out StationAccessBehavior access)
    {
        if (Value is >= (byte)StationAccessBehavior.MapRight and
            <= (byte)StationAccessBehavior.SaveFloor)
        {
            access = (StationAccessBehavior)Value;
            return true;
        }

        access = default;
        return false;
    }

    /// <summary>
    /// Decodes the eight shootable-block values reserved for downward gate triggers.
    /// These bytes overlap unrelated meanings in other collision families, so the caller
    /// must first establish that the level word is a shootable block.
    /// </summary>
    public bool TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger)
    {
        if (Value is >= (byte)DownwardGateTriggerBehavior.BlueLeft and
            <= (byte)DownwardGateTriggerBehavior.YellowRight)
        {
            trigger = (DownwardGateTriggerBehavior)Value;
            return true;
        }

        trigger = default;
        return false;
    }

    /// <summary>Formats the native BTS byte as a two-digit hexadecimal value.</summary>
    public override string ToString() => $"${Value:X2}";
}
