namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Exact source-key admission for the required compressed background library.</summary>
internal static class LibraryBackgroundClosedContractDefinitions
{
    /// <summary>Restricts background-library lookup to the reviewed tilemap identities and their defining catalog files.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Artwork.RoomBackgroundTilemapCatalog", "library-background-exact-required-source-keys", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/RoomBackgroundTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundSource.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundDestination.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundInstruction.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundProgramDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundProgramDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/LibraryBackgroundCommand.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundTilemapAtlas.cs")]),
    ];
}
