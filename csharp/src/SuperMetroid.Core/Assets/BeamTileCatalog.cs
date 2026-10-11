using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable beam sheets resolved at NMI; pending state stores identities, never PNG bytes.</summary>
public sealed class BeamTileCatalog : IVramAssetProvider, IInstalledArtworkTransferSource
{
    private readonly BeamTileAtlas[] sheets;
    /// <summary>Gets the optional installed beam and grapple palette catalog paired with these tile sheets.</summary>
    public BeamPaletteCatalog? Palettes { get; }
    /// <summary>Gets the optional installed fixed-color cycle used by the Hyper Beam visual effect.</summary>
    public HyperBeamFxColorCatalog? HyperBeamFxColors { get; }
    private BeamTileCatalog(BeamTileAtlas[] sheets, BeamPaletteCatalog? palettes,
        HyperBeamFxColorCatalog? hyperBeamFxColors)
    {
        // Establish the five primary sheets before their combination aliases. Immutable
        // references share only matching pixels; separately supplied edits remain isolated.
        for (int index = 0; index < BeamTileAtlasDefinitions.SelectionCount; index++)
        {
            SamusBeamCombination selection = SamusBeamCombinations.FromTableIndex(index);
            if (BeamTileAtlasDefinitions.CanonicalSelection(selection) != selection) continue;
            if (BeamTileAtlasDefinitions.SharedTileSourceSelection(selection) is SamusBeamCombination source)
                sheets[index] = sheets[index].SharePixelsFrom(sheets[source.TableIndex], wholeSheet: false);
        }
        for (int index = 0; index < BeamTileAtlasDefinitions.SelectionCount; index++)
        {
            SamusBeamCombination selection = SamusBeamCombinations.FromTableIndex(index);
            SamusBeamCombination source = BeamTileAtlasDefinitions.CanonicalSelection(selection);
            if (source != selection)
                sheets[index] = sheets[index].SharePixelsFrom(sheets[source.TableIndex], wholeSheet: true);
        }
        this.sheets = sheets;
        Palettes = palettes;
        HyperBeamFxColors = hyperBeamFxColors;
    }

    /// <summary>Builds a catalog from file-context-validated atlases without decoding PNGs twice.</summary>
    internal static BeamTileCatalog FromAtlases(BeamTileAtlas[] sheets, BeamPaletteCatalog palettes,
        HyperBeamFxColorCatalog hyperBeamFxColors)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        if (sheets.Length != BeamTileAtlasDefinitions.ArtworkCount || sheets.Any(sheet => sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new((BeamTileAtlas[])sheets.Clone(), palettes, hyperBeamFxColors);
    }

    /// <summary>Resolves a supported typed beam-tile identity to its immutable 256-byte planar transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
        sheets[BeamTileAtlasDefinitions.ArtworkOrdinal(SelectionFor(asset))].Transfer;

    /// <summary>Resolves restored native beam uploads through installed artwork, without ROM DMA.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (!Enum.IsDefined((BeamTileSource)sourceAddress) || byteCount != BeamTileAtlasDefinitions.ByteCount)
        {
            data = default;
            return false;
        }
        data = Resolve(AssetFor(BeamTileAtlasDefinitions.LegacySelectionFor((BeamTileSource)sourceAddress)));
        return true;
    }

