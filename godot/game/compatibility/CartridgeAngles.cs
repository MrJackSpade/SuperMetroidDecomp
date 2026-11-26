using Engine.Shared;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's own sines, its aim at a point and the products it makes with them, exactly as its enemies, bosses
/// and their shots reckon them. An angle here is the cartridge's: steps of a 256th of a turn, none pointing right and
/// a quarter turn (64) pointing up. Each member is one of the cartridge's pieces of arithmetic, kept with what it
/// drops and where it counts round, since where a thing is after many updates depends on them.
/// </summary>
/// <remarks>
/// Nothing here is kept from one call to the next. An enemy uses the member its own cartridge code uses: the same
/// enemy may take a fine sine for its step and the short product for where it circles.
/// </remarks>
public static class CartridgeAngles
{
    private const int HalfTurn = 128, QuarterTurn = 64;

    /// <summary>The sine over the first half turn in 65535ths of its peak, what is finer dropped; for a step of the half turn, 0 to 127.</summary>
    public static int UnsignedHalfWave(int step) => Turns.HalfWave(step, 65535);

    /// <summary>The sine over the first half turn in 256ths of its peak, what is finer dropped, and never more than 255.</summary>
    public static int CoarseHalfWave(int step) => Math.Min(255, Turns.HalfWave(step, 256));

    /// <summary>The fine sine over the whole turn, peaking at 32767 either way: the unsigned half wave halved, and only then given its sign.</summary>
    public static int FineSignedSine(int angle)
    {
        int wrapped = Turns.Wrap(angle);
        int half = UnsignedHalfWave(wrapped % HalfTurn) / 2;
        return wrapped >= HalfTurn ? -half : half;
    }

    /// <summary>The sine over the whole turn in 256ths of its peak, exactly 256 at the quarter turn.</summary>
    public static int SignedSine(int angle)
    {
        int wrapped = Turns.Wrap(angle), step = wrapped % HalfTurn;
        int size = step == QuarterTurn ? 256 : CoarseHalfWave(step);
        return wrapped >= HalfTurn ? -size : size;
    }

    /// <summary>The fine sine's size read coarsely, 0 to 127: counted in whole steps of 256 of its own.</summary>
    public static int CoarseSizeOfFineSine(int angle) => Math.Abs(FineSignedSine(angle)) / 256;

