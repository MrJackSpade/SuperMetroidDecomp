using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Cartridge values and helpers of <see cref="BeamTileAtlasDefinitions"/> that only verification reads.</summary>
internal static class BeamTileAtlasDefinitionsConstants
{
    /// <summary>$90:C3B1, BeamTilesPointers; resolves a legacy source to its base selection.
    /// Native shared sheets do not encode the independently editable combination identity.</summary>
    public static SamusBeamCombination LegacySelectionFor(BeamTileSource source)
    {
        return source switch
        {
            BeamTileSource.Chainsaw => SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection,
            BeamTileSource.Spacetime => SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection,
            BeamTileSource.Power => SamusBeamCombination.Power,
            BeamTileSource.Wave => SamusBeamCombination.Wave,
            BeamTileSource.Ice => SamusBeamCombination.Ice,
            BeamTileSource.Spazer => SamusBeamCombination.Spazer,
            BeamTileSource.Plasma => SamusBeamCombination.Plasma,
            _ => throw new InvalidOperationException($"Undefined BeamTileSource {source}."),
        };
    }
}
