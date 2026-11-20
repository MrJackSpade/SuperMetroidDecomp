using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed beam characters by combination, with the beam palettes; the projectile sheets are composed from them.</summary>
public sealed class BeamTileCatalog : IInstalledArtworkContent
{
    /// <summary>Every pictured combination's installed 256-byte beam atlas.</summary>
    private readonly IReadOnlyDictionary<SamusBeamCombination, BeamTileAtlas> sheets;
    /// <summary>Gets the optional installed beam and grapple palette catalog paired with these tile sheets.</summary>
    public InstalledBeamPalettes? Palettes { get; }
    /// <summary>Gets the optional installed fixed-color cycle used by the Hyper Beam visual effect.</summary>
    public HyperBeamFxColours? HyperBeamFxColors { get; }
    /// <summary>Keeps every combination's installed atlas, each read as installed.</summary>
    private BeamTileCatalog(IReadOnlyDictionary<SamusBeamCombination, BeamTileAtlas> sheets, InstalledBeamPalettes? palettes,
        HyperBeamFxColours? hyperBeamFxColors)
    {
        this.sheets = sheets;
        Palettes = palettes;
        HyperBeamFxColors = hyperBeamFxColors;
    }

    /// <summary>Builds a catalog from file-context-validated atlases without decoding PNGs twice.</summary>
    internal static BeamTileCatalog FromAtlases(IReadOnlyDictionary<SamusBeamCombination, BeamTileAtlas> sheets, InstalledBeamPalettes palettes,
        HyperBeamFxColours hyperBeamFxColors)
    {
        Ensure.NotNull(sheets);
        if (sheets.Count != BeamTileAtlasDefinitions.Pictured.Count ||
            BeamTileAtlasDefinitions.Pictured.Any(selection => !sheets.TryGetValue(selection, out BeamTileAtlas? sheet) || sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new(sheets.ToDictionary(entry => entry.Key, entry => entry.Value), palettes, hyperBeamFxColors);
    }

    /// <summary>Resolves a supported typed beam-tile identity to its immutable 256-byte planar transfer.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) =>
        Ensure.Listed(sheets, SelectionFor(asset)).Characters;

    /// <summary>
    /// Each beam combination that has artwork and the VRAM asset holding its sheet, the one pairing both directions read
    /// (#2557); the Spazer and Plasma combinations have none.
    /// </summary>
    private static readonly FrozenDictionary<SamusBeamCombination, InstalledArtworkId> Assets = new Dictionary<SamusBeamCombination, InstalledArtworkId>
    {
        [SamusBeamCombination.Power] = InstalledArtworkId.BeamPowerTiles,
        [SamusBeamCombination.Wave] = InstalledArtworkId.BeamWaveTiles,
        [SamusBeamCombination.Ice] = InstalledArtworkId.BeamIceTiles,
        [SamusBeamCombination.IceWave] = InstalledArtworkId.BeamIceWaveTiles,
        [SamusBeamCombination.Spazer] = InstalledArtworkId.BeamSpazerTiles,
        [SamusBeamCombination.SpazerWave] = InstalledArtworkId.BeamSpazerWaveTiles,
        [SamusBeamCombination.SpazerIce] = InstalledArtworkId.BeamSpazerIceTiles,
        [SamusBeamCombination.SpazerIceWave] = InstalledArtworkId.BeamSpazerIceWaveTiles,
        [SamusBeamCombination.Plasma] = InstalledArtworkId.BeamPlasmaTiles,
        [SamusBeamCombination.PlasmaWave] = InstalledArtworkId.BeamPlasmaWaveTiles,
        [SamusBeamCombination.PlasmaIce] = InstalledArtworkId.BeamPlasmaIceTiles,
        [SamusBeamCombination.PlasmaIceWave] = InstalledArtworkId.BeamPlasmaIceWaveTiles,
        [SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection] = InstalledArtworkId.BeamChainsawTiles,
        [SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection] = InstalledArtworkId.BeamSpacetimeTiles,
    }.ToFrozenDictionary();

    /// <summary>The beam combination whose sheet each beam VRAM asset uploads: <see cref="Assets"/> read the other way.</summary>
    private static readonly FrozenDictionary<InstalledArtworkId, SamusBeamCombination> Selections =
        Assets.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Maps a supported native equipped-beam selection to its queued VRAM asset identity.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no artwork ($0C or $0F).</exception>
    public static InstalledArtworkId AssetFor(SamusBeamCombination selection) =>
        Assets.TryGetValue(selection, out InstalledArtworkId asset)
            ? asset
            : throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork.");

    /// <summary>The beam combination whose sheet a beam VRAM asset uploads: the inverse of <see cref="AssetFor"/>.</summary>
    /// <exception cref="InvalidDataException">The asset is not beam artwork.</exception>
    public static SamusBeamCombination SelectionFor(InstalledArtworkId asset) =>
        Selections.TryGetValue(asset, out SamusBeamCombination selection)
            ? selection
            : throw new InvalidDataException($"Beam catalog cannot resolve {asset}.");
}
