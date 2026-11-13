using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>A Polyp's launch and cooldown, chosen by random draws.</summary>
public static class PolypLaunches
{
    /// <summary>Uses RNG bits 1..3; bit zero and the upper bits do not select a record.</summary>
    public static int Cooldown(int random) => Cooldowns[CooldownSelector.Of(random)];

    /// <summary>Uses RNG bits 1..4 to select quadratic speed indices 28 through 43.</summary>
    public static int InitialYIndex(int random) => FirstInitialYIndex + LaunchSelector.Of(random);

    /// <summary>The selected horizontal velocity.</summary>
    public static SignedEightEight XVelocity(int random) => XVelocities[LaunchSelector.Of(random)];
}
