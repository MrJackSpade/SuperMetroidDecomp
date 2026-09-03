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

    public static implicit operator RoomLevelWord(ushort raw) => new(raw);

    public static explicit operator ushort(RoomLevelWord word) => word.Raw;
}
