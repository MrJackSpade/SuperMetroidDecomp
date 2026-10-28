using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed beam characters by combination, with the beam palettes; the projectile sheets are composed from them.</summary>
public sealed class BeamTileCatalog : IInstalledArtworkContent
{
    /// <summary>Selected 256-byte beam atlases, with native aliases sharing matching tile pixels.</summary>
    internal readonly BeamTileAtlas[] sheets;
    /// <summary>Gets the optional installed beam and grapple palette catalog paired with these tile sheets.</summary>
    public BeamPaletteCatalog? Palettes { get; }
    /// <summary>Gets the optional installed fixed-color cycle used by the Hyper Beam visual effect.</summary>
    public HyperBeamFxColorCatalog? HyperBeamFxColors { get; }
    /// <summary>Establishes primary-sheet pixel sharing before whole-sheet combination aliases.</summary>
    internal BeamTileCatalog(BeamTileAtlas[] sheets, BeamPaletteCatalog? palettes,
        HyperBeamFxColorCatalog? hyperBeamFxColors)
    {
        // Establish the five primary sheets before their combination aliases. Immutable
        // references share only matching pixels; separately supplied edits remain isolated.
        for (int index = 0; index < BeamTileAtlasDefinitions.SelectionCount; index++)
        {
            SamusBeamCombination selection = SamusBeamCombinations.FromTableIndex(index);
            if (BeamTileAtlasDefinitions.CanonicalSelection(selection) != selection) continue;
            if (BeamTileAtlasDefinitions.SharedTileSourceSelection(selection) is SamusBeamCombination source)
                sheets[index] = sheets[index].SharePixelsFrom(sheets[source.TableIndex], sharing: BeamTileSharing.SharedTiles);
        }
        for (int index = 0; index < BeamTileAtlasDefinitions.SelectionCount; index++)
        {
            SamusBeamCombination selection = SamusBeamCombinations.FromTableIndex(index);
            SamusBeamCombination source = BeamTileAtlasDefinitions.CanonicalSelection(selection);
            if (source != selection)
                sheets[index] = sheets[index].SharePixelsFrom(sheets[source.TableIndex], sharing: BeamTileSharing.WholeSheet);
        }
        this.sheets = sheets;
        Palettes = palettes;
        HyperBeamFxColors = hyperBeamFxColors;
    }

    /// <summary>Builds a catalog from file-context-validated atlases without decoding PNGs twice.</summary>
    internal static BeamTileCatalog FromAtlases(BeamTileAtlas[] sheets, BeamPaletteCatalog palettes,
        HyperBeamFxColorCatalog hyperBeamFxColors)
    {
        Ensure.NotNull(sheets);
        if (sheets.Length != BeamTileAtlasDefinitions.ArtworkCount || sheets.Any(sheet => sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new((BeamTileAtlas[])sheets.Clone(), palettes, hyperBeamFxColors);
    }

    /// <summary>Resolves a supported typed beam-tile identity to its immutable 256-byte planar transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) =>
        sheets[BeamTileAtlasDefinitions.ArtworkOrdinal(SelectionFor(asset))].Characters;

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

    /// <summary>The beam combination whose sheet a beam VRAM asset uploads: the inverse of <see cref="AssetFor"/>.</summary>
    /// <exception cref="InvalidDataException">The asset is not beam artwork.</exception>
    public static SamusBeamCombination SelectionFor(InstalledArtworkId asset) => asset switch
    {
        InstalledArtworkId.BeamPowerTiles => SamusBeamCombination.Power,
        InstalledArtworkId.BeamWaveTiles => SamusBeamCombination.Wave,
        InstalledArtworkId.BeamIceTiles => SamusBeamCombination.Ice,
        InstalledArtworkId.BeamIceWaveTiles => SamusBeamCombination.IceWave,
        InstalledArtworkId.BeamSpazerTiles => SamusBeamCombination.Spazer,
        InstalledArtworkId.BeamSpazerWaveTiles => SamusBeamCombination.SpazerWave,
        InstalledArtworkId.BeamSpazerIceTiles => SamusBeamCombination.SpazerIce,
        InstalledArtworkId.BeamSpazerIceWaveTiles => SamusBeamCombination.SpazerIceWave,
        InstalledArtworkId.BeamPlasmaTiles => SamusBeamCombination.Plasma,
        InstalledArtworkId.BeamPlasmaWaveTiles => SamusBeamCombination.PlasmaWave,
        InstalledArtworkId.BeamPlasmaIceTiles => SamusBeamCombination.PlasmaIce,
        InstalledArtworkId.BeamPlasmaIceWaveTiles => SamusBeamCombination.PlasmaIceWave,
        InstalledArtworkId.BeamChainsawTiles => SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection,
        InstalledArtworkId.BeamSpacetimeTiles => SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection,
        InstalledArtworkId.None or InstalledArtworkId.StandardHudTiles or InstalledArtworkId.EscapeTimerFirstTiles or
            InstalledArtworkId.EscapeTimerSecondTiles or InstalledArtworkId.ProjectileIceWaveTrailTiles or
            InstalledArtworkId.ProjectileMissileTrailTiles or InstalledArtworkId.GrapplePointFirstTiles or
            InstalledArtworkId.GrapplePointSecondTiles or InstalledArtworkId.GrapplePointThirdTiles or
            InstalledArtworkId.GrapplePointFourthTiles or InstalledArtworkId.GrappleHorizontalSegmentTiles or
            InstalledArtworkId.GrappleDiagonalSegmentTiles or InstalledArtworkId.GrappleVerticalSegmentTiles or
            InstalledArtworkId.KraidBg3RestoreQuarter0 or InstalledArtworkId.KraidBg3RestoreQuarter1 or
            InstalledArtworkId.KraidBg3RestoreQuarter2 or InstalledArtworkId.KraidBg3RestoreQuarter3 or
            InstalledArtworkId.GunshipLiftoffFirstTiles or InstalledArtworkId.GunshipLiftoffSecondTiles or
            InstalledArtworkId.GunshipLiftoffThirdTiles or InstalledArtworkId.GunshipLiftoffFourthTiles or
            InstalledArtworkId.GunshipLiftoffFifthTiles => throw new InvalidDataException($"Beam catalog cannot resolve {asset}."),
        InstalledArtworkId.StandardObjectTiles or
            InstalledArtworkId.HudTopRowTilemap => throw new ArgumentOutOfRangeException(nameof(asset), asset, "Undefined VRAM asset."),
        _ => throw new ArgumentOutOfRangeException(nameof(asset), asset, "Undefined VRAM asset."),
    };
}