    /// <summary>
    /// The slope an aim at a point is read from: the shorter side in 256ths of a pixel, counted within 65536, over
    /// the longer side counted within 256; a longer side that comes to nothing gives the steepest, 65535.
    /// </summary>
    public static int AimSlope(int shortSide, int longSide)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(shortSide);
        ArgumentOutOfRangeException.ThrowIfNegative(longSide);
        int rise = unchecked(shortSide * 256) % 65536, run = longSide % 256;
        return run == 0 ? 65535 : rise / run;
    }

    /// <summary>
    /// The angle of the aim from a place at a point <paramref name="across"/> pixels right and <paramref name="down"/>
    /// pixels below it: from the nearer of the four straight directions, turned toward the other axis by an eighth of
    /// the slope. Offsets of 256 pixels or more bend it, as the slope counts round.
    /// </summary>
    public static int AimAngle(int across, int down)
    {
        int farAcross = Math.Abs(across), farDown = Math.Abs(down);
        bool left = across < 0, up = down < 0;

        // Toward lessening angles from right toward down and from left toward up; from up or down the other way round.
        bool lessens = left == up;
        if (farDown < farAcross)
        {
            int turn = AimSlope(farDown, farAcross) / 8;
            return Turns.Wrap((left ? HalfTurn : 0) + (lessens ? -turn : turn));
        }

        int inverse = AimSlope(farAcross, farDown) / 8;
        return Turns.Wrap((up ? QuarterTurn : -QuarterTurn) + (lessens ? inverse : -inverse));
    }

    /// <summary>
    /// The part of a speed along an angle's sine, both in 256ths of a pixel: the speed times the sine in 256ths, what
    /// is finer than a 256th dropped. A speed below nothing is taken as itself and 65536, and the answer is counted
    /// within sixteen bits, so a backward speed gives no plain reverse. A quarter turn ahead gives the cosine's part.
    /// </summary>
    public static int SpeedAlong(int speedIn256ths, int angle)
    {
        int taken = speedIn256ths < 0 ? speedIn256ths + 65536 : speedIn256ths;
        return unchecked((short)(SignedSine(angle) * taken / 256));
    }

    /// <summary>
    /// The speed along one axis of a whole number of pixels an update heading along an angle: the unsigned half wave's
    /// share of the speed, what is finer than a 65536th dropped, and then its direction, so it rounds toward nothing.
    /// Across is the sine a quarter turn on; down is the sine a half turn on.
    /// </summary>
    public static PixelVelocity VelocityAlong(int angle, int wholePixels)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(wholePixels);
        return new PixelVelocity(FineShare(angle + QuarterTurn, wholePixels), FineShare(angle + HalfTurn, wholePixels));
    }

    /// <summary>
    /// How far along an angle's sine the point a radius out lies, by the short product: the coarse half wave times the
    /// radius counted within 256, in 256ths of a pixel; in the turn's second half its whole pixels and its part of a
    /// pixel are each reversed by themselves, so the whole pixels drop toward nothing.
    /// </summary>
    public static Pixels CircleSine(int angle, int radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);
        int wrapped = Turns.Wrap(angle);
        int reach = CoarseHalfWave(wrapped % HalfTurn) * (radius % 256);
        int whole = reach / 256, fraction = reach % 256 * 256;
        return wrapped >= HalfTurn
            ? Pixels.FromParts(unchecked((short)-whole), unchecked((ushort)-fraction))
            : Pixels.FromParts(whole, fraction);
    }

    /// <summary>The point's cosine distance by the short product: its sine distance a quarter turn ahead.</summary>
    public static Pixels CircleCosine(int angle, int radius) => CircleSine(angle + QuarterTurn, radius);

    /// <summary>
    /// The point a radius out along an angle, in whole pixels by the short product: across by the cosine, and down by
    /// the sine a half turn ahead, since up is the angle's own way.
    /// </summary>
    public static PixelOffset CircleOffset(int angle, int radius) =>
        PixelOffset.FromWhole(CircleCosine(angle, radius).Whole, CircleSine(angle + HalfTurn, radius).Whole);

    /// <summary>
    /// How far along an angle's sine a length of whole pixels laid along the angle reaches, in whole pixels: the
    /// length times the sine in 256ths, rounded down, so a reach toward the lesser side that falls short of a whole
    /// pixel lies a whole pixel further out.
    /// </summary>
    public static int ReachRoundedDown(int angle, int length) => SignedSine(angle) * length >> 8;

    /// <summary>
    /// The same reach cut toward nothing: what falls short of a whole pixel is dropped on either side. Samus's
    /// shinespark echoes and the lights of her special attacks are placed so.
    /// </summary>
    public static int ReachCutShort(int angle, int length) => SignedSine(angle) * length / 256;

    /// <summary>
    /// The angle of the point <paramref name="across"/> pixels right and <paramref name="down"/> pixels below a
    /// place as the cartridge's grapple beam reckons it, which Mother Brain's great beam and the giant Metroid use
    /// too. Unlike every other angle here it counts clockwise from straight up: none up, a quarter turn right, a half
    /// turn down. From the nearer of the four straight directions it turns toward the other axis by an eighth of a
    /// turn times the shorter side over the longer, what is finer dropped. No offset at all is straight up.
    /// </summary>
    public static int AngleClockwiseFromUp(int across, int down)
    {
        const int EighthTurn = QuarterTurn / 2;
        bool left = across < 0, up = down < 0;
        int farAcross = Math.Abs(across), farDown = Math.Abs(down);
        if (farAcross == 0 && farDown == 0)
            return 0;

        // From right or left it turns clockwise when both lean the same way; from up or down the other way round.
        bool leanSameWay = left == up;
        if (farDown < farAcross)
        {
            int steps = farDown * EighthTurn / farAcross;
            return Turns.Wrap((left ? 3 * QuarterTurn : QuarterTurn) + (leanSameWay ? steps : -steps));
        }

        int reciprocal = farAcross * EighthTurn / farDown;
        return Turns.Wrap((up ? 0 : HalfTurn) + (leanSameWay ? -reciprocal : reciprocal));
    }

    /// <summary>The share of a whole speed along an axis's angle, as <see cref="VelocityAlong"/> takes it.</summary>
    private static Pixels FineShare(int axisAngle, int wholePixels)
    {
        int wrapped = Turns.Wrap(axisAngle);
        long share = (long)UnsignedHalfWave(wrapped % HalfTurn) * wholePixels;
        long signed = wrapped >= HalfTurn ? -share : share;
        long whole = Math.DivRem(signed, Pixels.FractionsPerPixel, out long fraction);
        if (fraction < 0)
        {
            whole--;
            fraction += Pixels.FractionsPerPixel;
        }

        return Pixels.FromParts(checked((int)whole), (int)fraction);
    }
}
