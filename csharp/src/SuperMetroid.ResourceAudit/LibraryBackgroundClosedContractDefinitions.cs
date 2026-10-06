namespace SuperMetroid.ResourceAudit;

/// <summary>Exact source-key admission for the required compressed background library.</summary>
internal static class LibraryBackgroundClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomBackgroundTilemapCatalog", "library-background-exact-required-source-keys", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapCatalog.cs", "AB1859D5BAF137EF962E4FF561062FE045F86BE4CCC33A746BDC9818C917D15F"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs", "75EC5A0418AC85318C8D794A698619FC7AFA4BF498FA8B58A78ECAEE6BD4CC88"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramDefinitions.cs", "FCB94D6C6D3C1DE901B1F169CEBD74DCCB9B368A10AF98D0B70B4A0799A45039"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs", "5B95FAC0B0C578C778C0E4F1FCEF735390593A1ADBBBCDD21AD3A7B2A1BDFBB9"),
             new("csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundCommand.cs", "61A7E66798DE3AA87448D5CE618C97E9C5C70D32D5AF1018AA28C0B13445B460"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs", "9FA2182B31CC5B874EC8DD279E72208D7EAA60147C92BF6099B21DCF99863A07")],
            "Public construction checks all 58 required source IDs derived from the reviewed decompression commands for a nonnull compiled atlas, not just dictionary count, and copies the dictionary before publication. Only the immutable required key domain is complete. This proves successfully constructed catalog membership, not arbitrary sources, caller command selection, WRAM/VRAM destinations, page choice or rendered tiles."),
    ];
}
