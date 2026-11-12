using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The grapple's installed documents (#1636): its sprite compositions, its flare placement and its swing frames.</summary>
/// <param name="Sprites">The grapple endpoint and beam sprite compositions, which the grapple sheet is composed from.</param>
/// <param name="FlarePlacement">The standing and running grapple flare offsets, which gameplay binds.</param>
/// <param name="SwingFrames">The angle-to-displayed-frame mapping of Samus's swing pose, which gameplay binds.</param>
public sealed record InstalledGrapplePresentation(InstalledGrappleSprites Sprites, ChargeFlarePlacements FlarePlacement, InstalledGrappleSwingFrames SwingFrames);
