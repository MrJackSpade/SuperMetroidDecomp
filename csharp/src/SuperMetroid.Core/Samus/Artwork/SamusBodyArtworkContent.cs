using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// What an installed Samus body holds (#1465, #1517, #1585): where each pose's installed animation frames place the body,
/// each pose's art origin, and the catalogs it binds.
/// </summary>
public sealed record SamusBodyArtworkContent
{
    /// <summary>
    /// The installed animation frames of each of the 253 poses, from the first: how many pixels above Samus's drawn point
    /// each frame's body origin sits, negative below it, or none for a frame that is not installed.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, IReadOnlyList<int?>> OriginRises { get; init; }

    /// <summary>
    /// Each pose's art origin: how many pixels above Samus's center the arm cannon, the Grapple beam, the charge flare and
    /// the speed-booster echoes are drawn, for the same poses as <see cref="OriginRises"/>.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, int> GraphicsYOffsets { get; init; }

    /// <summary>The selected appearance of the atmospheric effects.</summary>
    public required InstalledSamusAtmosphericArtwork Atmosphere { get; init; }

    /// <summary>The order her death sequence steps through and the room's whiteout colours (#1512).</summary>
    public required SamusDeathSteps DeathSteps { get; init; }

    /// <summary>Where each pose places the arm-cannon cover.</summary>
    public required InstalledSamusArmCannonArtwork ArmCannon { get; init; }
}
