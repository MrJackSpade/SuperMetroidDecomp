using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The Samus artwork the host has installed, rebound when it reloads artwork (#2049). Whoever builds Samus owns one (the
/// runtime for the session, the opening cinematic for its flashbacks) and hands it to every Samus it builds, so a
/// rebinding reaches the Samus already playing. It is built over installed artwork, and a rebinding replaces a catalog
/// with another installed one: the host installs its artwork before anything that builds Samus exists.
/// </summary>
[ArchitectureReview("context-bag", "The installed Samus artwork a host binds as one value; every member is an immutable artwork catalog or its required read, not a service.")]
public sealed class SamusArtworkBinding
{
    /// <summary>Binds the installed Samus artwork.</summary>
    /// <param name="body">Samus's installed body placements, death steps, arm-cannon cover and presentation catalogs.</param>
    /// <param name="sheets">The installed pre-coloured sheets Samus, her particles, her death explosion and her grapple beam draw from.</param>
    /// <param name="grappleFlarePlacement">The installed grapple flare placements.</param>
    /// <param name="grappleSwingFrames">The installed pictures Samus's swing shows at each grapple angle.</param>
    public SamusArtworkBinding(InstalledSamusBodyArtwork body, InstalledSpriteSheets sheets,
        ChargeFlarePlacements grappleFlarePlacement, InstalledGrappleSwingFrames grappleSwingFrames)
    {
        Body = body;
        ArmCannon = body.ArmCannon;
        Sheets = sheets;
        GrappleFlarePlacement = grappleFlarePlacement;
        GrappleSwingFrames = grappleSwingFrames;
    }

    /// <summary>Samus's installed body placements, death steps and presentation catalogs.</summary>
    public InstalledSamusBodyArtwork Body { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed arm-cannon cover placements: the body artwork's, bound with it.</summary>
    public InstalledSamusArmCannonArtwork ArmCannon { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed pre-coloured sheets Samus, her particles, her death explosion and her grapple beam draw from.</summary>
    public InstalledSpriteSheets Sheets { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed grapple flare placements.</summary>
    public ChargeFlarePlacements GrappleFlarePlacement { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed pictures Samus's swing shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames GrappleSwingFrames { get; set => field = Ensure.NotNull(value); }
}
