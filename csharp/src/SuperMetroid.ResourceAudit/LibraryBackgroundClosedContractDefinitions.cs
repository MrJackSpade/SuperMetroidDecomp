namespace SuperMetroid.ResourceAudit;

/// <summary>Exact source-key admission for the required compressed background library.</summary>
internal static class LibraryBackgroundClosedContractDefinitions
{
    /// <summary>Restricts background-library lookup to the reviewed source keys and their defining catalog files.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog", "library-background-exact-required-source-keys", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSources.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundCommand.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs")]),
    ];
}
