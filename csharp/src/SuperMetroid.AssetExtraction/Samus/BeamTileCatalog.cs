using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed beam characters by combination, with the beam palettes; the projectile sheets are composed from them.</summary>
public sealed class BeamTileCatalog
{
    /// <summary>Every combination's installed 256-byte beam atlas, by artwork ordinal.</summary>
    private readonly BeamTileAtlas[] sheets;
    /// <summary>Gets the optional installed beam and grapple palette catalog paired with these tile sheets.</summary>
    public InstalledBeamPalettes? Palettes { get; }
    /// <summary>Gets the optional installed fixed-color cycle used by the Hyper Beam visual effect.</summary>
    public HyperBeamFxColours? HyperBeamFxColors { get; }
    /// <summary>Keeps every combination's installed atlas, each read as installed.</summary>
    private BeamTileCatalog(BeamTileAtlas[] sheets, InstalledBeamPalettes? palettes,
        HyperBeamFxColours? hyperBeamFxColors)
    {
        this.sheets = sheets;
        Palettes = palettes;
        HyperBeamFxColors = hyperBeamFxColors;
    }

    /// <summary>Builds a catalog from file-context-validated atlases without decoding PNGs twice.</summary>
    internal static BeamTileCatalog FromAtlases(BeamTileAtlas[] sheets, InstalledBeamPalettes palettes,
        HyperBeamFxColours hyperBeamFxColors)
    {
        Ensure.NotNull(sheets);
        if (sheets.Length != BeamTileAtlasDefinitions.ArtworkCount || sheets.Any(sheet => sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new((BeamTileAtlas[])sheets.Clone(), palettes, hyperBeamFxColors);
    }

    /// <summary>The immutable 256-byte planar transfer of a beam combination's characters.</summary>
    public ReadOnlyMemory<byte> Resolve(SamusBeamCombination beam) =>
        sheets[BeamTileAtlasDefinitions.ArtworkOrdinal(beam)].Characters;

    /// <summary>Maps a supported native equipped-beam selection to its queued VRAM asset identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no artwork ($0C or $0F).</exception>
    public static InstalledArtworkId AssetFor(SamusBeamCombination selection) => selection switch
    {
        SamusBeamCombination.Power => InstalledArtworkId.BeamPowerTiles,
        SamusBeamCombination.Wave => InstalledArtworkId.BeamWaveTiles,
        SamusBeamCombination.Ice => InstalledArtworkId.BeamIceTiles,
        SamusBeamCombination.IceWave => InstalledArtworkId.BeamIceWaveTiles,
        SamusBeamCombination.Spazer => InstalledArtworkId.BeamSpazerTiles,
        SamusBeamCombination.SpazerWave => InstalledArtworkId.BeamSpazerWaveTiles,
        SamusBeamCombination.SpazerIce => InstalledArtworkId.BeamSpazerIceTiles,
        SamusBeamCombination.SpazerIceWave => InstalledArtworkId.BeamSpazerIceWaveTiles,
        SamusBeamCombination.Plasma => InstalledArtworkId.BeamPlasmaTiles,
        SamusBeamCombination.PlasmaWave => InstalledArtworkId.BeamPlasmaWaveTiles,
        SamusBeamCombination.PlasmaIce => InstalledArtworkId.BeamPlasmaIceTiles,
        SamusBeamCombination.PlasmaIceWave => InstalledArtworkId.BeamPlasmaIceWaveTiles,
        SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection => InstalledArtworkId.BeamChainsawTiles,
        SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection => InstalledArtworkId.BeamSpacetimeTiles,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork."),
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
    };

}
