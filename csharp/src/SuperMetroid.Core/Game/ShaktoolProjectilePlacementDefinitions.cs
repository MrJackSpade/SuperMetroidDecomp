namespace SuperMetroid.Core.Game;

/// <summary>Eight-direction placement of the dormant Shaktool circle attack.</summary>
internal static class ShaktoolProjectilePlacementDefinitions
{
    /// <summary>$86:BDE3/$BDF3 place the circle center sixteen pixels from its owning segment.</summary>
    private const int Radius = 16;

    /// <summary>
    /// Calculates the $86:BDE3 X and $86:BDF3 Y offsets from the angle's three high bits.
    /// Cardinal directions use the radius; diagonal coordinates round the radius divided
    /// by sqrt(2) outward. Y is the same component a quarter-turn earlier.
    /// </summary>
    internal static (short X, short Y) Offset(byte angle)
    {
        int octant = angle >> 5;
        return (Component(octant), Component((octant + 6) & 7));
    }

    private static short Component(int octant)
    {
        int halfTurn = octant & 3;
        int magnitude = halfTurn == 0 ? 0 : halfTurn == 2 ? Radius :
            (int)Math.Ceiling(Radius / Math.Sqrt(2));
        return (short)(octant < 4 ? magnitude : -magnitude);
    }
}
