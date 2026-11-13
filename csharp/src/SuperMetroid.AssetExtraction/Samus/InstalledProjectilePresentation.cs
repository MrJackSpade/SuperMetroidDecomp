using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed projectile presentation as the game reads it (#1636): its documents, loaded, and the identity of the
/// installed files they were loaded from. The beam, trail and grapple characters and the beam colours are import data the
/// installation never holds (<see cref="ImportProjectileArtwork"/>, #1637); the game draws the sheets composed from them.
/// </summary>
/// <param name="Catalog">Projectile, bomb and flare compositions.</param>
/// <param name="ContentSha256">Identity of the installed projectile files.</param>
/// <param name="HyperBeamFxColors">The fixed-colour cycle of the Hyper Beam's effect.</param>
/// <param name="Trails">Trail frame appearances.</param>
/// <param name="FlarePlacement">Charge-flare placement.</param>
/// <param name="FlareCompositions">Charge-flare compositions.</param>
/// <param name="Grapple">The grapple's sprite compositions, flare placement and swing frames.</param>
/// <param name="FrameBindings">Timed projectile frame sprite choices.</param>
[ArchitectureReview("context-bag", "The installed projectile presentation the game read; every member is loaded document data or its identity, not a service.")]
public sealed record InstalledProjectilePresentation(InstalledProjectileSprites Catalog, string ContentSha256,
    HyperBeamFxColours HyperBeamFxColors, InstalledProjectileTrails Trails, ChargeFlarePlacements FlarePlacement, ChargeFlareSprites FlareCompositions,
    InstalledGrapplePresentation Grapple, InstalledProjectileFrameBindings FrameBindings);
