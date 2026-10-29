namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Installed body half-picture availability, independently reviewed from Samus placement tables.</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    /// <summary>Reviews complete physical transfer records while accounting for placement, pose, and timing sources.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusBodyArtwork", "samus-complete-installed-frame-pictures",
            ["Frame"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusBodyArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyArtworkContent.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyFramePictures.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPicture.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPictures.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPlacementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPlacements.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseId.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseNumbering.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOriginDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOrigins.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusDrawingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusBodyFrameState.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusSpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyHalfFrame.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusPoseSpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledGrappleSwingFrames.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelays.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Movement/SamusMovementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Spatial/TileFlip.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SnesBgTilemapWord.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpritePart.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionRadii.cs")]),
    ];
}
