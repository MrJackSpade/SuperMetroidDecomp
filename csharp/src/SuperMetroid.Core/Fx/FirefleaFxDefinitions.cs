using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Fireflea room effect's darkness definitions, counted on the game's fade levels. The room's flash is an animation of
/// its looks (<see cref="FirefleaFlashAnimations"/>); the flash it shows and the darkness the Firefleas killed bring are
/// mutable state in <see cref="FirefleaFxState"/>.
/// </summary>
internal static class FirefleaFxDefinitions
{
    /// <summary>
    /// The darkness a sixth Fireflea death shows: the room lightens to a faint darkness of fade level two, where the
    /// ramp's deepest step had darkened it by twenty-five. Presentation only: no gameplay reads the shade.
    /// </summary>
    internal const int SixthDeathFaintDarknessLevel = 2;

    /// <summary>
    /// The darkness fade level each count of Firefleas killed shows, from none killed through the deepest darkness of
    /// five, and the faint darkness a sixth death shows.
    /// </summary>
    internal static readonly ImmutableArray<int> DarknessLevels = [0, 6, 12, 18, 24, 25, SixthDeathFaintDarknessLevel];

    /// <summary>The updates the room shows each look of its flash.</summary>
    internal const int FlashDuration = 6;

    /// <summary>The Firefleas killed, five, from which the room stops flashing and holds its flash's brightest look.</summary>
    internal const int KilledToHoldFlash = 5;
}
