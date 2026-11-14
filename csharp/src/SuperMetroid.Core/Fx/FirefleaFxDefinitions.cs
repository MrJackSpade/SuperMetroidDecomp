using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's darkness definitions, counted on the game's fade levels. The flashing
/// phase and the darkness step the enemy deaths reach are mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The darkness a sixth Fireflea death shows: the room lightens to a faint darkness of fade level two, where the
    /// ramp's deepest step had darkened it by twenty-five. Presentation only: no gameplay reads the shade.
    /// </summary>
    internal const int SixthDeathFaintDarknessLevel = 2;

    /// <summary>
    /// The darkness fade level each darkness step shows, from none (no Fireflea killed) through the deepest darkness
    /// of five deaths, and the faint darkness a sixth death shows.
    /// </summary>
    internal static readonly ImmutableArray<int> DarknessLevels = [0, 6, 12, 18, 24, 25, SixthDeathFaintDarknessLevel];

    /// <summary>Six effect passes per flash phase.</summary>
    internal const int FlashDuration = 6;

    /// <summary>The twelve phases of the flash cycle, which then starts again from the first.</summary>
    internal const int FlashCount = 12;

    /// <summary>The darkness step, five deaths, from which the room stops flashing.</summary>
    internal const int SteadyDarknessStep = 5;

    /// <summary>The flash phase the room holds from the deepest darkness on: the cycle's brightest.</summary>
    internal const int SteadyFlashPhase = 6;
}
