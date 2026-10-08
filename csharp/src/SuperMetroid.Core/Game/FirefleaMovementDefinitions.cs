namespace SuperMetroid.Core.Game;

/// <summary>Exact bounded Fireflea movement-radius rule.</summary>
internal static class FirefleaMovementDefinitions
{

    /// <summary>Returns 8*(index+1) for the parameter-two high-byte selector 0..7.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against every NTSC J/U v1.0 word and pinned
    /// bank_A3.asm SetFirefleaRadius. Native doubles the selector for word indexing
    /// and masks the result to a byte; all eight results are already below 256.
    /// Circular positions and wrapped vertical extrema remain caller-owned.
    /// </remarks>
    internal static ushort Radius(byte index)
    {
        if (index >= 8)
            throw new InvalidDataException(
                $"Fireflea radius index {index} is outside the 8-entry native table.");
        return (ushort)(8 * (index + 1));
    }
}
