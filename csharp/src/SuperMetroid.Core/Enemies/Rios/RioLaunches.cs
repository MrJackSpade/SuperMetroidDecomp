namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Chooses a Norfair Rio's downward launch.</summary>
public static class RioLaunches
{
    /// <summary>The fast or slow downward launch <paramref name="random"/> picks.</summary>
    public static SignedEightEight NorfairYVelocity(int random) =>
        RioLaunchDefinitions.SlowLaunchBit.IsSetIn(random) ? RioLaunchDefinitions.NorfairSlowYVelocity : RioLaunchDefinitions.NorfairFastYVelocity;
}
