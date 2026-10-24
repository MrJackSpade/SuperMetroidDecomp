using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The composed enemy sheets by installed name, and the manifest naming each set's sheet for each enemy.</summary>
/// <param name="Sheets">Each distinct sheet by installed name.</param>
/// <param name="Manifest">Each graphics set's sheet name for each of its enemies.</param>
internal sealed record EnemySheetComposition(IReadOnlyDictionary<string, ComposedSpriteSheet> Sheets, EnemySheetManifestDocument Manifest);
