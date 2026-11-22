using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects' colour groups that recolour an enemy's own colours (#1512): the enemy lays the shade of each frame its
/// group shows over its draws through its colour state, in the order the frames and its own colour changes came.
/// </summary>
internal static partial class ColourEffectEnemyColourDefinitions
{
    /// <summary>
    /// The groups one enemy shows, each on the enemy its effect's start hands it (#2288): the Torizos' glowing belly, the
    /// Ridley and Phantoon sprite statues' grey-out in Tourian's statue room, and the landed gunship's glowing lights (#1733).
    /// </summary>
    internal static ImmutableHashSet<ColourEffectColours> EnemyColours { get; } =
        [ColourEffectColours.TorizoBelly, ColourEffectColours.RidleyStatue, ColourEffectColours.PhantoonStatue,
            ColourEffectColours.GunshipGlow];

    /// <summary>
    /// The escape's red-flashing shutters, which the room's destroyable shutters show, whichever of them the room holds:
    /// Mother Brain's death and the first escape room's effect start it before any shutter it shows on is known.
    /// </summary>
    internal const ColourEffectColours EscapeShutterColours = ColourEffectColours.TourianShutters;
}
