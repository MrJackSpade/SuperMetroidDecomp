namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>Fixed launch/cooldown mechanics selected by three independent native RNG draws.</summary>
public static class PolypLaunchDefinitions
{
    /// <summary>The RNG bits 1..3 that select the cooldown; bit zero and the upper bits do not select a record.</summary>
    internal const int CooldownSelectorMask = 0x0e;

    /// <summary>The RNG bits 1..4 that select the launch speed and the horizontal velocity.</summary>
    internal const int LaunchSelectorMask = 0x1e;

    /// <summary>The cooldown each RNG selector chooses: sixteen frames, eight longer per selector.</summary>
    internal static readonly ushort[] Cooldowns = [16, 24, 32, 40, 48, 56, 64, 72];

    /// <summary>The quadratic speed index the first launch selector chooses; each following selector chooses the next.</summary>
    internal const int FirstInitialYIndex = 28;

    /// <summary>
    /// The horizontal velocity each launch selector chooses: the first eight push right, the second eight push left by
    /// the same magnitudes, each $10 faster than the last from $60.
    /// </summary>
    internal static readonly short[] XVelocities =
    [
        96, 112, 128, 144, 160, 176, 192, 208,
        -96, -112, -128, -144, -160, -176, -192, -208,
    ];
}
