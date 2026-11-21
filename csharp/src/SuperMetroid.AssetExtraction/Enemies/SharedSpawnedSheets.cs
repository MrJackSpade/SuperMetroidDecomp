namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed spawned-object sheets with their common art held once (#1513).</summary>
/// <param name="Shares">Where each laid-out spawned-object sheet's pictures are installed, in the order the layouts were given.</param>
internal sealed record SharedSpawnedSheets(SpawnedSheetShare[] Shares);
