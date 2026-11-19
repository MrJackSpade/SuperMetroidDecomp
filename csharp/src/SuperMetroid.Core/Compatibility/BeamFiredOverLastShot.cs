namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a beam fired with every one of Samus's shot places held goes, and the Power Bomb fuse it starts with (#2468). The
/// shot count, not the places, admits a beam (<see cref="SamusShotCount"/>), and the count can read below the place count
/// while every place is held. The uncharged, charged and Hyper Beam fires then take the last place over the shot holding
/// it: that shot ends where it stands, with nothing deleted and nothing uncounted, and the beam starts its Power Bomb
/// fuse from the count the replaced shot leaves in its place (<see cref="FuseLeftBy"/>, from the facts of that shot
/// <see cref="ReplacedShot"/> describes). An ignited missile leaves the speed it ignited at in 256ths of a pixel; a
/// reflected one waiting to re-ignite, the head start its reflection gave it; one not yet ignited, nothing; an ignited
/// Super Missile with a link, its whole ignition speed with twice its link's place for the fraction; its link, nothing; a
/// crash echo, its angle in steps; and a beam, the fuse it carries. Only the Chainsaw shot burns that fuse; every other
/// beam carries it for a beam fired over it in turn. A missile and a Super Missile's link take only a free place.
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
    /// <summary>
    /// The step between the fractions an ignited Super Missile's count holds for successive link places: a 128th of a pixel
    /// an update.
    /// </summary>
    private static readonly SignedEightEight LinkPlaceStep = SignedEightEight.FromPixelFraction(1, 128);

    /// <summary>
    /// The count an ignited Super Missile naming its link leaves: its ignition speed's whole pixels, with the link's place,
    /// a step a place, as their fraction.
    /// </summary>
    private static SignedEightEight LinkedSuperMissileCount(SignedEightEight ignitionSpeed, int linkPlace) =>
        SignedEightEight.FromPixels(ignitionSpeed.WholePixels) + LinkPlaceStep * Ensure.AtLeastZero(linkPlace);

    /// <summary>The place a beam takes: the lowest free one, or the last when every place is held.</summary>
    /// <param name="lowestFreePlace">The lowest place no shot holds, or null when every place is held.</param>
    /// <param name="placeCount">The number of shot places, at least one.</param>
    public static int Place(int? lowestFreePlace, int placeCount) =>
        lowestFreePlace ?? Ensure.BetweenInclusive(1, int.MaxValue, placeCount) - 1;

    /// <summary>
    /// The Power Bomb fuse, in updates, a beam fired into a place starts with (#2524): the count <paramref name="replaced"/>
    /// leaves there. A free place leaves nothing, and so does a missile not yet ignited, a Super Missile's link among them;
    /// a reflected missile waiting to re-ignite leaves its head start, and an ignited one the speed it ignited at, both in
    /// 256ths of a pixel, or for a Super Missile naming a link that speed's whole pixels with twice the link's place below
    /// them; a crash echo leaves its departure angle in steps, and a beam or an explosion the fuse it carries.
    /// </summary>
    /// <param name="replaced">What the beam finds in the place it takes.</param>
    /// <exception cref="InvalidOperationException">
    /// The place holds a special attack's particle: a beam takes a held place only when every place is held, and then the
    /// last, which no particle holds.
    /// </exception>
    public static int FuseLeftBy(ReplacedShot replaced) => Ensure.NotNull(replaced) switch
    {
        ReplacedShot.FreePlace or ReplacedShot.UnignitedMissile => 0,
        ReplacedShot.ReflectedMissile reflected => reflected.HeadStart.InTwoHundredFiftySixthsOfPixel,
        ReplacedShot.IgnitedMissile { LinkPlace: { } linkPlace } superMissile =>
            LinkedSuperMissileCount(superMissile.IgnitionSpeed, linkPlace).InTwoHundredFiftySixthsOfPixel,
        ReplacedShot.IgnitedMissile missile => missile.IgnitionSpeed.InTwoHundredFiftySixthsOfPixel,
        ReplacedShot.CrashEcho echo => echo.DepartureAngle.RawValue,
        ReplacedShot.Beam beam => beam.CarriedFuse,
        ReplacedShot.SpecialAttackParticle => throw new InvalidOperationException(
            "A beam fired with every place held takes the last place, which no special attack particle holds, yet a particle was replaced."),
        _ => throw new ArgumentOutOfRangeException(nameof(replaced), replaced, $"Undefined {nameof(ReplacedShot)}."),
    };

    /// <summary>
    /// What a beam fired into one of Samus's shot places finds there (#2524), which <see cref="FuseLeftBy"/> turns
    /// into the Power Bomb fuse the beam starts with: a free place, or the kind of shot holding it with the facts of that
    /// shot the count it leaves depends on.
    /// </summary>
    public abstract record ReplacedShot
    {
        /// <summary>Closes the hierarchy to the holdings declared here.</summary>
        private ReplacedShot()
        {
        }

        /// <summary>A place no shot holds.</summary>
        public sealed record FreePlace : ReplacedShot;

        /// <summary>
        /// A missile or Super Missile fired and not yet ignited, or a Super Missile's invisible link, which never ignites.
        /// </summary>
        public sealed record UnignitedMissile : ReplacedShot;

        /// <summary>A missile or Super Missile an enemy reflected, waiting to re-ignite.</summary>
        /// <param name="HeadStart">The head start its reflection gave it toward the speed it re-ignites at.</param>
        public sealed record ReflectedMissile(SignedEightEight HeadStart) : ReplacedShot;

        /// <summary>An ignited missile or Super Missile, or the explosion of one.</summary>
        public sealed record IgnitedMissile : ReplacedShot
        {
            /// <summary>An ignited missile that ignited at <paramref name="ignitionSpeed"/>, naming <paramref name="linkPlace"/>.</summary>
            /// <param name="ignitionSpeed">The speed it ignited at.</param>
            /// <param name="linkPlace">The place an ignited Super Missile names as its link, or null for a missile or a Super Missile naming none.</param>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="linkPlace"/> is negative.</exception>
            public IgnitedMissile(SignedEightEight ignitionSpeed, int? linkPlace)
            {
                IgnitionSpeed = ignitionSpeed;
                LinkPlace = linkPlace is { } place ? Ensure.AtLeastZero(place) : null;
            }

            /// <summary>The speed it ignited at.</summary>
            public SignedEightEight IgnitionSpeed { get; }

            /// <summary>The place an ignited Super Missile names as its link, or null for a missile or a Super Missile naming none.</summary>
            public int? LinkPlace { get; }
        }

        /// <summary>A shinespark crash echo flying out from Samus.</summary>
        /// <param name="DepartureAngle">The angle it was released at.</param>
        public sealed record CrashEcho(TrigonometryAngle DepartureAngle) : ReplacedShot;

        /// <summary>A beam or the Hyper Beam's shot, or the explosion of either.</summary>
        public sealed record Beam : ReplacedShot
        {
            /// <summary>A beam carrying <paramref name="carriedFuse"/>.</summary>
            /// <param name="carriedFuse">The Power Bomb fuse it carries, in updates.</param>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="carriedFuse"/> is negative.</exception>
            public Beam(int carriedFuse) => CarriedFuse = Ensure.AtLeastZero(carriedFuse);

            /// <summary>The Power Bomb fuse it carries, in updates.</summary>
            public int CarriedFuse { get; }
        }

        /// <summary>One of a special attack's particles.</summary>
        public sealed record SpecialAttackParticle : ReplacedShot;
    }
}
