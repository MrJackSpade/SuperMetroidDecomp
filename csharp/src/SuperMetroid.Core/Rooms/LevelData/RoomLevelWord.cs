namespace SuperMetroid.Core.Rooms.LevelData;

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
    /// <summary>Low ten bits selecting a room metatile definition.</summary>
    private const ushort VisualBlockIndexMask = 0x03ff;
    /// <summary>Two parent-metatile horizontal and vertical flip bits.</summary>
    internal const ushort VisualFlipMask = 0x0c00;
    /// <summary>All twelve presentation bits, excluding physical collision dispatch.</summary>
    private const ushort VisualBitsMask = VisualBlockIndexMask | VisualFlipMask;
    /// <summary>High nibble selecting the native collision handler.</summary>
    private const ushort CollisionTypeMask = 0xf000;
    /// <summary>High bit checked by the native solid-attachment probe.</summary>
    private const ushort SolidProbeMask = 0x8000;
    /// <summary>Distance from the low bit to the collision dispatch nibble.</summary>
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

    /// <summary>
    /// <c>$84:B3C1 Setup_DeactivatePLM</c>: clears collision bits 12..14 and keeps bit 15 with the visual payload, so a
    /// solid block stays solid and every other block becomes air.
    /// </summary>
    public RoomLevelWord WithPlmDeactivation() => new(unchecked((ushort)(Raw & (SolidProbeMask | VisualBitsMask))));

    /// <summary>Rejects a metatile index that would overwrite flip or collision bits.</summary>
    private static void ValidateVisualBlockIndex(ushort visualBlockIndex)
    {
        if ((visualBlockIndex & ~VisualBlockIndexMask) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualBlockIndex));
    }

    /// <summary>Rejects flags outside the two parent-metatile flip bits.</summary>
    internal static void ValidateVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
    {
        const LevelBlockFlipFlags All =
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical;
        if ((visualFlipFlags & ~All) != 0)
            throw new ArgumentOutOfRangeException(nameof(visualFlipFlags));
    }

    /// <summary>Rejects a collision dispatch value that cannot fit in the native high nibble.</summary>
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
