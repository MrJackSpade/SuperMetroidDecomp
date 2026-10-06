using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable beam sheets resolved at NMI; pending state stores identities, never PNG bytes.</summary>
public sealed class BeamTileCatalog : IVramAssetProvider, IInstalledArtworkTransferSource
{
    private readonly BeamTileAtlas[] sheets;
    public BeamPaletteCatalog? Palettes { get; }
    public HyperBeamFxColorCatalog? HyperBeamFxColors { get; }
    private BeamTileCatalog(BeamTileAtlas[] sheets, BeamPaletteCatalog? palettes,
        HyperBeamFxColorCatalog? hyperBeamFxColors)
    { this.sheets = sheets; Palettes = palettes; HyperBeamFxColors = hyperBeamFxColors; }

    /// <summary>Builds a catalog from file-context-validated atlases without decoding PNGs twice.</summary>
    internal static BeamTileCatalog FromAtlases(BeamTileAtlas[] sheets, BeamPaletteCatalog palettes,
        HyperBeamFxColorCatalog hyperBeamFxColors)
    {
        ArgumentNullException.ThrowIfNull(sheets);
        if (sheets.Length != BeamTileAtlasDefinitions.ArtworkCount || sheets.Any(sheet => sheet is null))
            throw new ArgumentException("Every beam selection needs a compiled atlas.", nameof(sheets));
        return new((BeamTileAtlas[])sheets.Clone(), palettes, hyperBeamFxColors);
    }
    public static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files,
        BeamPaletteCatalog? palettes = null, HyperBeamFxColorCatalog? hyperBeamFxColors = null)
    {
        var sheets = new BeamTileAtlas[BeamTileAtlasDefinitions.ArtworkCount];
        for (int i = 0; i < sheets.Length; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i));
            if (!files.TryGetValue(name, out var png) || png is null)
                throw new InvalidDataException($"Missing beam artwork {name}.");
            sheets[i] = BeamTileAtlas.Load(new MemoryStream(png, writable: false));
        }
        return new(sheets, palettes, hyperBeamFxColors);
    }

    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        if (asset == VramAssetId.BeamChainsawTiles) return sheets[BeamTileAtlasDefinitions.SelectionCount].Transfer;
        if (asset == VramAssetId.BeamSpacetimeTiles) return sheets[BeamTileAtlasDefinitions.SelectionCount + 1].Transfer;
        int selection = (int)asset - (int)VramAssetId.BeamPowerTiles;
        if ((uint)selection >= BeamTileAtlasDefinitions.SelectionCount) throw new InvalidDataException($"Beam catalog cannot resolve {asset}.");
        return sheets[selection].Transfer;
    }

    /// <summary>Resolves restored native beam uploads through installed artwork, without ROM DMA.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        int selection = BeamTileAtlasDefinitions.LegacySelectionFor(sourceAddress);
        if (selection >= 0 && byteCount == BeamTileAtlasDefinitions.ByteCount)
        {
            data = Resolve(AssetFor(selection));
            return true;
        }
        data = default;
        return false;
    }

    public static VramAssetId AssetFor(int selection)
    {
        if (selection == Game.ChainsawBeamGraphicsDefinitions.Selection) return VramAssetId.BeamChainsawTiles;
        if (selection == Game.SpacetimeBeamGraphicsDefinitions.Selection) return VramAssetId.BeamSpacetimeTiles;
        if ((uint)selection >= BeamTileAtlasDefinitions.SelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return (VramAssetId)((int)VramAssetId.BeamPowerTiles + selection);
    }
}
