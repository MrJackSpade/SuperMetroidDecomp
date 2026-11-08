namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's darkness definitions, counted on the game's fade levels. The flashing
/// phase and enemy-death darkness level are mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The darkness fade level at darkness offset twelve, one entry past the six-step ramp: level two. The game read the
    /// byte past its ramp there, whose bits above the level only name colour channels its writes set to the same level.
    /// </summary>
    internal const int PastRampDarknessLevel = 2;
    /// <summary>Six effect passes per flashing shade.</summary>
    internal const int FlashDuration = 6;

    /// <summary>Twelve entries before the flashing cycle wraps.</summary>
    internal const int FlashCount = 12;

    /// <summary>Darkness byte offset at which flashing stops.</summary>
    internal const int SteadyDarknessThreshold = 10;

    /// <summary>Fixed flashing-table index used at maximum darkness.</summary>
    internal const int SteadyFlashIndex = 6;
}
