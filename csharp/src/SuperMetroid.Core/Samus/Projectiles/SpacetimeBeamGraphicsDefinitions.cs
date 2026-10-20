using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Adjacent-table graphics selected when the player unpauses with SpaceTime equipped.</summary>
internal static class SpacetimeBeamGraphicsDefinitions
{
    /// <summary>$000E: Plasma/Spazer/Ice selects the bounded SpaceTime beam.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaIce;
    /// <summary>$9A:C401: $90:C3CD's palette pointer used as a tile pointer by $90:AC8D.</summary>
    internal const int TileSource = 0x9ac401;
    /// <summary>
    /// Every colour of the SpaceTime beam palette. The cartridge's <c>$90:ACCD</c> reads its colours through
    /// <c>$90:C3E5</c>'s colour word used as a pointer, <c>$90:19FF</c>: live enemy-projectile timers and
    /// pre-instructions in WRAM. The port does not keep that native layout, so the palette is a defined one instead
    /// (owner decision on #1318).
    /// </summary>
    internal const ushort PaletteWord = 0x0000;

    /// <summary>Loads the SpaceTime beam's defined palette.</summary>
    internal static void LoadPalette(IPaletteColors cgram)
    {
        for (ushort color = 0; color < SuperMetroid.Core.Samus.Artwork.BeamPaletteDefinitions.ColorCount; color++)
            new SamusPalettes(cgram).Beam.Write(color, Bgr555.FromCgramPortWord(PaletteWord));
    }
}
