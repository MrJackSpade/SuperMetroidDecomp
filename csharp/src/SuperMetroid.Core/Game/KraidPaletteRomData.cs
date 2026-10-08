namespace SuperMetroid.Core.Game;

/// <summary>Five immutable RGB5 sources selected by Kraid's palette handlers.</summary>
public enum KraidPaletteSource
{
    /// <summary>$A7:86C7, Palette_KraidRoomBackground: sixteen RGB5 colors for background palette six, restored in the defeated room and used by backdrop fades.</summary>
    RoomBackdrop,
    /// <summary>$A7:AAA6, Kraid initialization's spritePalette3: sixteen rock colors staged in native target OBJ palette three, exposed at global CGRAM palette eleven by the runtime.</summary>
    InitialTarget,
    /// <summary>$A7:B3D3, Palette_Kraid_BG_HurtFlash and eight following health bands: 144 colors selected for background palette seven, also supplying eye-transition targets.</summary>
    Health,
    /// <summary>$A7:B513, Palette_Kraid_Sprite_HurtFlash and eight following health bands: 144 colors selected alongside the background health band for OBJ palette seven.</summary>
    Secondary,
    /// <summary>$A7:B4F3, Palette_Kraid_Death: sixteen colors replacing background palette seven when death retracts the arm before the final fade.</summary>
    DeathArm,
}

/// <summary>Native bank-$A7 Kraid palette addresses and exact source lengths.</summary>
public static class KraidPaletteRomData
{
    /// <summary>$A7:AAA6, initial target colors staged at CGRAM palette eleven.</summary>
    public const int InitialTargetPaletteWords = 0xa7aaa6;

    /// <summary>Nine sixteen-color health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colors in one native Kraid palette band.</summary>
    public const int BandColors = 16;

    /// <summary>
    /// Selects the native palette operand for a named source. Independently checked
    /// against A7:A981, AA97, B3BB, B3C2 and C37B; invalid source values are rejected.
    /// </summary>
    /// <param name="source">The named palette operand whose original cartridge identity is required.</param>
    /// <returns>The 24-bit SNES bus address of its first little-endian RGB5 color word; no cartridge read is performed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source is not one of the five supported palette identities.</exception>
    public static int SourceAddress(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop => EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords,
        KraidPaletteSource.InitialTarget => InitialTargetPaletteWords,
        KraidPaletteSource.Health => EnemyRomTablePointers.Kraid.HealthPaletteWords,
        KraidPaletteSource.Secondary => EnemyRomTablePointers.Kraid.SecondaryPaletteWords,
        KraidPaletteSource.DeathArm => EnemyRomTablePointers.Kraid.DeathArmPaletteWords,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };

    /// <summary>
    /// Three individual sixteen-color bands and two nine-band health sequences.
    /// Exact native source extents are independently checked; no adjacent palette is included.
    /// </summary>
    /// <param name="source">The palette source whose complete editable color extent is required.</param>
    /// <returns>Sixteen RGB5 color words for a single band or 144 for a flash-plus-eight-health sequence; this count is not in bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source is not one of the five supported palette identities.</exception>
    public static int ColorCount(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop or KraidPaletteSource.InitialTarget or
            KraidPaletteSource.DeathArm => BandColors,
        KraidPaletteSource.Health or KraidPaletteSource.Secondary =>
            HealthBandCount * BandColors,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };
}
