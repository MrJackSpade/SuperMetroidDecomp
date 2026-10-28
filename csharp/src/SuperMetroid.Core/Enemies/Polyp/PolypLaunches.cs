using static SuperMetroid.Core.Enemies.Polyp.PolypLaunchDefinitions;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>A Polyp's launch and cooldown, chosen by native random draws.</summary>
public static class PolypLaunches
{
    /// <summary>Uses RNG bits 1..3; bit zero and the upper bits do not select a record.</summary>
    public static ushort Cooldown(int random) => Cooldowns[CooldownSelector.Of(random)];

    /// <summary>Uses RNG bits 1..4 to select quadratic speed indices 28 through 43.</summary>
    public static ushort InitialYIndex(int random) => (ushort)(FirstInitialYIndex + LaunchSelector.Of(random));

    /// <summary>Preserves the native sign grouping and sixteen-bit representation.</summary>
    public static ushort XVelocity(int random) => unchecked((ushort)XVelocities[LaunchSelector.Of(random)]);
}
