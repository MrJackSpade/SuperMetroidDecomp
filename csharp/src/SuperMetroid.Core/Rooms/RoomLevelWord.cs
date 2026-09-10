namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete high-nibble dispatch values in one native room level-data word. The names
/// follow the sixteen-entry bank-$94 collision jump tables rather than describing only
/// the subset currently translated by a particular caller.
/// </summary>
public enum RoomCollisionType : byte
{
    Air = 0x0,
    Slope = 0x1,
    SpikeAir = 0x2,
    SpecialAir = 0x3,
    ShootableAir = 0x4,
    HorizontalExtension = 0x5,
    UnusedAir = 0x6,
    BombableAir = 0x7,
    SolidBlock = 0x8,
    DoorBlock = 0x9,
    SpikeBlock = 0xa,
    SpecialBlock = 0xb,
    ShootableBlock = 0xc,
    VerticalExtension = 0xd,
    GrappleBlock = 0xe,
    BombableBlock = 0xf,
}

/// <summary>
/// A lossless view over one 16-bit native <c>level_data</c> entry.
/// </summary>
/// <remarks>
/// This wrapper does not normalize or rebuild the word. The raw value remains available,
/// and every exposed field is a mask or shift documented by the bank-$80 renderer and
/// bank-$94 collision dispatcher. That makes the type useful at debugger boundaries while
/// preserving unknown collision-nibble values exactly.
/// </remarks>
public readonly record struct RoomLevelWord(ushort Raw)
{
    private const ushort VisualBlockIndexMask = 0x03ff;
    private const ushort VisualFlipMask = 0x0c00;
    private const ushort CollisionTypeMask = 0xf000;
    private const ushort SolidProbeMask = 0x8000;
    private const int CollisionTypeShift = 12;

    /// <summary>Low ten bits selecting a visual 16×16 block definition.</summary>
    public ushort VisualBlockIndex => (ushort)(Raw & VisualBlockIndexMask);

    /// <summary>Independent parent-block horizontal and vertical flip bits.</summary>
    public LevelBlockFlipFlags VisualFlipFlags =>
        (LevelBlockFlipFlags)(Raw & VisualFlipMask);

    /// <summary>
    /// Exact native dispatcher nibble. Use this when handling an unnamed value; unlike the
    /// enum view it makes no claim that every one of the sixteen handlers is translated.
    /// </summary>
    public byte CollisionTypeValue => (byte)(Raw >> CollisionTypeShift);

    /// <summary>
    /// Typed view of the same nibble. An enum cast preserves unnamed values, so inspecting
    /// this property can never discard or rewrite cartridge data.
    /// </summary>
    public RoomCollisionType CollisionType => (RoomCollisionType)CollisionTypeValue;

    /// <summary>
    /// Raw bit tested by $A0:BBBF/$A0:BC76 attachment probes. This is not resolved
    /// block solidity: it ignores slope geometry and BTS extension destinations.
    /// </summary>
    public bool HasSolidProbeBit => (Raw & SolidProbeMask) != 0;

    /// <summary>Builds a complete native level word from its three independent fields.</summary>
    public static RoomLevelWord Create(
        ushort visualBlockIndex,
        LevelBlockFlipFlags visualFlipFlags,
        RoomCollisionType collisionType)
    {
        ValidateVisualBlockIndex(visualBlockIndex);
        ValidateVisualFlipFlags(visualFlipFlags);
        ValidateCollisionType(collisionType);
        return new RoomLevelWord(unchecked((ushort)(
            visualBlockIndex |
            (ushort)visualFlipFlags |
            ((ushort)collisionType << CollisionTypeShift))));
    }

    /// <summary>Replaces only the visual block index and preserves both flips and collision.</summary>
    public RoomLevelWord WithVisualBlockIndex(ushort visualBlockIndex)
    {
        ValidateVisualBlockIndex(visualBlockIndex);
        return new RoomLevelWord(unchecked((ushort)((Raw & ~VisualBlockIndexMask) | visualBlockIndex)));
    }

    /// <summary>Replaces only the parent-block visual transforms.</summary>
    public RoomLevelWord WithVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
    {
        ValidateVisualFlipFlags(visualFlipFlags);
        return new RoomLevelWord(unchecked((ushort)(
            (Raw & ~VisualFlipMask) | (ushort)visualFlipFlags)));
    }

    /// <summary>Replaces only the four-bit collision dispatcher value.</summary>
    public RoomLevelWord WithCollisionType(RoomCollisionType collisionType)
    {
        ValidateCollisionType(collisionType);
        return new RoomLevelWord(unchecked((ushort)(
            (Raw & ~CollisionTypeMask) | ((ushort)collisionType << CollisionTypeShift))));
    }

    private static void ValidateVisualBlockIndex(ushort visualBlockIndex)
    {
        if ((visualBlockIndex & ~VisualBlockIndexMask) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualBlockIndex));
    }

    private static void ValidateVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
    {
        const LevelBlockFlipFlags All =
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical;
        if ((visualFlipFlags & ~All) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualFlipFlags));
    }

    private static void ValidateCollisionType(RoomCollisionType collisionType)
    {
        if ((byte)collisionType > 0x0f)
            throw new ArgumentOutOfRangeException(nameof(collisionType));
    }

    public static implicit operator RoomLevelWord(ushort raw) => new(raw);

    public static explicit operator ushort(RoomLevelWord word) => word.Raw;
}
