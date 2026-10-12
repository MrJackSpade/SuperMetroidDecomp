using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>CGRAM ranges Samus's palette routines in banks $90/$91/$9B write directly (#630).</summary>
[PpuLayoutBoundary("catalog: Samus CGRAM ranges")]
public static class SamusPaletteLayout
{
    /// <summary>CGADD $C0-$CF (OBJ palette 4, <c>Palettes_SpriteP4</c>): Samus's suit palette.</summary>
    public static readonly PpuLayoutCell Suit = PpuMemoryLayout.Colors(nameof(Suit),
        SamusPaletteRomData.Common.SamusObjPaletteStart, SamusPaletteRomData.Common.ColorsPerObjPalette, "Bgr555 OBJ palette");

    /// <summary>CGADD $C4 (OBJ palette 4, colour 4): Samus's visor colour, cycled by the visor and X-ray routines.</summary>
    public static readonly PpuLayoutCell Visor = PpuMemoryLayout.Colors(nameof(Visor),
        SamusXrayRomData.Palette.VisorCgramIndex, 1, "Bgr555 OBJ palette colour");

    /// <summary>CGADD $F0-$FF (OBJ palette 7): the suitless Samus palette loaded during the death sequence.</summary>
    public static readonly PpuLayoutCell Suitless = PpuMemoryLayout.Colors(nameof(Suitless),
        SamusPaletteRomData.Common.SuitlessObjPaletteStart, SamusPaletteRomData.Common.ColorsPerObjPalette, "Bgr555 OBJ palette");

    /// <summary>CGADD $E0-$EF (OBJ palette 6): the selected beam palette loaded by <c>$90:AC8D</c>.</summary>
    public static readonly PpuLayoutCell Beam = PpuMemoryLayout.Colors(nameof(Beam),
        SamusProjectileRomData.Palettes.BeamDestinationIndex, Assets.BeamPaletteDefinitions.ColorCount, "Bgr555 OBJ palette");

    /// <summary>CGADD $DF (OBJ palette 5, colour 15): the grapple flare colour <c>$9B:C686</c> writes beside the beam palette.</summary>
    public static readonly PpuLayoutCell GrappleFlare = PpuMemoryLayout.Colors(nameof(GrappleFlare),
        SamusGrappleRomData.Palettes.FlareColorIndex, 1, "Bgr555 OBJ palette colour");

    /// <summary>CGADD $E0-$E9: the Crystal Flash body colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBody = PpuMemoryLayout.Colors(nameof(CrystalFlashBody),
        SamusPaletteRomData.CrystalFlash.BodyCgramStart, Assets.CrystalFlashColorFormat.BodyColorCount, "Bgr555 Crystal Flash body");

    /// <summary>CGADD $EA-$EF: the Crystal Flash bubble colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBubble = PpuMemoryLayout.Colors(nameof(CrystalFlashBubble),
        SamusPaletteRomData.CrystalFlash.BubbleCgramStart, Assets.CrystalFlashColorFormat.BubbleColorCount, "Bgr555 Crystal Flash bubble");

    /// <summary>CGADD $00-$BF: every colour below Samus's palette, whited out by the death sequence (<c>$9B:B4E0</c>).</summary>
    public static readonly PpuLayoutCell DeathWhiteoutBelowSuit = PpuMemoryLayout.Colors(nameof(DeathWhiteoutBelowSuit),
        0x00, SamusPaletteRomData.Common.SamusObjPaletteStart, "Bgr555 colours");

    /// <summary>CGADD $D0-$EF: the OBJ palettes between Samus's suit and suitless palettes, whited out by the death sequence.</summary>
    public static readonly PpuLayoutCell DeathWhiteoutBetweenSuits = PpuMemoryLayout.Colors(nameof(DeathWhiteoutBetweenSuits),
        SamusPaletteRomData.Common.SamusObjPaletteStart + SamusPaletteRomData.Common.ColorsPerObjPalette,
        SamusPaletteRomData.Common.SuitlessObjPaletteStart -
            (SamusPaletteRomData.Common.SamusObjPaletteStart + SamusPaletteRomData.Common.ColorsPerObjPalette),
        "Bgr555 OBJ palettes");
}

/// <summary>Typed view of Samus's CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus CGRAM ranges")]
public readonly struct SamusPalettes(SnesCgram cgram)
{
    /// <summary>Samus's suit palette.</summary>
    public CgramPalette Suit => new(cgram, SamusPaletteLayout.Suit);

    /// <summary>Samus's visor colour.</summary>
    public CgramPalette Visor => new(cgram, SamusPaletteLayout.Visor);

    /// <summary>The suitless palette.</summary>
    public CgramPalette Suitless => new(cgram, SamusPaletteLayout.Suitless);

    /// <summary>The beam palette.</summary>
    public CgramPalette Beam => new(cgram, SamusPaletteLayout.Beam);

    /// <summary>The grapple flare colour.</summary>
    public CgramPalette GrappleFlare => new(cgram, SamusPaletteLayout.GrappleFlare);

    /// <summary>The Crystal Flash body colours.</summary>
    public CgramPalette CrystalFlashBody => new(cgram, SamusPaletteLayout.CrystalFlashBody);

    /// <summary>The Crystal Flash bubble colours.</summary>
    public CgramPalette CrystalFlashBubble => new(cgram, SamusPaletteLayout.CrystalFlashBubble);

    /// <summary>Colours below the suit palette whited out at death.</summary>
    public CgramPalette DeathWhiteoutBelowSuit => new(cgram, SamusPaletteLayout.DeathWhiteoutBelowSuit);

    /// <summary>Colours between the suit and suitless palettes whited out at death.</summary>
    public CgramPalette DeathWhiteoutBetweenSuits => new(cgram, SamusPaletteLayout.DeathWhiteoutBetweenSuits);
}
