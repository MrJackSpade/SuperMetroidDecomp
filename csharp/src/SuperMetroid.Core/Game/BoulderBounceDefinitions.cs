namespace SuperMetroid.Core.Game;

/// <summary>Native Boulder quadratic-curve indices for successive ground impacts.</summary>
public static class BoulderBounceDefinitions
{
    /// <summary>
    /// $A6:86EF, BounceSpeedTableIndices: $0000, $1000, $1800. At $A6:88EA the
    /// routine indexes from table+2 with (remainingBounces-1)*2, so the last
    /// impact reads the first zero word before the counter underflows into rolling.
    /// </summary>
    public static ushort SpeedIndex(ushort remainingBounces) => remainingBounces switch
    {
        2 => 0x1800,
        1 => 0x1000,
        0 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(remainingBounces)),
    };
}
