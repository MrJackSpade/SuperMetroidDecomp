using Engine.Shared;

namespace Game.Compatibility;

public static partial class CartridgeAngles
{
    /// <summary>
    /// How far along an angle's sine a length of <paramref name="length"/> whole pixels laid along the angle reaches,
    /// in whole pixels: the length times the sine in 256ths, rounded down, so a reach toward the axis's lesser side
    /// that falls short of a whole pixel lies a whole pixel further out.
    /// </summary>
    public static int ReachRoundedDown(int angle, int length) => SignedSine(angle) * length >> 8;

    /// <summary>
    /// The angle of the point <paramref name="across"/> pixels right and <paramref name="down"/> pixels below a place,
    /// counted in 256ths of a turn clockwise from straight up, as the cartridge's grapple beam reckons it: from the
    /// nearer of the four straight directions, turned toward the other axis by an eighth of a turn times the shorter
    /// side over the longer, what is finer dropped. No offset at all is straight up.
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
            int side = left ? 3 * QuarterTurn : QuarterTurn;
            return Turns.Wrap(side + (leanSameWay ? steps : -steps));
        }

        int reciprocal = farAcross * EighthTurn / farDown;
        int upOrDown = up ? 0 : HalfTurn;
        return Turns.Wrap(upOrDown + (leanSameWay ? -reciprocal : reciprocal));
    }
}
