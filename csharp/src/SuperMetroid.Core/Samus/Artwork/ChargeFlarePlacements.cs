using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacements
{
    /// <summary>Import-selected source kind; placement values themselves cannot distinguish Grapple from beam art.</summary>
    private readonly ChargeFlareSource source;
    /// <summary>Only authored offsets differing from the selected stock source definitions, by placement.</summary>
    internal readonly Dictionary<ChargeFlarePlacement, Offset16<RoomPixels>> offsets = new();
    /// <summary>Copies the changed visual offsets the import pipeline validated, keeping the beam or Grapple stock offsets for the rest.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A supplied placement is not one the source holds.</exception>
    internal ChargeFlarePlacements(IReadOnlyDictionary<ChargeFlarePlacement, Offset16<RoomPixels>> supplied, ChargeFlareSource source)
    {
        this.source = source;
        foreach ((ChargeFlarePlacement placement, Offset16<RoomPixels> offset) in supplied)
            if (offset != CalculatedOffset(placement)) offsets.Add(placement, offset);
    }
    /// <summary>Resolves an independently editable visual muzzle offset for the requested movement mode and flare aim, using the beam or Grapple source contract selected at import.</summary>
    /// <param name="running">True for running placement; false for the standing/default row.</param>
    /// <param name="aim">
    /// The muzzle the flare is placed at or, for the standing beam flare, the turning placement. No pose places a running
    /// or Grapple flare while turning, so those placements are rejected (#1500).
    /// </param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The source holds no such placement.</exception>
    internal Offset16<RoomPixels> Resolve(bool running, SamusFlareAim aim)
    {
        var placement = new ChargeFlarePlacement(running, aim);
        Offset16<RoomPixels> calculated = CalculatedOffset(placement);
        return offsets.TryGetValue(placement, out var supplied) ? supplied : calculated;
    }
    /// <summary>Gets the fallback from the beam or Grapple definitions chosen at import.</summary>
    private Offset16<RoomPixels> CalculatedOffset(ChargeFlarePlacement placement) => source == ChargeFlareSource.Grapple
        ? ChargeFlarePlacementOffsets.GrappleOffset(placement.Running, placement.Aim)
        : ChargeFlarePlacementOffsets.BeamOffset(placement.Running, placement.Aim);

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
