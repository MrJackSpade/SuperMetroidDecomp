using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import reads it (#633, #1465): the cartridge's definition-group starts, per-pose frame lists and
/// frame selections, from which the import resolves the half pictures each pose frame shows, with where each pose and frame
/// places the body and the dependent artwork catalogs.
/// </summary>
public sealed record SamusBodyArtworkParts
{
    /// <summary>Each upper-body definition group's first record.</summary>
    public required int[] TopSetStarts { get; init; }

    /// <summary>Each lower-body definition group's first record.</summary>
    public required int[] BottomSetStarts { get; init; }

    /// <summary>Each pose with a record's first four-byte frame selector, keyed by the pose.</summary>
    public required IReadOnlyDictionary<SamusPoseId, int> PoseFirstFrames { get; init; }

    /// <summary>
    /// Where each pose with a record places the body (#1517): its art origin and, for a pose whose frames differ from it,
    /// each animation frame's own placement.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, SamusBodyPosePlacement> Placements { get; init; }

    /// <summary>The contiguous native frame-selector interval.</summary>
    public required SamusBodyFrameSelection[] Frames { get; init; }

    /// <summary>The compositions each pose half shows at each animation frame, which the whole body pictures are composed from.</summary>
    public required SamusSpritemapSources Spritemaps { get; init; }

    /// <summary>The appearances of the atmospheric effects.</summary>
    public required SamusAtmosphericSourceArtwork Atmosphere { get; init; }

    /// <summary>Editable fatal-damage and death palettes.</summary>
    public required InstalledSamusDeathPalettes DeathPalettes { get; init; }

    /// <summary>The arm cannon's descriptors, appearances and cover tile selections, and where each pose places its cover.</summary>
    public required SamusArmCannonSourceArtwork ArmCannon { get; init; }
}
