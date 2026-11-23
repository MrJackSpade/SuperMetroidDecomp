using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import reads it (#633, #1465, #2556): each pose's art origin and what each of its picture frames
/// shows, decoded once from the cartridge's tables, with the compositions and the dependent artwork catalogs.
/// </summary>
public sealed partial record SamusBodyArtworkParts
{
    /// <summary>Each pose with a record's art origin.</summary>
    public required IReadOnlyDictionary<SamusPoseId, int> ArtOrigins { get; init; }

    /// <summary>
    /// Each pose with a record's decoded picture frames, by the frame's place in the game's layout of the pose's animation:
    /// the half pictures each selects and where it places the body.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>> Frames { get; init; }

    /// <summary>The compositions the pose halves, the explosion and the atmosphere show, which the sheets are composed from.</summary>
    public required SamusSpritemapSources Spritemaps { get; init; }

    /// <summary>The appearances of the atmospheric effects.</summary>
    public required SamusAtmosphericSourceArtwork Atmosphere { get; init; }

    /// <summary>The death step each explosion frame shows and the room's whiteout colours, which gameplay reads (#1637).</summary>
    public required SuperMetroid.Core.Samus.Death.SamusDeathSteps DeathSteps { get; init; }

    /// <summary>The arm cannon's descriptors, appearances and cover tile selections, and where each pose places its cover.</summary>
    public required SamusArmCannonSourceArtwork ArmCannon { get; init; }
}
