using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Where each pose draws the arm cannon's cover (#1463, #1616). Gameplay draws the cover's pre-coloured picture from the
/// overlay sheet by aim direction and cover frame, at the pose's placement; the descriptors, appearances and source artwork the
/// placements and the sheet come from are the import pipeline's.
/// </summary>
public sealed class InstalledSamusArmCannonArtwork
{
    /// <summary>Each pose's placement.</summary>
    private readonly Dictionary<SamusPoseId, SamusArmCannonPlacement> placements;

    /// <summary>Holds each pose's placement.</summary>
    /// <param name="placements">The placement of every pose the import's tables describe.</param>
    /// <exception cref="InvalidDataException">A pose has no placement.</exception>
    internal InstalledSamusArmCannonArtwork(IReadOnlyDictionary<SamusPoseId, SamusArmCannonPlacement> placements)
    {
        Ensure.NotNull(placements);
        if (placements.Values.Any(placement => placement is null))
            throw new InvalidDataException("Arm-cannon placements name a pose without a placement.");
        this.placements = placements.ToDictionary();
    }

    /// <summary>Where <paramref name="pose"/> draws the cover.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no placement.</exception>
    public SamusArmCannonPlacement Placement(SamusPoseId pose) => placements.TryGetValue(pose, out SamusArmCannonPlacement? placement)
        ? placement
        : throw new ArgumentOutOfRangeException(nameof(pose));
}