    /// <summary>Maps a supported native equipped-beam selection to its queued VRAM asset identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no artwork ($0C or $0F).</exception>
    public static VramAssetId AssetFor(SamusBeamCombination selection) => selection switch
    {
        SamusBeamCombination.Power => VramAssetId.BeamPowerTiles,
        SamusBeamCombination.Wave => VramAssetId.BeamWaveTiles,
        SamusBeamCombination.Ice => VramAssetId.BeamIceTiles,
        SamusBeamCombination.IceWave => VramAssetId.BeamIceWaveTiles,
        SamusBeamCombination.Spazer => VramAssetId.BeamSpazerTiles,
        SamusBeamCombination.SpazerWave => VramAssetId.BeamSpazerWaveTiles,
        SamusBeamCombination.SpazerIce => VramAssetId.BeamSpazerIceTiles,
        SamusBeamCombination.SpazerIceWave => VramAssetId.BeamSpazerIceWaveTiles,
        SamusBeamCombination.Plasma => VramAssetId.BeamPlasmaTiles,
        SamusBeamCombination.PlasmaWave => VramAssetId.BeamPlasmaWaveTiles,
        SamusBeamCombination.PlasmaIce => VramAssetId.BeamPlasmaIceTiles,
        SamusBeamCombination.PlasmaIceWave => VramAssetId.BeamPlasmaIceWaveTiles,
        Game.ChainsawBeamGraphicsDefinitions.Selection => VramAssetId.BeamChainsawTiles,
        Game.SpacetimeBeamGraphicsDefinitions.Selection => VramAssetId.BeamSpacetimeTiles,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork."),
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
    };

    /// <summary>The beam combination whose sheet a beam VRAM asset uploads: the inverse of <see cref="AssetFor"/>.</summary>
    /// <exception cref="InvalidDataException">The asset is not beam artwork.</exception>
    public static SamusBeamCombination SelectionFor(VramAssetId asset) => asset switch
    {
        VramAssetId.BeamPowerTiles => SamusBeamCombination.Power,
        VramAssetId.BeamWaveTiles => SamusBeamCombination.Wave,
        VramAssetId.BeamIceTiles => SamusBeamCombination.Ice,
        VramAssetId.BeamIceWaveTiles => SamusBeamCombination.IceWave,
        VramAssetId.BeamSpazerTiles => SamusBeamCombination.Spazer,
        VramAssetId.BeamSpazerWaveTiles => SamusBeamCombination.SpazerWave,
        VramAssetId.BeamSpazerIceTiles => SamusBeamCombination.SpazerIce,
        VramAssetId.BeamSpazerIceWaveTiles => SamusBeamCombination.SpazerIceWave,
        VramAssetId.BeamPlasmaTiles => SamusBeamCombination.Plasma,
        VramAssetId.BeamPlasmaWaveTiles => SamusBeamCombination.PlasmaWave,
        VramAssetId.BeamPlasmaIceTiles => SamusBeamCombination.PlasmaIce,
        VramAssetId.BeamPlasmaIceWaveTiles => SamusBeamCombination.PlasmaIceWave,
        VramAssetId.BeamChainsawTiles => Game.ChainsawBeamGraphicsDefinitions.Selection,
        VramAssetId.BeamSpacetimeTiles => Game.SpacetimeBeamGraphicsDefinitions.Selection,
        VramAssetId.None or VramAssetId.StandardHudTiles or VramAssetId.EscapeTimerFirstTiles or
            VramAssetId.EscapeTimerSecondTiles or VramAssetId.ProjectileIceWaveTrailTiles or
            VramAssetId.ProjectileMissileTrailTiles or VramAssetId.GrapplePointFirstTiles or
            VramAssetId.GrapplePointSecondTiles or VramAssetId.GrapplePointThirdTiles or
            VramAssetId.GrapplePointFourthTiles or VramAssetId.GrappleHorizontalSegmentTiles or
            VramAssetId.GrappleDiagonalSegmentTiles or VramAssetId.GrappleVerticalSegmentTiles or
            VramAssetId.KraidBg3RestoreQuarter0 or VramAssetId.KraidBg3RestoreQuarter1 or
            VramAssetId.KraidBg3RestoreQuarter2 or VramAssetId.KraidBg3RestoreQuarter3 or
            VramAssetId.GunshipLiftoffFirstTiles or VramAssetId.GunshipLiftoffSecondTiles or
            VramAssetId.GunshipLiftoffThirdTiles or VramAssetId.GunshipLiftoffFourthTiles or
            VramAssetId.GunshipLiftoffFifthTiles => throw new InvalidDataException($"Beam catalog cannot resolve {asset}."),
        _ => throw new ArgumentOutOfRangeException(nameof(asset), asset, "Undefined VRAM asset."),
    };
}
