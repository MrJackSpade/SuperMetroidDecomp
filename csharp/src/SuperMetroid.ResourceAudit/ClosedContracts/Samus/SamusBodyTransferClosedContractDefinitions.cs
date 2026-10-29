namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Installed body half-picture availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    /// <summary>Reviews complete physical transfer records while accounting for placement, pose, and timing sources.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusBodyArtwork", "samus-complete-physical-transfer-records",
            ["Frame", "DefinitionRecord"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusBodyArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyFrameSharing.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPlacements.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPoseFrames.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseId.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseNumbering.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOriginDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOrigins.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyDefinitionLayout.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyDefinitionStarts.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusDrawingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusBodyFrameState.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusSpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapSharing.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledGrappleSwingFrames.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelays.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Movement/SamusMovementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapPoseBases.cs"),
             new("csharp/src/SuperMetroid.Core/Spatial/TileFlip.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SnesBgTilemapWord.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpritePart.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionRadii.cs")]),
    ];
}
