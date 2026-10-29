using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Where each pose draws the arm cannon's cover (#1463). Gameplay draws the cover's pre-coloured picture from the
/// overlay sheet by aim direction and cover frame, at the pose's placement; the descriptors, appearances and tiles the
/// placements and the sheet come from are the import pipeline's.
/// </summary>
public abstract class InstalledSamusArmCannonArtwork
{
    /// <summary>Each pose's placement, in pose order.</summary>
    private readonly SamusArmCannonPlacement[] placements;

    /// <summary>Holds each pose's placement.</summary>
    /// <param name="placements">One placement per pose with a record, in pose order.</param>
    /// <exception cref="InvalidDataException">The placements do not cover every pose with a record.</exception>
    protected InstalledSamusArmCannonArtwork(IReadOnlyList<SamusArmCannonPlacement> placements)
    {
        Ensure.NotNull(placements);
        if (placements.Count != InstalledSamusBodyArtwork.PoseCount || placements.Any(placement => placement is null))
            throw new InvalidDataException("Arm-cannon placements must cover every pose.");
        this.placements = [.. placements];
    }

    /// <summary>SHA-256 of selected cannon placement, cover appearances, tile selectors and characters.</summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Where <paramref name="pose"/> draws the cover.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    public SamusArmCannonPlacement Placement(SamusPoseId pose) => SamusPoseNumbering.HasRecord(pose)
        ? placements[SamusPoseNumbering.Number(pose)]
        : throw new ArgumentOutOfRangeException(nameof(pose));
}
