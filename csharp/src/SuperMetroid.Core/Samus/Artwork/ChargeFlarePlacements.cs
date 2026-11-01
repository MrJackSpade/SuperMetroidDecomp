namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable visual muzzle offsets; never used for projectile physics or Grapple connection.</summary>
public sealed class ChargeFlarePlacements
{
    /// <summary>Import-selected source kind; placement values themselves cannot distinguish Grapple from beam art.</summary>
    private readonly ChargeFlareSource source;
    /// <summary>Only authored offsets differing from the selected stock source definitions, by <see cref="Slot"/>.</summary>
    internal readonly Dictionary<int, Offset16<RoomPixels>> offsets = new();
    /// <summary>Copies the changed visual offsets the import pipeline validated, keeping the beam or Grapple stock offsets for the rest.</summary>
    internal ChargeFlarePlacements(IReadOnlyDictionary<int, Offset16<RoomPixels>> supplied, ChargeFlareSource source)
    {
        this.source = source;
        foreach ((int slot, Offset16<RoomPixels> offset) in supplied)
            if (offset != CalculatedOffset(slot >= ChargeFlarePlacementDefinitions.StandingDirectionCount,
                slot % ChargeFlarePlacementDefinitions.StandingDirectionCount)) offsets.Add(slot, offset);
    }
    /// <summary>Resolves an independently editable visual muzzle offset for the requested movement mode and direction, using the beam or Grapple source contract selected at import.</summary>
    /// <param name="running">True for running placement; false for the standing/default row.</param>
    /// <param name="direction">
    /// A named aiming direction, 0..9, or for the standing beam flare a turning placement, 10..12. No pose selects a row
    /// past them, so other selections are rejected (#1500).
    /// </param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    public Offset16<RoomPixels> Resolve(bool running, int direction)
    {
        if ((uint)direction >= DirectionsFor(source, running)) throw new ArgumentOutOfRangeException(nameof(direction));
        return offsets.TryGetValue(Slot(running, direction), out var supplied)
            ? supplied : CalculatedOffset(running, direction);
    }
    /// <summary>Gets the fallback from the beam or Grapple definitions chosen at import.</summary>
    private Offset16<RoomPixels> CalculatedOffset(bool running, int direction) => source == ChargeFlareSource.Grapple
        ? ChargeFlarePlacementOffsets.GrappleOffset(running, direction)
        : ChargeFlarePlacementOffsets.BeamOffset(running, direction);

    /// <summary>
    /// How many directions a movement mode places the flare for: the ten named aiming directions, and for the standing
    /// beam flare its turning placements too; Grapple never fires while turning.
    /// </summary>
    internal static int DirectionsFor(ChargeFlareSource source, bool running) => source == ChargeFlareSource.Grapple || running
        ? ChargeFlarePlacementDefinitions.DirectionCount
        : ChargeFlarePlacementDefinitions.StandingDirectionCount;

    /// <summary>Where a movement mode's direction is kept: the standing directions first, then the running ones.</summary>
    internal static int Slot(bool running, int direction) =>
        (running ? ChargeFlarePlacementDefinitions.StandingDirectionCount : 0) + direction;
}
