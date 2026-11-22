using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The Samus artwork the host has installed, rebound when it reloads artwork (#2049). Whoever builds Samus owns one (the
/// runtime for the session, the opening cinematic for its flashbacks) and hands it to every Samus it builds, so a
/// rebinding reaches the Samus already playing. It is built over installed artwork, and a rebinding replaces a catalog
/// with another installed one: the host installs its artwork before anything that builds Samus exists.
/// </summary>
[ArchitectureReview("context-bag", "The installed Samus artwork every Samus, shot and bomb system of a session draws from, rebound in place when the host binds new content; every member is an immutable artwork catalog, not a service.")]
public sealed partial class SamusArtworkBinding
{
    /// <summary>Binds the installed Samus artwork.</summary>
    /// <param name="body">Samus's installed body placements, death steps, arm-cannon cover and presentation catalogs.</param>
    /// <param name="shownFrames">The installed frames her body can show in each pose and the picture frame each draws.</param>
    /// <param name="sheets">The installed pre-coloured sheets Samus, her particles, her death explosion and her grapple beam draw from.</param>
    /// <param name="grappleFlarePlacement">The installed grapple flare placements.</param>
    /// <param name="grappleSwingFrames">The installed pictures Samus's swing shows at each grapple angle.</param>
    /// <param name="projectileFrames">The installed composition each frame of her shots' and bombs' animations draws.</param>
    /// <param name="powerBombGlowColors">The installed colours her Power Bomb and Crystal Flash blasts glow in.</param>
    public SamusArtworkBinding(InstalledSamusBodyArtwork body, SamusShownFrames shownFrames, InstalledSpriteSheets sheets,
        ChargeFlarePlacements grappleFlarePlacement, InstalledGrappleSwingFrames grappleSwingFrames,
        InstalledProjectileFrameBindings projectileFrames, InstalledPowerBombGlowColors powerBombGlowColors)
    {
        PowerBombGlowColors = powerBombGlowColors;
        Body = body;
        ArmCannon = body.ArmCannon;
        ShownFrames = shownFrames;
        Sheets = sheets;
        GrappleFlarePlacement = grappleFlarePlacement;
        GrappleSwingFrames = grappleSwingFrames;
        ProjectileFrames = projectileFrames;
    }

    /// <summary>Samus's installed body placements, death steps and presentation catalogs.</summary>
    public InstalledSamusBodyArtwork Body { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed frames her body can show in each pose and the picture frame each draws.</summary>
    public SamusShownFrames ShownFrames { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed arm-cannon cover placements: the body artwork's, bound with it.</summary>
    public InstalledSamusArmCannonArtwork ArmCannon { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed pre-coloured sheets Samus, her particles, her death explosion and her grapple beam draw from.</summary>
    public InstalledSpriteSheets Sheets { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed grapple flare placements.</summary>
    public ChargeFlarePlacements GrappleFlarePlacement { get; set => field = Ensure.NotNull(value); }

    /// <summary>The installed pictures Samus's swing shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames GrappleSwingFrames { get; set => field = Ensure.NotNull(value); }

    /// <summary>
    /// The installed composition each frame of her shots' and bombs' animations draws, which every projectile and bomb
    /// system built with this binding resolves its frames through.
    /// </summary>
    public InstalledProjectileFrameBindings ProjectileFrames { get; set => field = Ensure.NotNull(value); }

    /// <summary>
    /// The installed colours her Power Bomb and Crystal Flash blasts glow in, which every bomb system built with this binding
    /// shows its blast in (#2397).
    /// </summary>
    public InstalledPowerBombGlowColors PowerBombGlowColors { get; set => field = Ensure.NotNull(value); }
}
