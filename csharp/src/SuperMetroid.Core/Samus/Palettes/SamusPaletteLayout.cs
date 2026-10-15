using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Palettes;

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
        SamusProjectileRomData.Palettes.BeamDestinationIndex, SuperMetroid.Core.Samus.Artwork.BeamPaletteDefinitions.ColorCount, "Bgr555 OBJ palette");

    /// <summary>CGADD $DF (OBJ palette 5, colour 15): the grapple flare colour <c>$9B:C686</c> writes beside the beam palette.</summary>
    public static readonly PpuLayoutCell GrappleFlare = PpuMemoryLayout.Colors(nameof(GrappleFlare),
        SamusGrappleRomData.Palettes.FlareColorIndex, 1, "Bgr555 OBJ palette colour");

    /// <summary>CGADD $E0-$E9: the Crystal Flash body colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBody = PpuMemoryLayout.Colors(nameof(CrystalFlashBody),
        SamusPaletteRomData.CrystalFlash.BodyCgramStart, SuperMetroid.Core.Samus.Artwork.CrystalFlashColorFormat.BodyColorCount, "Bgr555 Crystal Flash body");

    /// <summary>CGADD $EA-$EF: the Crystal Flash bubble colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBubble = PpuMemoryLayout.Colors(nameof(CrystalFlashBubble),
        SamusPaletteRomData.CrystalFlash.BubbleCgramStart, SuperMetroid.Core.Samus.Artwork.CrystalFlashColorFormat.BubbleColorCount, "Bgr555 Crystal Flash bubble");

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
