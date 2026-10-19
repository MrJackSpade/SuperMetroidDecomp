namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Addresses and loop bounds reached by the malformed SpaceTime Beam callback (#629).</summary>
[CpuBusBoundary("catalog: SpaceTime Beam corrupted palette copy")]
public static class SpacetimeBeamCorruptionLayout
{
    /// <summary>
    /// The direct-page long pointer <c>$00-$02</c> that <c>LDA [$00],Y</c> consumes: direct-page scratch the port does
    /// not model. It has always read zero in the port, as the retail probe observed (<c>$00:0000</c>).
    /// </summary>
    public const int SourceLongPointerValue = 0x000000;

    /// <summary>
    /// $7E:C1C0, the sprite-palette-six destination embedded in the interrupted
    /// <c>STA.l $7EC1C0,X</c> at $90:AD14. The inherited X runs the copy past this palette.
    /// </summary>
    public const int SpritePaletteSixAddress = 0x7ec1c0;

    /// <summary>$7E:D820, the event bitset that starts the progression slice of the live SRAM mirror.</summary>
    public const int ProgressionMirrorAddress = 0x7ed820;

    /// <summary>$90:AD1C compares inherited Y against $0020 after incrementing it.</summary>
    public const ushort PaletteByteCount = 0x0020;

    /// <summary>Bytes in one bank-$93 projectile animation record.</summary>
    public const ushort AnimationRecordByteCount = 8;
}
