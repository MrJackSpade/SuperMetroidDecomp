namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for the two Brinstar blue-spore palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F775</c> and <c>$F779</c> contain the same presentation BGR555
/// sequence. The Spore Spawn variant additionally installs the area-mini-boss death
/// pre-instruction. This catalog owns setup, timing, waits, and loop control only.
/// Standard $8D:ED99 selects CGRAM byte $00E2 and enters records at
/// $ED9D + 10*i; Spore Spawn $8D:EE2D first installs pre-instruction
/// $EEC5, selects the same index, and enters records at $EE35 + 10*i.
/// Each valid i=0..13 lasts ten frames, writes three presentation colors, and
/// ends in $C595 wait. Their $C61E gotos at $EE29 and $EEC1 return to
/// the respective first records, giving 140-frame cycles; i=14 reaches
/// loop control. All 66 control words across both programs match the
/// pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class BrinstarBlueSporePaletteFxColorDefinitions
{
    /// <summary>The ordinary Brinstar entry without a boss-death pre-instruction.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition Standard =
        new(BrinstarBlueSporePaletteOwner.StandardRooms);
    /// <summary>The Spore Spawn entry that deletes itself after the area's mini-boss dies.</summary>
    private static readonly BrinstarBlueSporePaletteFxColorDefinition SporeSpawn =
        new(BrinstarBlueSporePaletteOwner.SporeSpawnRoom);

    /// <summary>Fourteen ten-frame records form one complete spore-color cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxColorDefinition> All { get; } = [Standard, SporeSpawn];
}
