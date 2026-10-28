namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Chooses a Norfair Rio's downward launch.</summary>
public static class RioLaunches
{
    /// <summary>$A2:C1C1: RNG bit two selects one of two adjacent downward velocities.</summary>
    public static ushort NorfairYVelocity(int random) =>
        (random & 4) == 0 ? RioLaunchDefinitions.NorfairFastYVelocity : RioLaunchDefinitions.NorfairSlowYVelocity;
}
