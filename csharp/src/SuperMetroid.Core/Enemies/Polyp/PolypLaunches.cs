using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>A Polyp's launch and cooldown, chosen by random draws.</summary>
public static class PolypLaunches
{
    /// <summary>Uses the random number divided by two, modulo 8.</summary>
    public static int Cooldown(int random) => Cooldowns[CooldownSelector.Of(random)];

    /// <summary>Uses the random number divided by two, modulo 16, to select speed-curve marks 28 through 43.</summary>
    public static int InitialYMark(int random) => FirstInitialYMark + LaunchSelector.Of(random);

    /// <summary>The selected horizontal velocity.</summary>
    public static SignedEightEight XVelocity(int random) => XVelocities[LaunchSelector.Of(random)];
}
