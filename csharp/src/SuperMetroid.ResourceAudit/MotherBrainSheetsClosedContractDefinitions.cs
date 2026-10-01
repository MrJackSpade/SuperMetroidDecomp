namespace SuperMetroid.ResourceAudit;

/// <summary>Complete source and page-size admission for fixed Mother Brain transfer sheets.</summary>
internal static class MotherBrainSheetsClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainSpecialSpriteArtworkCatalog", "mother-brain-special-sheets-complete-pages", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkCatalog.cs", "D9276621440C24A335CD2A127B35A1E48478CEEA8F3D01A06FBFA3995DC69F63"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs", "4CF6AF09EF81015FDAD738612261276031AFEDC5308C93C2D148E1D3A842D5AF"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs", "0C2CD85F446A356CF2A0E226E7F9D45C64F23C118A58097058E2B33152F97512")],
            "Public construction copies the dictionary and requires exactly the four immutable source identities, all nonnull and with complete native page lengths. Get selects only by source ID, not a caller-authored sheet record. Required source membership and page availability are complete; this does not certify arbitrary IDs, transfer selection, destination placement, host binding, timing or pixels."),
    ];
}
