using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body as the import reads it (#633, #1465): the cartridge's definition-group starts, per-pose frame lists and
/// frame selections, from which the import resolves the half pictures each pose frame shows, with the offsets and
/// dependent artwork catalogs.
/// </summary>
public sealed record SamusBodyArtworkParts
{
    /// <summary>Each upper-body definition group's first record.</summary>
    public required int[] TopSetStarts { get; init; }

    /// <summary>Each lower-body definition group's first record.</summary>
    public required int[] BottomSetStarts { get; init; }

    /// <summary>Each pose's first four-byte frame selector.</summary>
    public required int[] PoseFirstFrames { get; init; }

    /// <summary>Per-pose signed vertical art origins.</summary>
    public required sbyte[] GraphicsYOffsets { get; init; }

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

    /// <summary>Native landing vertical-offset bytes.</summary>
    public required ushort[] LandingYOffsets { get; init; }

    /// <summary>Native posture-transition vertical offsets.</summary>
    public required sbyte[] PostureYOffsets { get; init; }

    /// <summary>Native drained-state vertical offsets.</summary>
    public required sbyte[] DrainedYOffsets { get; init; }
}
