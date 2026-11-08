namespace SuperMetroid.Core.Fx;

/// <summary>
/// Compiled fixed-color shade definitions for the Fireflea room effect. The flashing
/// phase and enemy-death darkness level are mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The shade level at darkness offset twelve, one entry past the six-step ramp: the game reads the byte
    /// past its ramp there, 194, whose level is two.
    /// </summary>
    internal const int PastRampShadeLevel = 194;
    /// <summary>Six effect passes per flashing shade.</summary>
    internal const int FlashDuration = 6;

    /// <summary>Twelve entries before the flashing cycle wraps.</summary>
    internal const int FlashCount = 12;

    /// <summary>Darkness byte offset at which flashing stops.</summary>
    internal const int SteadyDarknessThreshold = 10;

    /// <summary>Fixed flashing-table index used at maximum darkness.</summary>
    internal const int SteadyFlashIndex = 6;
}
