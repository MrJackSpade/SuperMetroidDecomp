namespace SuperMetroid.ResourceAudit;

/// <summary>Physical DMA-record availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.SamusBodyArtworkCatalog", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionAddress", "DefinitionAt"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusRenderingRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusTileTransferState.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesTileWords.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs")]),
    ];
}
