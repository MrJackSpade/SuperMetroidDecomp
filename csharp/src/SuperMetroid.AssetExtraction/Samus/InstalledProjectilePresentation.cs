using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed projectile presentation as the game reads it (#1636): its documents, loaded, and the identity of the
/// installed files they were loaded from. The beam, trail and grapple characters, the beam colours (#1637), the Hyper Beam's
/// colour cycle (#1693) and the projectile, flare, trail and grapple compositions, whose parts are the original's packed
/// sprite placements (#1638), are import data the installation never holds (<see cref="ImportProjectileArtwork"/>); the game
/// draws the sheets composed from them.
/// </summary>
/// <param name="ContentSha256">Identity of the installed projectile files.</param>
/// <param name="FlarePlacement">Charge-flare placement.</param>
/// <param name="Grapple">The grapple's flare placement and swing frames.</param>
/// <param name="FrameBindings">Timed projectile frame sprite choices.</param>
public sealed partial record InstalledProjectilePresentation(string ContentSha256,
    BeamFlarePlacements FlarePlacement, InstalledGrapplePresentation Grapple, InstalledProjectileFrameBindings FrameBindings);
