using System.Text.Json;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Writes installed sprite-sheet manifests with the settings the game reads them with.</summary>
public static class SpriteSheetManifestWriter
{
    /// <summary>Serializes a sheet manifest.</summary>
    public static byte[] Write(SpriteSheetDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(Ensure.NotNull(document), SpriteSheetFile.Options);
}
