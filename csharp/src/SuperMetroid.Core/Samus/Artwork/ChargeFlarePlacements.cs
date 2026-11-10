using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable visual muzzle offsets, as installed (#1596); never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacements
{
    /// <summary>The installed offset of every placement the flare's source holds.</summary>
    private readonly Dictionary<ChargeFlarePlacement, Offset16<RoomPixels>> offsets;

    /// <summary>Takes the offset of every placement <paramref name="source"/> holds, as the import pipeline validated them.</summary>
    /// <param name="supplied">The offset of each placement; it must hold every placement of the source and nothing else.</param>
    /// <param name="source">Whether the placements are the beam flare's or the Grapple flare's.</param>
    /// <exception cref="ArgumentException">A placement of the source is missing, or a supplied placement is not one the source holds.</exception>
    internal ChargeFlarePlacements(IReadOnlyDictionary<ChargeFlarePlacement, Offset16<RoomPixels>> supplied, ChargeFlareSource source)
    {
        Ensure.NotNull(supplied);
        offsets = [];
        foreach (ChargeFlarePlacement placement in Placements(source))
            offsets.Add(placement, supplied.TryGetValue(placement, out Offset16<RoomPixels> offset)
                ? offset
                : throw new ArgumentException($"The {source} flare has no installed offset for {placement}.", nameof(supplied)));
        if (supplied.Count != offsets.Count)
            throw new ArgumentException($"{supplied.Count - offsets.Count} supplied {source} flare offsets are not placements it holds.", nameof(supplied));
    }

    /// <summary>Resolves the installed visual muzzle offset for the requested movement mode and flare aim.</summary>
    /// <param name="running">True for running placement; false for the standing/default row.</param>
    /// <param name="aim">
    /// The muzzle the flare is placed at or, for the standing beam flare, the turning placement. No pose places a running
    /// or Grapple flare while turning, so those placements are rejected (#1500).
    /// </param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source holds no such placement.</exception>
    internal Offset16<RoomPixels> Resolve(bool running, SamusFlareAim aim) =>
        offsets.TryGetValue(new ChargeFlarePlacement(running, aim), out Offset16<RoomPixels> offset)
            ? offset
            : throw new ArgumentOutOfRangeException(nameof(aim), aim, $"No {(running ? "running" : "standing")} pose places this flare there.");

    /// <summary>
    /// Every placement <paramref name="source"/> holds: each direction's muzzle in both movement modes, and for the
    /// standing beam flare each turning placement too; Grapple never fires while turning.
    /// </summary>
    internal static IEnumerable<ChargeFlarePlacement> Placements(ChargeFlareSource source)
    {
        foreach (bool running in (bool[])[false, true])
            foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
                yield return new(running, SamusFlareAim.AtMuzzle(direction));
        if (source == ChargeFlareSource.Beam)
            foreach (SamusTurningAim turning in Enum.GetValues<SamusTurningAim>())
                yield return new(false, SamusFlareAim.WhileTurning(turning));
    }
}
