namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Exact bounded Fireflea movement-radius rule.</summary>
internal static class FirefleaMovementDefinitions
{

    /// <summary>The movement radius, 8*(index+1), for each parameter-two high-byte selector 0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 word and pinned
    /// SetFirefleaRadius. The game masks the selected radius to eight bits, which changes none of the eight authored
    /// radii, so no mask is kept.
    /// Circular positions and wrapped vertical extrema remain caller-owned.
    /// </remarks>
    internal static readonly int[] Radii = [8, 16, 24, 32, 40, 48, 56, 64];
}
