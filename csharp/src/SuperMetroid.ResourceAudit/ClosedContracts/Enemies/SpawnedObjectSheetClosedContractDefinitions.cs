namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Exact spawned-object pictures of each graphics set's owners (#1513, #1536).</summary>
internal static class SpawnedObjectSheetClosedContractDefinitions
{
    /// <summary>
    /// The spawned-object sheet, the generated spawns its pictures are named from, the composer and the loader that requires
    /// each installed sheet to hold exactly those pictures, and the presentation that names the picture an object draws.
    /// <c>ShowsNothing</c> answers over the same pictures: the loader requires the sheet's pictures and its pictures that
    /// show nothing to be exactly the owner's, and a name outside both throws (#1538, #1732).
    /// </summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.SpawnedObjectSheet", "spawned-object-sheet-exact-spawned-pictures", ["Frame", "ShowsNothing"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/SpawnedObjectSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySheetDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EncounterLook.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/InstalledEnemySheets.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/EnemyPresentation.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/SpawnedObjectSheetComposer.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/SpawnedObjectFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/SpawnedObjects.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/CommonSpawnedObjects.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EncounterFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Installation/EnemySheetFiles.cs")]),
    ];
}
