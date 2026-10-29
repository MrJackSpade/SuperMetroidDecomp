namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Exact source-key admission for the required compressed background library.</summary>
internal static class LibraryBackgroundClosedContractDefinitions
{
    /// <summary>Restricts background-library lookup to the reviewed tilemap identities and their defining catalog files.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Artwork.RoomBackgroundTilemaps", "library-background-exact-required-source-keys", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundTilemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Backgrounds/RoomBackgroundTilemap.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundSource.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundDestination.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundInstruction.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundProgram.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundProgramDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundProgramDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Rooms/LibraryBackgroundCommand.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundTilemapAtlas.cs")]),
    ];
}
