using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where one laid-out spawned-object sheet's pictures are installed once its common art is held once (#1513): the sheet of
/// the pictures particular to it, and the common sheet each of its other pictures is held on.
/// </summary>
/// <param name="Own">The sheet of the pictures no other spawned-object sheet holds; none when every picture is common.</param>
/// <param name="Common">For each picture held on a common sheet, by picture name, that sheet's place among the common sheets.</param>
internal sealed record SpawnedSheetShare(EncodedSpriteSheet? Own, Dictionary<string, int> Common);
