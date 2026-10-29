using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// What an installed Samus body holds (#1465): the half pictures each pose shows at each animation frame, where the body
/// is placed, and the catalogs it binds.
/// </summary>
public sealed record SamusBodyArtworkContent
{
    /// <summary>
    /// The half pictures each of the 253 poses shows at each animation frame the body sheets cover, or none for a frame
    /// whose pictures are not installed.
    /// </summary>
    public required IReadOnlyDictionary<SamusPoseId, IReadOnlyList<SamusBodyFramePictures?>> Frames { get; init; }

    /// <summary>Per-pose signed vertical art origins.</summary>
    public required sbyte[] GraphicsYOffsets { get; init; }

    /// <summary>Native landing vertical-offset bytes.</summary>
    public required ushort[] LandingYOffsets { get; init; }

    /// <summary>Native posture-transition vertical offsets.</summary>
    public required sbyte[] PostureYOffsets { get; init; }

    /// <summary>Native drained-state vertical offsets.</summary>
    public required sbyte[] DrainedYOffsets { get; init; }

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

    /// <summary>Identity of the body characters the pictures show, composed into the body sheets at installation.</summary>
    public required string BodyCharactersContentIdentity { get; init; }
}
