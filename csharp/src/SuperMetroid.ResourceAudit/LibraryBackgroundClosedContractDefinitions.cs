namespace SuperMetroid.ResourceAudit;

/// <summary>Exact source-key admission for the required compressed background library.</summary>
internal static class LibraryBackgroundClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog", "library-background-exact-required-source-keys", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapCatalog.cs", "AB1859D5BAF137EF962E4FF561062FE045F86BE4CCC33A746BDC9818C917D15F"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs", "FC1E1B32EDA63F828A112AB17864551E72E8F8B92A50A6D0C31C58CE0DCB1BB7"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs", "9FA2182B31CC5B874EC8DD279E72208D7EAA60147C92BF6099B21DCF99863A07")],
            "Public construction checks all 58 required source IDs for a nonnull compiled atlas, not just dictionary count, and copies the dictionary before publication. Only the immutable required key domain is complete. This proves successfully constructed catalog membership, not arbitrary sources, caller command selection, WRAM/VRAM destinations, page choice or rendered tiles."),
    ];
}
