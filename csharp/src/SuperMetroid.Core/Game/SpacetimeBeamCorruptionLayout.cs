namespace SuperMetroid.Core.Game;

/// <summary>Addresses and loop bounds reached by the malformed SpaceTime Beam callback.</summary>
public static class SpacetimeBeamCorruptionLayout
{
    /// <summary>Mask for the 65C816's wrapping 24-bit CPU address bus.</summary>
    public const int CpuAddressMask = 0x00ff_ffff;

    /// <summary>
    /// $7E:C1C0, the sprite-palette-six destination embedded in the interrupted
    /// <c>STA.l $7EC1C0,X</c> at $90:AD14.
    /// </summary>
    public const int SpritePaletteSixWramAddress = 0x7ec1c0;

    /// <summary>
    /// Direct-page long pointer $00-$02 consumed by <c>LDA [$00],Y</c>. Gameplay uses
    /// direct page zero, so these CPU-bus addresses are the low-WRAM mirror.
    /// </summary>
    public const int SourceLongPointerAddress = 0x000000;

    /// <summary>$90:AD1C compares inherited Y against $0020 after incrementing it.</summary>
    public const ushort PaletteByteCount = 0x0020;

    /// <summary>Bytes in one bank-$93 projectile animation record.</summary>
    public const ushort AnimationRecordByteCount = 8;
}
