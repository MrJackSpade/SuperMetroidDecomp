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
    /// The darkness fade level each count of Firefleas killed shows, from none killed through the deepest darkness of
    /// five, which every later death keeps (#2573).
    /// </summary>
    internal static readonly ImmutableArray<int> DarknessLevels = [0, 6, 12, 18, 24, 25];

    /// <summary>The updates the room shows each look of its flash.</summary>
    internal const int FlashDuration = 6;

    /// <summary>The Firefleas killed from which the room stops flashing and holds its flash's brightest look.</summary>
    internal const int KilledToHoldFlash = 5;
}
