using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The grapple's installed documents (#1636): its flare placement and its swing frames. Its sprite compositions are import
/// data (#1638, <see cref="ProjectileCompositions.Grapple"/>).
/// </summary>
/// <param name="FlarePlacement">The standing and running grapple flare offsets, which gameplay binds.</param>
/// <param name="SwingFrames">The swing-direction-to-displayed-frame mapping of Samus's swing pose, which gameplay binds.</param>
public sealed record InstalledGrapplePresentation(ChargeFlarePlacements FlarePlacement, InstalledGrappleSwingFrames SwingFrames);
