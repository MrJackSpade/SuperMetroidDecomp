namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    /// <summary>Reviews complete physical transfer records while accounting for placement, pose, and timing sources.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt", "BodyArtwork"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPixelDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseId.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOriginDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyDefinitionLayout.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusRenderingRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusTileTransferState.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusArmCannonPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleSwingFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Movement/SamusMovementRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileFlipFlags.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesBgTilemapWord.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesObjAttributeWord.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesOamHighTablePair.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesSpritemapXWord.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionDefinitions.cs")]),
    ];
}
