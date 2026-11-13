namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Installed body frame placement availability, independently reviewed from Samus placement tables (#1585).</summary>
internal static class SamusBodyTransferClosedContractDefinitions
{
    /// <summary>Reviews the installed frames' placements while accounting for pose and timing sources.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.InstalledSamusBodyArtwork", "samus-complete-installed-frame-placements",
            ["OriginRise"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledSamusBodyArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyArtworkContent.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseId.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOriginDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseProjectileOrigins.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusUnauthoredPoseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusUnauthoredPoseDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Compatibility/UnauthoredPoseReads.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseAim.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusFlareAim.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusTurningAim.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/SamusFacingDirection.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusProjectileDirection.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Drawing/SamusPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledGrappleSwingFrames.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelays.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Movement/SamusMovementDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Spatial/PictureFlip.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SnesBgTilemapWord.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpritePart.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseCollisionRadii.cs")]),
    ];
}
