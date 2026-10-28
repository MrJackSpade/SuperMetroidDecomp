namespace SuperMetroid.Core.Fx;

/// <summary>
/// Compiled fixed-color shade definitions for the Fireflea room effect. The flashing
/// phase and enemy-death darkness level are mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The shade level at darkness offset twelve, one entry past the six-step ramp: the cartridge reads the first byte
    /// of the next routine there, 194, whose five-bit intensity is two.
    /// </summary>
    internal const byte PastRampShadeLevel = 194;
    /// <summary>$88:B07F/$88:B0D5: six effect passes per flashing shade.</summary>
    internal const ushort FlashDuration = 6;

    /// <summary>$88:B0EC: twelve entries before the flashing cycle wraps.</summary>
    internal const ushort FlashCount = 12;

    /// <summary>$88:B0DE: darkness byte offset at which flashing stops.</summary>
    internal const ushort SteadyDarknessThreshold = 10;

    /// <summary>$88:B0E3: fixed flashing-table index used at maximum darkness.</summary>
    internal const ushort SteadyFlashIndex = 6;
}
