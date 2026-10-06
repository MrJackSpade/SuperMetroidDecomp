using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Adjacent-table graphics selected when the player unpauses with SpaceTime equipped.</summary>
internal static class SpacetimeBeamGraphicsDefinitions
{
    /// <summary>$000E: Plasma/Spazer/Ice selects the bounded SpaceTime beam.</summary>
    internal const int Selection = 0x0e;
    /// <summary>$9A:C401: $90:C3CD's palette pointer used as a tile pointer by $90:AC8D.</summary>
    internal const int TileSource = 0x9ac401;
    /// <summary>$90:19FF: $90:C3E5's palette color used as a pointer by $90:ACCD, mirroring live WRAM.</summary>
    internal const ushort PalettePointer = 0x19ff;

    internal static void LoadPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        for (ushort color = 0; color < Assets.BeamPaletteDefinitions.ColorCount; color++)
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + color,
                SnesIndirectLongDataRead.ReadWord(bus, 0x90, PalettePointer, (ushort)(color * 2)));
    }
}
