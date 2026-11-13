namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Reviewed complete projectile/rope visual domains; no firing, motion, damage or sound is exercised.</summary>
internal static class ProjectileClosedContractDefinitions
{
    /// <summary>Lists the reviewed projectile and grapple presentation entry points with the source files required to keep each visual contract self-contained.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements", "flare-complete-standing-running-offsets", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/ChargeFlarePlacements.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/ChargeFlarePlacement.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/ChargeFlareSource.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusFlareAim.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusTurningAim.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusMuzzleOrigins.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/SamusProjectileFamily.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/SamusProjectileFamilies.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/SamusProjectileFamilyDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusProjectileDirection.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Grapple/GrappleFiringDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Grapple/GrappleFiring.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Grapple/GrappleBeamDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.ChargeFlareSprites", "flare-complete-private-composition-set", ["Frame"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/ChargeFlareSprites.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/ChargeFlareSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/InstalledProjectileSprites.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectileSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusColourDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpriteComposition.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.InstalledProjectileTrails", "trail-complete-timed-appearances", ["Resolve"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/InstalledProjectileTrails.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectileTrailVisualDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings", "projectile-complete-timed-bindings", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledProjectileFrameBindings.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusProjectileRecordDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusProjectileFrame.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectileSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusColourDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.InstalledGrappleSprites", "grapple-complete-endpoint-and-segments", ["Segment"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/InstalledGrappleSprites.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/GrappleSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames", "grapple-complete-byte-angle-display-map", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/InstalledGrappleSwingFrames.cs")]),
        new("SuperMetroid.AssetExtraction.Samus.InstalledProjectileSprites", "projectile-production-complete-compositions", ["Frame"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Samus/InstalledProjectileSprites.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpriteComposition.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectileSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Palettes/SamusColourDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Samus/ChargeFlareSprites.cs")]),
    ];
}
