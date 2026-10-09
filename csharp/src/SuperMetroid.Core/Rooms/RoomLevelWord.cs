namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete high-nibble dispatch values in one native room level-data word. The names
/// follow the sixteen-entry bank-$94 collision jump tables rather than describing only
/// the subset currently translated by a particular caller.
/// </summary>
public enum RoomCollisionType : byte
{
    /// <summary>Empty air handled by dispatcher entry zero.</summary>
    Air = 0x0,
    /// <summary>Slope collision whose geometry is selected by BTS.</summary>
    Slope = 0x1,
    /// <summary>Non-solid spike-air behavior.</summary>
    SpikeAir = 0x2,
    /// <summary>Special non-solid air behavior selected by BTS.</summary>
    SpecialAir = 0x3,
    /// <summary>Non-solid shootable-air behavior.</summary>
    ShootableAir = 0x4,
    /// <summary>Horizontal extension whose BTS displacement resolves another block.</summary>
    HorizontalExtension = 0x5,
    /// <summary>Unused non-solid dispatcher entry.</summary>
    UnusedAir = 0x6,
    /// <summary>Non-solid bombable-air behavior.</summary>
    BombableAir = 0x7,
    /// <summary>Ordinary fully solid block.</summary>
    SolidBlock = 0x8,
    /// <summary>Door collision block associated with a door PLM.</summary>
    DoorBlock = 0x9,
    /// <summary>Solid spike-block behavior.</summary>
    SpikeBlock = 0xa,
    /// <summary>Special solid-block behavior selected by BTS.</summary>
    SpecialBlock = 0xb,
    /// <summary>Solid shootable-block behavior.</summary>
    ShootableBlock = 0xc,
    /// <summary>Vertical extension whose BTS displacement resolves another block.</summary>
    VerticalExtension = 0xd,
    /// <summary>Solid block that accepts the grapple beam.</summary>
    GrappleBlock = 0xe,
    /// <summary>Solid bombable-block behavior.</summary>
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
/// <param name="Raw">The original packed level-data word, retaining presentation bits, collision type, and any unnamed collision value.</param>
public readonly record struct RoomLevelWord(ushort Raw)
{
    /// <summary>Mask selecting the ten-bit visual block-definition index.</summary>
    private const ushort VisualBlockIndexMask = 0x03ff;

    /// <summary>Mask selecting the horizontal and vertical parent-block flip bits.</summary>
    private const ushort VisualFlipMask = 0x0c00;

    /// <summary>Combined mask for the block index and flip fields that form the visual word.</summary>
    private const ushort VisualBitsMask = VisualBlockIndexMask | VisualFlipMask;

    /// <summary>Mask selecting the high-nibble collision dispatcher value.</summary>
    private const ushort CollisionTypeMask = 0xf000;

    /// <summary>Bit tested by native attachment probes as a coarse solid-block indicator.</summary>
    private const ushort SolidProbeMask = 0x8000;

    /// <summary>Number of bits the high-nibble collision value is shifted to reach its low-byte representation.</summary>
    private const int CollisionTypeShift = 12;

    /// <summary>Low ten bits selecting a visual 16×16 block definition.</summary>
    public ushort VisualBlockIndex => (ushort)(Raw & VisualBlockIndexMask);

    /// <summary>Independent parent-block horizontal and vertical flip bits.</summary>
    public LevelBlockFlipFlags VisualFlipFlags =>
        (LevelBlockFlipFlags)(Raw & VisualFlipMask);

    /// <summary>Combined presentation-only block index and parent flips, without collision bits.</summary>
    public ushort VisualWord => (ushort)(Raw & VisualBitsMask);

    /// <summary>Whether a standalone visual reference contains only the twelve presentation bits.</summary>
    public static bool IsValidVisualWord(ushort visualWord) =>
        (visualWord & ~VisualBitsMask) == 0;

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

    /// <summary>Replaces both presentation fields while preserving the physical collision nibble.</summary>
    public RoomLevelWord WithVisualWord(ushort visualWord)
    {
        if (!IsValidVisualWord(visualWord))
            throw new ArgumentOutOfRangeException(nameof(visualWord));
        return new RoomLevelWord(unchecked((ushort)(
            (Raw & CollisionTypeMask) | visualWord)));
    }

    /// <summary>Replaces only the four-bit collision dispatcher value.</summary>
    public RoomLevelWord WithCollisionType(RoomCollisionType collisionType)
    {
        ValidateCollisionType(collisionType);
        return new RoomLevelWord(unchecked((ushort)(
            (Raw & ~CollisionTypeMask) | ((ushort)collisionType << CollisionTypeShift))));
    }

    /// <summary>Ensures a visual block reference fits in the word's ten-bit index field.</summary>
    /// <param name="visualBlockIndex">Block-definition index supplied for the visual portion of a level word.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index sets bits outside the ten-bit field.</exception>
    private static void ValidateVisualBlockIndex(ushort visualBlockIndex)
    {
        if ((visualBlockIndex & ~VisualBlockIndexMask) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualBlockIndex));
    }

    /// <summary>Restricts the visual flip field to its independently supported horizontal and vertical bits.</summary>
    /// <param name="visualFlipFlags">Parent-block flip flags to encode.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value contains flags other than horizontal or vertical flip.</exception>
    private static void ValidateVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
    {
        const LevelBlockFlipFlags All =
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical;
        if ((visualFlipFlags & ~All) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualFlipFlags));
    }

    /// <summary>Ensures a collision dispatch value fits the four-bit high-nibble field.</summary>
    /// <param name="collisionType">Collision handler value to encode.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value exceeds the native sixteen-entry dispatch range.</exception>
    private static void ValidateCollisionType(RoomCollisionType collisionType)
    {
        if ((byte)collisionType > 0x0f)
            throw new ArgumentOutOfRangeException(nameof(collisionType));
    }

    /// <summary>Wraps a raw native level-data word without normalization.</summary>
    public static implicit operator RoomLevelWord(ushort raw) => new(raw);

    /// <summary>Extracts the unchanged raw native level-data word.</summary>
    public static explicit operator ushort(RoomLevelWord word) => word.Raw;
}
