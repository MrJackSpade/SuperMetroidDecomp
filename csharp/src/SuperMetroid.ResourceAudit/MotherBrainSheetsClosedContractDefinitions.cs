namespace SuperMetroid.ResourceAudit;

/// <summary>Complete source and page-size admission for fixed Mother Brain transfer sheets.</summary>
internal static class MotherBrainSheetsClosedContractDefinitions
{
    /// <summary>Admits the complete Mother Brain special-sheet source set to the fixed-page presentation audit.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainSpecialSpriteArtworkCatalog", "mother-brain-special-sheets-complete-pages", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainSpecialSpriteArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainSpecialSpriteArtworkDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomCharacterAtlas.cs")]),
    ];
}
