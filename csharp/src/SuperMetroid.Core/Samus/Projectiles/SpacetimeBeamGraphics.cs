using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>How the SpaceTime beam loads its palette: every colour is its one defined colour.</summary>
internal static class SpacetimeBeamGraphics
{
    /// <summary>Loads the SpaceTime beam's defined palette.</summary>
    internal static void LoadPalette(ColourRun beam)
    {
        for (int color = 0; color < SuperMetroid.Core.Samus.Artwork.BeamPaletteDefinitions.ColorCount; color++)
            beam.Write(color, PaletteColour);
    }
}
