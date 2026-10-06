using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable beam sheets resolved at NMI; pending state stores identities, never PNG bytes.</summary>
public sealed class BeamTileCatalog : IVramAssetProvider
{
    private readonly BeamTileAtlas[] sheets;
    public BeamPaletteCatalog? Palettes { get; }
    public HyperBeamFxColorCatalog? HyperBeamFxColors { get; }
    private BeamTileCatalog(BeamTileAtlas[] sheets, BeamPaletteCatalog? palettes,
        HyperBeamFxColorCatalog? hyperBeamFxColors)
    {
        // Establish the five primary sheets before their combination aliases. Immutable
        // references share only matching pixels; separately supplied edits remain isolated.
        for (int selection = 0; selection < sheets.Length; selection++)
        {
            if (BeamTileAtlasDefinitions.CanonicalSelection(selection) != selection) continue;
            int source = BeamTileAtlasDefinitions.SharedTileSourceSelection(selection);
            if (source >= 0) sheets[selection] = sheets[selection].SharePixelsFrom(sheets[source], wholeSheet: false);
        }
        for (int selection = 0; selection < sheets.Length; selection++)
        {
            int source = BeamTileAtlasDefinitions.CanonicalSelection(selection);
            if (source != selection) sheets[selection] = sheets[selection].SharePixelsFrom(sheets[source], wholeSheet: true);
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
        if (sheets.Length != BeamTileAtlasDefinitions.SelectionCount || sheets.Any(sheet => sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new((BeamTileAtlas[])sheets.Clone(), palettes, hyperBeamFxColors);
    }
    public static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files,
        BeamPaletteCatalog? palettes = null, HyperBeamFxColorCatalog? hyperBeamFxColors = null)
    {
        var sheets = new BeamTileAtlas[BeamTileAtlasDefinitions.SelectionCount];
        for (int i = 0; i < sheets.Length; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(i);
            if (!files.TryGetValue(name, out var png) || png is null)
                throw new InvalidDataException($"Missing beam artwork {name}.");
            sheets[i] = BeamTileAtlas.Load(new MemoryStream(png, writable: false), i);
        }
        return new(sheets, palettes, hyperBeamFxColors);
    }

    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        int selection = (int)asset - (int)VramAssetId.BeamPowerTiles;
        if ((uint)selection >= sheets.Length) throw new InvalidDataException($"Beam catalog cannot resolve {asset}.");
        return sheets[selection].Transfer;
    }

    public static VramAssetId AssetFor(int selection)
    {
        if ((uint)selection >= BeamTileAtlasDefinitions.SelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return (VramAssetId)((int)VramAssetId.BeamPowerTiles + selection);
    }
}
