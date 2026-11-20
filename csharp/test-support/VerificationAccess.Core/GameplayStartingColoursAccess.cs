using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The starting background colours, which production decodes into no owner (#2570): checks decode them from the import's
/// starting image as <see cref="GameplayColourImage"/> decodes the colours production keeps.
/// </summary>
internal static class GameplayStartingColoursAccess
{
    /// <summary>The background colours, in the cartridge's order, of the starting image the import data at <paramref name="importData"/> holds.</summary>
    internal static RgbColor[] BackgroundColours(string importData)
    {
        GameplayBasePaletteDocument document = JsonAssetDocument.Read<GameplayBasePaletteDocument>(
            File.ReadAllBytes(Path.Combine(importData, GameplayBasePaletteFormat.ArtworkFileName)), GameplayBasePaletteFormat.JsonOptions);
        return [.. document.Initial.AsSpan(GameplayColourPositionDefinitionsConstants.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours)
            .ToArray().Select(colour => colour.ToRgbColor())];
    }
}
