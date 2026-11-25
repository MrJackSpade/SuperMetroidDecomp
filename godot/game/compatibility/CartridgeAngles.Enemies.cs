using Engine.Shared;

namespace Game.Compatibility;

public static partial class CartridgeAngles
{
    /// <summary>
    /// The angle of a point <paramref name="across"/> pixels right and <paramref name="down"/> pixels below a place, as
    /// the cartridge's grapple routine reckons it, which some enemies use too. Unlike every other angle here it counts
    /// clockwise from straight up: none up, a quarter turn right, a half turn down. From the nearer of the four straight
    /// directions it turns toward the other axis by the shorter side over the longer in eighths of a turn, rounded
    /// down. No offset at all gives none.
    /// </summary>
    public static int ClockwiseAngleFromUp(int across, int down)
    {
        bool left = across < 0, up = down < 0;
        int farAcross = Math.Abs(across), farDown = Math.Abs(down);
        if (farAcross == 0 && farDown == 0)
            return 0;

        // Mostly across, it turns from right or left clockwise when both lean the same way; mostly up or down, the
        // other way round.
        bool leanSameWay = left == up;
        if (farDown < farAcross)
        {
            int steps = ClockwiseOctantSteps(farDown, farAcross);
            return Turns.Wrap((left ? 3 * QuarterTurn : QuarterTurn) + (leanSameWay ? steps : -steps));
        }

        int reciprocalSteps = ClockwiseOctantSteps(farAcross, farDown);
        return Turns.Wrap((up ? 0 : HalfTurn) + (leanSameWay ? -reciprocalSteps : reciprocalSteps));
    }

    /// <summary>
    /// The steps, up to an eighth of a turn, that the shorter side over the longer turns from the longer side's axis,
    /// rounded down, as <see cref="ClockwiseAngleFromUp"/> takes them.
    /// </summary>
    private static int ClockwiseOctantSteps(int minor, int major) => minor * (QuarterTurn / 2) / major;
}
