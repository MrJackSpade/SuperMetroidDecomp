using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// What an installed Samus body holds (#1465, #1517): each pose's installed animation frames with the pictures they show
/// and where they place the body, each pose's art origin, and the catalogs it binds.
/// </summary>
public sealed record SamusBodyArtworkContent
{
    /// <summary>
    /// The installed animation frames of each of the 253 poses, from the first, each with the half pictures it shows and its
    /// body placement, or none for a frame that is not installed.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> Frames { get; init; }

    /// <summary>
    /// Each pose's art origin: how many pixels above Samus's center the arm cannon, the Grapple beam, the charge flare and
    /// the speed-booster echoes are drawn, for the same poses as <see cref="Frames"/>.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, int> GraphicsYOffsets { get; init; }

    /// <summary>Which body picture each pose half shows at each animation frame.</summary>
    public required InstalledSamusSpritemaps Spritemaps { get; init; }

    /// <summary>The selected appearance of the atmospheric effects.</summary>
    public required InstalledSamusAtmosphericArtwork Atmosphere { get; init; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public required SamusDeathSteps DeathSteps { get; init; }

    /// <summary>Where each pose places the arm-cannon cover.</summary>
    public required InstalledSamusArmCannonArtwork ArmCannon { get; init; }
}
