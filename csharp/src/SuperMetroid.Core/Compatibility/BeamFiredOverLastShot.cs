namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a beam fired with every one of Samus's shot places held goes, and the Power Bomb fuse it starts with (#2468). The
/// shot count, not the places, admits a beam (<see cref="SamusShotCount"/>), and the count can read below the place count
/// while every place is held. The uncharged, charged and Hyper Beam fires then take the last place over the shot holding
/// it: that shot ends where it stands, with nothing deleted and nothing uncounted, and the beam starts its Power Bomb
/// fuse from the count the replaced shot leaves in its place. An ignited missile leaves the speed it ignited at in 256ths
/// of a pixel; a reflected one waiting to re-ignite, the head start its reflection gave it; one not yet ignited, nothing;
/// an ignited Super Missile with a link, its whole ignition speed with twice its link's place for the fraction
/// (<see cref="FuseLeftByLinkedSuperMissile"/>); its link, nothing; a crash echo, its angle in steps; and a beam, the fuse
/// it carries. Only the Chainsaw shot burns that fuse; every other beam carries it for a beam fired over it in turn. A
/// missile and a Super Missile's link take only a free place.
/// </summary>
/// <remarks>
/// Consulted when a beam takes its place, after the shot count admits it and before Samus's pose gives its aim, and when
/// the beam's shot replaces the one there. Evidence: the three fires' place searches (<c>$90:B8AA</c>, <c>$90:B9A1</c>,
/// <c>$90:BCD8</c>) step past every place whose shot deals damage and, finding none free, step back to the last; the fires
/// write the shot's position, direction, kind, damage, animation, speed and frame behaviour but never the word each place
/// keeps for its projectile, which only a deletion and the clearing of Samus's shots zero (<c>$90:ADB7</c>,
/// <c>$90:AD22</c>); and the Chainsaw's frame behaviour runs the Power Bomb fuse on that word (<c>$90:C157</c>). What a
/// replaced shot leaves there: a missile's ignition adds 256 (<c>$90:C301</c>) to the zero of a free place or the 240 a
/// reflection writes (<c>$90:BE39</c>, <c>$90:BE58</c>), and that sum is the speed it ignites at (<c>$90:B312</c>); a Super
/// Missile's link keeps the sum's whole pixels and writes the link's index, twice its place, below them (<c>$90:BF8B</c>);
/// a crash echo's release writes its angle (<c>$90:D454</c>, <c>$90:D491</c>); a link, a beam and an explosion write
/// nothing. Of the shots' updates only the Chainsaw's reads the word as a beam's, and the Power Bomb's boundary step
/// it runs after the fuse reacts only from a fuse that had already burned out: it returns while the fuse counts above
/// zero (<c>$94:A06A</c>), and the step that burns a fuse out leaves the count with its top bit set (<c>$90:C195</c>),
/// which the boundary step clears instead of reacting. Nothing beyond the place is touched: a
/// Super Missile naming the place as its link moves and deletes whatever shot holds it (<see cref="UnlinkedSuperMissileLink"/>),
/// and a special attack's particles, which keep their own words, hold only the first four places.
/// </remarks>
[CompatibilityService("a beam fired with every shot place held takes the last place over the shot holding it and starts its Power Bomb fuse from the count that shot leaves")]
public static class BeamFiredOverLastShot
{
    /// <summary>The step between the fractions an ignited Super Missile's count holds for successive link places.</summary>
    private const int LinkPlaceStep = 2;

    /// <summary>The place a beam takes: the lowest free one, or the last when every place is held.</summary>
    /// <param name="lowestFreePlace">The lowest place no shot holds, or null when every place is held.</param>
    /// <param name="placeCount">The number of shot places, at least one.</param>
    public static int Place(int? lowestFreePlace, int placeCount) =>
        lowestFreePlace ?? Ensure.BetweenInclusive(1, int.MaxValue, placeCount) - 1;

    /// <summary>
    /// The fuse an ignited Super Missile with a link leaves in its place: the whole pixels of the speed it ignited at, in
    /// 256ths of a pixel, with twice its link's place below them.
    /// </summary>
    /// <param name="ignitionSpeed">The speed the Super Missile ignited at.</param>
    /// <param name="linkPlace">The zero-based place the Super Missile names as its link.</param>
    public static int FuseLeftByLinkedSuperMissile(SignedEightEight ignitionSpeed, int linkPlace) =>
        SignedEightEight.FromParts(ignitionSpeed.WholePixels, LinkPlaceStep * Ensure.AtLeastZero(linkPlace))
            .InTwoHundredFiftySixthsOfPixel;
}
