using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable beam sheets resolved at NMI; pending state stores identities, never PNG bytes.</summary>
public sealed class BeamTileCatalog : IVramAssetProvider
{
    private readonly BeamTileAtlas[] sheets;
    public BeamPaletteCatalog? Palettes { get; }
    private BeamTileCatalog(BeamTileAtlas[] sheets, BeamPaletteCatalog? palettes)
    { this.sheets = sheets; Palettes = palettes; }
    public static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files, BeamPaletteCatalog? palettes = null)
    {
        var sheets = new BeamTileAtlas[BeamTileAtlasDefinitions.SelectionCount];
        for (int i = 0; i < sheets.Length; i++)
        {
            string name = BeamTileAtlasDefinitions.FileName(i);
            if (!files.TryGetValue(name, out var png) || png is null)
                throw new InvalidDataException($"Missing beam artwork {name}.");
            sheets[i] = BeamTileAtlas.Load(new MemoryStream(png, writable: false));
        }
        return new(sheets, palettes);
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
