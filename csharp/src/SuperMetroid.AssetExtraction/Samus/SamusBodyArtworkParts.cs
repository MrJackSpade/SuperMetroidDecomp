using SuperMetroid.Core.Samus.Artwork;
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

    /// <summary>Each pose's first four-byte frame selector.</summary>
    public required int[] PoseFirstFrames { get; init; }

    /// <summary>
    /// Where each pose with a record places the body (#1517): its art origin and, for a pose whose frames differ from it,
    /// each animation frame's own placement.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, SamusBodyPosePlacement> Placements { get; init; }

    /// <summary>The contiguous native frame-selector interval.</summary>
    public required SamusBodyFrameSelection[] Frames { get; init; }

    /// <summary>Which body picture each pose half shows at each animation frame.</summary>
    public required InstalledSamusSpritemaps Spritemaps { get; init; }

    /// <summary>The selected appearance of the atmospheric effects.</summary>
    public required InstalledSamusAtmosphericArtwork Atmosphere { get; init; }

    /// <summary>Editable fatal-damage and death palettes.</summary>
    public required InstalledSamusDeathPalettes DeathPalettes { get; init; }

    /// <summary>Identity of the selected death-explosion characters.</summary>
    public required string DeathTilesContentIdentity { get; init; }

    /// <summary>Where each pose places the arm-cannon cover.</summary>
    public required InstalledSamusArmCannonArtwork ArmCannon { get; init; }
}
