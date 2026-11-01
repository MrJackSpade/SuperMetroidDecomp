using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The encoded enemy sheets by installed name, and the manifest naming each set's sheet for each enemy.</summary>
/// <param name="Sheets">Each distinct sheet's files by installed name, in the order they were named.</param>
/// <param name="Manifest">Each graphics set's sheet name for each of its enemies.</param>
internal sealed record EnemySheetComposition(IReadOnlyDictionary<string, EncodedSpriteSheet> Sheets, EnemySheetManifestDocument Manifest);
