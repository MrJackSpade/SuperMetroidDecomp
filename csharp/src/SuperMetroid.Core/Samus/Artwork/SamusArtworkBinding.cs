using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The Samus artwork the host has installed, rebound when it reloads artwork (#2049). Whoever builds Samus owns one (the
/// runtime for the session, the opening cinematic for its flashbacks) and hands it to every Samus it builds, so a
/// rebinding reaches the Samus already playing. Each catalog is absent until the host installs it; a draw that needs an
/// absent one fails, naming it.
/// </summary>
public sealed class SamusArtworkBinding
{
    /// <summary>Samus's installed body placements, death steps and presentation catalogs, or none before they are installed.</summary>
    public InstalledSamusBodyArtwork? Body { get; set; }

    /// <summary>The installed arm-cannon cover placements, or none before they are installed.</summary>
    public InstalledSamusArmCannonArtwork? ArmCannon { get; set; }

    /// <summary>The installed pre-coloured sheets Samus, her particles, her death explosion and her grapple beam draw from.</summary>
    public InstalledSpriteSheets? Sheets { get; set; }

    /// <summary>The installed grapple flare placements, or none before they are installed.</summary>
    public ChargeFlarePlacements? GrappleFlarePlacement { get; set; }

    /// <summary>The installed pictures Samus's swing shows at each grapple angle, or none before they are installed.</summary>
    public InstalledGrappleSwingFrames? GrappleSwingFrames { get; set; }

    /// <summary>The installed body artwork a draw or a death step needs.</summary>
    /// <exception cref="InvalidOperationException">No body artwork is installed.</exception>
    internal InstalledSamusBodyArtwork InstalledBody =>
        Body ?? throw new InvalidOperationException("Samus requires her installed body artwork.");

    /// <summary>The installed arm-cannon placements the cover's draw needs.</summary>
    /// <exception cref="InvalidOperationException">No arm-cannon artwork is installed.</exception>
    internal InstalledSamusArmCannonArtwork InstalledArmCannon =>
        ArmCannon ?? throw new InvalidOperationException("The arm cannon requires its installed placements.");

    /// <summary>The installed sheets a draw needs.</summary>
    /// <exception cref="InvalidOperationException">No sheets are installed.</exception>
    internal InstalledSpriteSheets InstalledSheets =>
        Sheets ?? throw new InvalidOperationException("Samus's drawing requires the installed sprite sheets.");

    /// <summary>The installed grapple flare placements the beam's flare needs.</summary>
    /// <exception cref="InvalidOperationException">No grapple flare placements are installed.</exception>
    internal ChargeFlarePlacements InstalledGrappleFlarePlacement =>
        GrappleFlarePlacement ?? throw new InvalidOperationException("The grapple flare requires its installed placements.");

    /// <summary>The installed swing pictures the swing needs.</summary>
    /// <exception cref="InvalidOperationException">No swing pictures are installed.</exception>
    internal InstalledGrappleSwingFrames InstalledGrappleSwingFrames =>
        GrappleSwingFrames ?? throw new InvalidOperationException("The grapple swing requires its installed pictures.");
}
