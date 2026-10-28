namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Exact bounded Fireflea movement-radius rule.</summary>
internal static class FirefleaMovementDefinitions
{

    /// <summary>The movement radius, 8*(index+1), for each parameter-two high-byte selector 0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 word and pinned
    /// bank_A3.asm SetFirefleaRadius. Native doubles the selector for word indexing
    /// and masks the result to a byte; all eight results are already below 256.
    /// Circular positions and wrapped vertical extrema remain caller-owned.
    /// </remarks>
    internal static readonly ushort[] Radii = [8, 16, 24, 32, 40, 48, 56, 64];

    /// <summary>Parameter-one bit 1: the Fireflea moves in a circle rather than vertically.</summary>
    internal const ushort CircularMovementParameterBit = 0x0002;
}
