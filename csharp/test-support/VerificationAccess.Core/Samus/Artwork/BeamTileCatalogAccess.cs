using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    /// <summary>Builds a focused beam catalog from named PNG bytes and optional palette catalogs.</summary>
    extension(BeamTileCatalog)
    {
        /// <summary>Requires every beam artwork file and decodes each under its authored selection.</summary>
        internal static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files,
            InstalledBeamPalettes? palettes = null, HyperBeamFxColours? hyperBeamFxColors = null)
        {
            var sheets = new Dictionary<global::SuperMetroid.Core.Progression.SamusBeamCombination, BeamTileAtlas>();
            foreach (global::SuperMetroid.Core.Progression.SamusBeamCombination selection in BeamTileAtlasDefinitions.Pictured)
            {
                string name = BeamTileAtlasDefinitions.FileName(selection);
                if (!files.TryGetValue(name, out var png) || png is null)
                    throw new InvalidDataException($"Missing beam artwork {name}.");
                sheets[selection] = BeamTileAtlas.Load(new MemoryStream(png, writable: false), selection);
            }
            return global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.AssetExtraction.Samus.BeamTileCatalog>([typeof(IReadOnlyDictionary<global::SuperMetroid.Core.Progression.SamusBeamCombination, BeamTileAtlas>), typeof(global::SuperMetroid.AssetExtraction.Samus.Artwork.InstalledBeamPalettes), typeof(global::SuperMetroid.AssetExtraction.Samus.Artwork.HyperBeamFxColours)], new object?[] { sheets, palettes, hyperBeamFxColors });
        }
    }
    /// <summary>Exposes <see cref="BeamTileCatalog"/> state that only verification reads.</summary>
    extension(BeamTileCatalog self)
    {
        /// <summary>Resolves restored native beam uploads through installed artwork, without ROM DMA.</summary>
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            if (!Enum.IsDefined((BeamTileSource)sourceAddress) || byteCount != BeamTileAtlasDefinitions.ByteCount)
            {
                data = default;
                return false;
            }
            data = self.Resolve(BeamTileAtlasDefinitionsConstants.LegacySelectionFor((BeamTileSource)sourceAddress));
            return true;
        }
    }
}
