using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed spawned-object sheets with their common art held once (#1513).</summary>
/// <param name="Common">The common sheets, one for each colouring the common pictures are drawn in.</param>
/// <param name="Shares">Where each laid-out spawned-object sheet's pictures are installed, in the order the layouts were given.</param>
internal sealed record SharedSpawnedSheets(InstalledSheet[] Common, SpawnedSheetShare[] Shares);
