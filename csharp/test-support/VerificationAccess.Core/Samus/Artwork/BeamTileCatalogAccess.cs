namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    /// <summary>Builds a focused beam catalog from named PNG bytes and optional palette catalogs.</summary>
    extension(BeamTileCatalog)
    {
        /// <summary>Requires every beam artwork file and decodes each under its authored selection.</summary>
        internal static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files,
            BeamPaletteCatalog? palettes = null, HyperBeamFxColorCatalog? hyperBeamFxColors = null)
        {
            var sheets = new BeamTileAtlas[BeamTileAtlasDefinitions.ArtworkCount];
            for (int i = 0; i < sheets.Length; i++)
            {
                string name = BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i));
                if (!files.TryGetValue(name, out var png) || png is null)
                    throw new InvalidDataException($"Missing beam artwork {name}.");
                sheets[i] = BeamTileAtlas.Load(new MemoryStream(png, writable: false), BeamTileAtlasDefinitions.SelectionAt(i));
            }
            return new BeamTileCatalog(sheets, palettes, hyperBeamFxColors);
        }
    }
}
