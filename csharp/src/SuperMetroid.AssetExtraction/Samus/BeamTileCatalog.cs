using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed beam characters by combination, with the beam palettes; the projectile sheets are composed from them.</summary>
public sealed class BeamTileCatalog
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

    /// <summary>The immutable 256-byte planar transfer of a beam combination's characters.</summary>
    public ReadOnlyMemory<byte> Resolve(SamusBeamCombination beam) =>
        Ensure.Listed(sheets, beam).Characters;

}
