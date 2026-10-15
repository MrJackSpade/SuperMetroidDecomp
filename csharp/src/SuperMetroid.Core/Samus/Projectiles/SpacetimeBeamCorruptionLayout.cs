namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Addresses and loop bounds reached by the malformed SpaceTime Beam callback (#629).</summary>
[CpuBusBoundary("catalog: SpaceTime Beam corrupted palette copy")]
public static class SpacetimeBeamCorruptionLayout
{
    /// <summary>
    /// Direct-page long pointer $00-$02 consumed by <c>LDA [$00],Y</c>. Gameplay uses direct
    /// page zero, so these CPU-bus addresses are the low-WRAM mirror of <c>$7E:0000-$7E:0002</c>.
    /// </summary>
    public static readonly CpuBusCell SourceLongPointer = CpuBusCell.WramBytes(nameof(SourceLongPointer), 0x000000, 3,
        "24-bit long pointer (u8[3] little-endian)");

    /// <summary>
    /// $7E:C1C0, the sprite-palette-six destination embedded in the interrupted
    /// <c>STA.l $7EC1C0,X</c> at $90:AD14. The inherited X runs the copy past this palette.
    /// </summary>
    public static readonly CpuBusCell SpritePaletteSix = CpuBusCell.WramBytes(nameof(SpritePaletteSix), 0x7ec1c0,
        PaletteByteCount, "BGR555[16] (u16[] little-endian)");

    /// <summary>$90:AD1C compares inherited Y against $0020 after incrementing it.</summary>
    public const ushort PaletteByteCount = 0x0020;

    /// <summary>Bytes in one bank-$93 projectile animation record.</summary>
    public const ushort AnimationRecordByteCount = 8;
}
