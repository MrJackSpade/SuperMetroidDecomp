namespace SuperMetroid.ResourceAudit;

/// <summary>Complete source and page-size admission for fixed Mother Brain transfer sheets.</summary>
internal static class MotherBrainSheetsClosedContractDefinitions
{
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainSpecialSpriteArtworkCatalog", "mother-brain-special-sheets-complete-pages", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs")]),
    ];
}
