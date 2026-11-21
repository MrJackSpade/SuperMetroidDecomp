using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The beam flare's installed visual offsets (#1596): at each muzzle in both movement modes, and standing at each turning
/// placement. No pose places a flare while turning running (#1500), so the turning placements are standing ones.
/// </summary>
public sealed class BeamFlarePlacements : ChargeFlarePlacements
{
    /// <summary>Takes the beam flare's offset at every placement, as the import pipeline validated them.</summary>
    /// <param name="supplied">The offset of each placement; it must hold every placement of the beam flare and nothing else.</param>
    /// <exception cref="ArgumentException">A placement is missing, or a supplied placement is not one the beam flare holds.</exception>
    internal BeamFlarePlacements(IReadOnlyDictionary<ChargeFlarePlacement, Offset16<RoomPixels>> supplied)
        : base(supplied, ChargeFlareSource.Beam)
    {
    }

    /// <summary>The installed visual offset of the standing flare at the turning placement of a turn holding <paramref name="heldAim"/>.</summary>
    /// <param name="heldAim">The aim the turn holds.</param>
    /// <returns>The signed whole-pixel placement offset, without camera or pose-graphics Y adjustments.</returns>
    internal Offset16<RoomPixels> WhileTurning(SamusTurningAim heldAim) =>
        offsets[new ChargeFlarePlacement(false, new SamusFlareAim.WhileTurning(heldAim))];
}
