using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Immutable visual muzzle offsets, as installed (#1596), at each muzzle in both movement modes: the Grapple flare's, and
/// the beam flare's with its turning placements (<see cref="BeamFlarePlacements"/>); never used for projectile physics or
/// Grapple connection.
/// </summary>
public partial class ChargeFlarePlacements
{
    /// <summary>The projectile directions in the order the flare placements are listed (#2567).</summary>
    private static readonly IReadOnlyList<SamusProjectileDirection> DirectionOrder = Ensure.EveryMemberOnce<SamusProjectileDirection>(
    [
        SamusProjectileDirection.UpFacingRight,
        SamusProjectileDirection.UpRight,
        SamusProjectileDirection.Right,
        SamusProjectileDirection.DownRight,
        SamusProjectileDirection.DownFacingRight,
        SamusProjectileDirection.DownFacingLeft,
        SamusProjectileDirection.DownLeft,
        SamusProjectileDirection.Left,
        SamusProjectileDirection.UpLeft,
        SamusProjectileDirection.UpFacingLeft,
    ]);

    /// <summary>The turning aims in the order the flare placements are listed (#2567).</summary>
    private static readonly IReadOnlyList<SamusTurningAim> TurningAimOrder = Ensure.EveryMemberOnce<SamusTurningAim>(
    [
        SamusTurningAim.Up,
        SamusTurningAim.Neutral,
        SamusTurningAim.Down,
    ]);

    /// <summary>The installed offset of every placement the flare's source holds, each checked present as it is built.</summary>
    internal readonly Dictionary<ChargeFlarePlacement, Offset16<RoomPixels>> offsets;

    /// <summary>Takes the offset of every placement <paramref name="source"/> holds, as the import pipeline validated them.</summary>
    /// <param name="supplied">The offset of each placement; it must hold every placement of the source and nothing else.</param>
    /// <param name="source">Whether the placements are the beam flare's or the Grapple flare's.</param>
    /// <exception cref="ArgumentException">A placement of the source is missing, or a supplied placement is not one the source holds.</exception>
    private protected ChargeFlarePlacements(IReadOnlyDictionary<ChargeFlarePlacement, Offset16<RoomPixels>> supplied, ChargeFlareSource source)
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

    /// <summary>Takes the Grapple flare's offset at each muzzle in both movement modes, as the import pipeline validated them.</summary>
    /// <param name="supplied">The offset of each placement; it must hold every placement of the Grapple flare and nothing else.</param>
    /// <exception cref="ArgumentException">A placement is missing, or a supplied placement is not one the Grapple flare holds.</exception>
    internal static ChargeFlarePlacements Grapple(IReadOnlyDictionary<ChargeFlarePlacement, Offset16<RoomPixels>> supplied) =>
        new(supplied, ChargeFlareSource.Grapple);

    /// <summary>The installed visual offset of the flare at the muzzle of <paramref name="direction"/>.</summary>
    /// <param name="running">True for running placement; false for standing and every other movement.</param>
    /// <param name="direction">The aim whose muzzle the flare is placed at.</param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    internal Offset16<RoomPixels> AtMuzzle(bool running, SamusProjectileDirection direction) =>
        offsets[new ChargeFlarePlacement(running, new SamusFlareAim.AtMuzzle(direction))];

    /// <summary>
    /// Every placement <paramref name="source"/> holds: each direction's muzzle in both movement modes, and for the
    /// standing beam flare each turning placement too; Grapple never fires while turning.
    /// </summary>
    internal static IEnumerable<ChargeFlarePlacement> Placements(ChargeFlareSource source)
    {
        foreach (bool running in (bool[])[false, true])
            foreach (SamusProjectileDirection direction in DirectionOrder)
                yield return new(running, new SamusFlareAim.AtMuzzle(direction));
        if (source == ChargeFlareSource.Beam)
            foreach (SamusTurningAim turning in TurningAimOrder)
                yield return new(false, new SamusFlareAim.WhileTurning(turning));
    }
}
