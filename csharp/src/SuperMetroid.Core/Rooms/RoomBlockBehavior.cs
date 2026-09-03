namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Lossless typed view of one room block's BTS byte. BTS has no single global enum:
/// the collision nibble selects the routine that gives this byte its meaning. Callers
/// must therefore use the context-specific views below instead of treating unrelated
/// meanings that happen to share a numeric value as one domain.
/// </summary>
public readonly record struct RoomBlockBehavior(byte Value)
{
    /// <summary>Signed block displacement used by horizontal and vertical extensions.</summary>
    public sbyte ExtensionOffset => unchecked((sbyte)Value);

    /// <summary>Low-five-bit index into the bank-$94 slope-shape tables.</summary>
    public byte SlopeShape => unchecked((byte)(Value & 0x1f));

    /// <summary>Whether the slope selects the non-square height-table dispatcher.</summary>
    public bool IsNonSquareSlope => SlopeShape >= 5;

    /// <summary>Horizontal reflection bit when this byte belongs to a slope block.</summary>
    public bool SlopeFlipsHorizontally => (Value & 0x40) != 0;

    /// <summary>Vertical reflection bit when this byte belongs to a slope block.</summary>
    public bool SlopeFlipsVertically => (Value & 0x80) != 0;

    /// <summary>Two-bit slope orientation/quadrant selector stored in bits six and seven.</summary>
    public byte SlopeOrientation => unchecked((byte)(Value >> 6));

    /// <summary>
    /// Whether a projectile/bomb reaction selects the area-dependent table. This is the
    /// same physical bit as the slope vertical reflection bit, but only in this context.
    /// </summary>
    public bool UsesAreaReactionTable => (Value & 0x80) != 0;

    /// <summary>Low-seven-bit index used by area-dependent reaction tables.</summary>
    public byte AreaReactionIndex => unchecked((byte)(Value & 0x7f));

    /// <summary>Whether this is a valid index into a normal table of the given size.</summary>
    public bool IsNormalReactionIndex(int entryCount) =>
        !UsesAreaReactionTable && Value < entryCount;

    /// <summary>Whether this is a valid index into an area table of the given size.</summary>
    public bool IsAreaReactionIndex(int entryCount) =>
        UsesAreaReactionTable && AreaReactionIndex < entryCount;

    /// <summary>Whether grapple collision rejects the indexed reaction via bit seven.</summary>
    public bool RejectsGrappleReaction => (Value & 0x80) != 0;

    /// <summary>Low-seven-bit grapple reaction-table index.</summary>
    public byte GrappleReactionIndex => unchecked((byte)(Value & 0x7f));

    /// <summary>Persistent grapple anchors are table entries zero and three.</summary>
    public bool IsPersistentGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 0 or 3;

    /// <summary>Breakable grapple anchors are table entries one and two.</summary>
    public bool IsBreakableGrappleReaction =>
        !RejectsGrappleReaction && GrappleReactionIndex is 1 or 2;

    /// <summary>Validated direct index for a non-area projectile/bomb reaction table.</summary>
    public byte NormalReactionIndex => Value;

    /// <summary>Low-two-bit size variant shared by the four breakable-block dimensions.</summary>
    public byte ReactionSizeIndex => unchecked((byte)(Value & 0x03));

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
        if (Value is >= (byte)DownwardGateTriggerBehavior.GreenLeft and
            <= (byte)DownwardGateTriggerBehavior.YellowRight)
        {
            trigger = (DownwardGateTriggerBehavior)Value;
            return true;
        }

        trigger = default;
        return false;
    }

    public override string ToString() => $"${Value:X2}";
}

/// <summary>Exclusive shootable-block BTS identities used by downward gate shot blocks.</summary>
public enum DownwardGateTriggerBehavior : byte
{
    GreenLeft = 0x46,
    GreenRight = 0x47,
    RedLeft = 0x48,
    RedRight = 0x49,
    BlueLeft = 0x4a,
    BlueRight = 0x4b,
    YellowLeft = 0x4c,
    YellowRight = 0x4d,
}

/// <summary>
/// Verified singleton BTS meanings used by several collision-family dispatchers. These
/// are typed values rather than a global enum because equal bytes can mean different
/// things after a different collision nibble selects another native routine.
/// </summary>
public static class RoomBlockBehaviorValues
{
    /// <summary>Zero terminates an extension chain and is the first entry in many tables.</summary>
    public static readonly RoomBlockBehavior None = new(0x00);

    /// <summary>Left-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingLeft = new(0x40);

    /// <summary>Right-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingRight = new(0x41);

    /// <summary>Up-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingUp = new(0x42);

    /// <summary>Down-facing blue-door shootable-cap dispatcher.</summary>
    public static readonly RoomBlockBehavior BlueDoorFacingDown = new(0x43);

    /// <summary>Resident colored-door or special PLM projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior ResidentPlmProjectileTrigger = new(0x44);

    /// <summary>Resident permanent-collectible touch/projectile-notification seam.</summary>
    public static readonly RoomBlockBehavior CollectibleTrigger = new(0x45);

    /// <summary>Resident scroll-trigger PLM collision seam.</summary>
    public static readonly RoomBlockBehavior ScrollTrigger = new(0x46);
}

/// <summary>
/// Exclusive meanings of BTS $47-$4D after a special-solid block selects the station
/// access dispatcher. Left/right describe the side of the station containing the block.
/// </summary>
public enum StationAccessBehavior : byte
{
    MapRight = 0x47,
    MapLeft = 0x48,
    EnergyRight = 0x49,
    EnergyLeft = 0x4a,
    MissileRight = 0x4b,
    MissileLeft = 0x4c,
    SaveFloor = 0x4d,
}
