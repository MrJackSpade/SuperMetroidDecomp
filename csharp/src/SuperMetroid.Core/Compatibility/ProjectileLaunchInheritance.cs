namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The motion a projectile inherits from Samus when it launches (#1521). Each of Samus's movement updates records the
/// distance it moved in one direction, overwriting only that direction; the next launch adds an inherited velocity to
/// the shot's own speed. The inherited velocity does not come from the net displacement: its whole pixels are the last
/// recorded distance in the shot's direction, but its fraction is the top part of the fraction recorded before that
/// direction, in the order camera vertical subspeed, leftward, rightward, upward, downward. A leftward shot therefore
/// inherits part of the camera's vertical subspeed, and a rightward shot part of the last leftward movement.
/// </summary>
/// <remarks>
/// <para>
/// Recorded by Samus's live horizontal and vertical movement, and by the camera's vertical tracking at the end of its
/// update; read when a beam, missile or Super Missile sets its launch velocity; cleared of its four distances, keeping
/// the camera subspeed, by the projectile update that precedes the next movement. Lifetime: Samus's.
/// </para>
/// <para>
/// Rules. The inherited velocity's whole pixels are the distance's whole pixels within a cycle of 256 pixels, read as
/// -128 through 127, and its fraction is the preceding fraction's 256ths of a pixel. An upward shot inherits a quarter
/// of its upward velocity, and nothing while the upward distance has no whole pixel in its cycle; that quarter is always
/// taken rising, so an upward distance whose whole pixels read as downward gives a quarter a full quarter-range further
/// up.
/// </para>
/// <para>
/// Evidence: the original launch reads each direction's velocity from storage that runs one place into the preceding
/// field; the projectile-inheritance probe checks all fifty native snapshot cases, live movement writes and a runtime
/// shot fired after controller-driven movement.
/// </para>
/// </remarks>
[CompatibilityService("a projectile's launch velocity inherits Samus's last movement in its direction, with its fraction taken from the movement recorded before that direction")]
public sealed class ProjectileLaunchInheritance
{
    /// <summary>The steps of a fraction of a pixel in a recorded distance or the camera subspeed.</summary>
    private const int FractionSteps = 65536;

    /// <summary>The steps of a fraction of a pixel in an inherited velocity.</summary>
    private const int VelocityFractionSteps = 256;

    /// <summary>The whole pixels an inherited velocity holds before they continue from zero.</summary>
    private const int VelocityWholeCycle = 256;

    /// <summary>The first whole-pixel place in the velocity cycle that reads as negative.</summary>
    private const int FirstNegativeVelocityWhole = VelocityWholeCycle / 2;

    /// <summary>A quarter of the inherited velocity's range, 64 pixels an update: the rising quarter's shift.</summary>
    private static readonly SignedEightEight QuarterVelocityRange = SignedEightEight.FromParts(VelocityWholeCycle / 4, 0);

    /// <summary>The share of the inherited velocity an upward shot adds.</summary>
    private const int RisingShareDivisor = 4;

    /// <summary>The four recorded distances: leftward, rightward, upward, downward.</summary>
    private readonly SignedSixteenSixteen[] _distances = new SignedSixteenSixteen[4];

    /// <summary>
    /// The camera's vertical subspeed, the fraction of its vertical speed in 65536ths of a pixel, 0 through 65535. The
    /// camera records it at the end of its update; a leftward launch inherits its top part as its fraction.
    /// </summary>
    public int CameraYSubspeed
    {
        get;
        set => field = Ensure.BetweenInclusive(0, FractionSteps - 1, value);
    }

    /// <summary>Records one horizontal movement update's signed distance, replacing the last one in <paramref name="direction"/>.</summary>
    public void Record(HorizontalDirection direction, SignedSixteenSixteen distance) => _distances[Place(direction)] = distance;

    /// <summary>Records one vertical movement update's signed distance, replacing the last one in <paramref name="direction"/>.</summary>
    public void Record(VerticalDirection direction, SignedSixteenSixteen distance) => _distances[Place(direction)] = distance;

    /// <summary>The last distance recorded in <paramref name="direction"/>; zero once cleared.</summary>
    public SignedSixteenSixteen Distance(VerticalDirection direction) => _distances[Place(direction)];

    /// <summary>Clears the four recorded distances, keeping the camera subspeed.</summary>
    public void ClearMovement() => Array.Clear(_distances);

    /// <summary>The velocity a shot launched in <paramref name="direction"/> inherits, in 8.8 pixels per update.</summary>
    public SignedEightEight InheritedVelocity(HorizontalDirection direction) => InheritedVelocity(Place(direction));

    /// <summary>The velocity a shot launched in <paramref name="direction"/> inherits, in 8.8 pixels per update.</summary>
    public SignedEightEight InheritedVelocity(VerticalDirection direction) => InheritedVelocity(Place(direction));

    /// <summary>
    /// The share of the upward inherited velocity an upward shot adds: nothing while the upward distance has no whole
    /// pixel in its cycle, otherwise a quarter of the inherited velocity, rounded down and always taken rising, so a
    /// quarter of a downward-reading velocity is moved a quarter-range further up. A quarter lies within a quarter-range
    /// either way, so moving one up stays inside the velocity's range.
    /// </summary>
    public SignedEightEight RisingLaunchVelocity()
    {
        if (WholePixelsInCycle(Distance(VerticalDirection.Up)) == 0)
            return SignedEightEight.Zero;
        SignedEightEight inherited = InheritedVelocity(VerticalDirection.Up);
        SignedEightEight quarter = inherited.DividedRoundingDown(RisingShareDivisor);
        return inherited.IsNegative ? quarter : quarter - QuarterVelocityRange;
    }

    /// <summary><see cref="RisingLaunchVelocity"/> in 256ths of a pixel an update, for the launch that still adds raw speeds.</summary>
    public int RisingLaunchShare() => RisingLaunchVelocity().SignedRawValue;

    /// <summary>
    /// The inherited velocity of the distance at <paramref name="place"/>: its whole pixels within their 256-pixel cycle,
    /// and the 256ths of the fraction recorded before it, the camera subspeed before the first distance.
    /// </summary>
    private SignedEightEight InheritedVelocity(int place)
    {
        int whole = WholePixelsInCycle(_distances[place]);
        int precedingFraction = place == 0 ? CameraYSubspeed : _distances[place - 1].Fraction;
        int fraction = precedingFraction / (FractionSteps / VelocityFractionSteps);
        return SignedEightEight.FromParts(whole, fraction);
    }

    /// <summary>The whole pixels of <paramref name="distance"/> within the velocity's 256-pixel cycle, -128 through 127.</summary>
    private static int WholePixelsInCycle(SignedSixteenSixteen distance)
    {
        int place = (distance.WholePixels % VelocityWholeCycle + VelocityWholeCycle) % VelocityWholeCycle;
        return place >= FirstNegativeVelocityWhole ? place - VelocityWholeCycle : place;
    }

    /// <summary>The place of a horizontal direction's distance: leftward first, then rightward.</summary>
    private static int Place(HorizontalDirection direction) => direction switch
    {
        HorizontalDirection.Left => 0,
        HorizontalDirection.Right => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined horizontal direction."),
    };

    /// <summary>The place of a vertical direction's distance after the two horizontal ones: upward, then downward.</summary>
    private static int Place(VerticalDirection direction) => direction switch
    {
        VerticalDirection.Up => 2,
        VerticalDirection.Down => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined vertical direction."),
    };
}
