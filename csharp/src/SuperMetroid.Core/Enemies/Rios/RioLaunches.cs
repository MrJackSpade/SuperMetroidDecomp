namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Chooses a Norfair Rio's downward launch.</summary>
internal static class RioLaunches
{
    /// <summary>The fast or slow downward launch <paramref name="random"/> picks.</summary>
    internal static SignedEightEight NorfairYVelocity(int random) =>
        RioLaunchDefinitions.SlowLaunchBit.IsSetIn(random) ? RioLaunchDefinitions.NorfairSlowYVelocity : RioLaunchDefinitions.NorfairFastYVelocity;
}
