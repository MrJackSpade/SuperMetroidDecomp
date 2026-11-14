using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects' colour groups that recolour an enemy's own colours (#1512): the enemy lays the shade of each frame its
/// group shows over its draws through its colour state, in the order the frames and its own colour writes came.
/// </summary>
internal static class ColourEffectEnemyColourDefinitions
{
    /// <summary>
    /// The groups an enemy's colour state shows: the Torizos' glowing belly, the Ridley and Phantoon sprite statues'
    /// grey-out in Tourian's statue room, the escape's red-flashing shutters over Mother Brain's sprites, and the landed
    /// gunship's glowing lights (#1733).
    /// </summary>
    internal static ImmutableHashSet<ColourEffectColours> EnemyColours { get; } =
        [ColourEffectColours.TorizoBelly, ColourEffectColours.RidleyStatue, ColourEffectColours.PhantoonStatue, ColourEffectColours.TourianShutters,
            ColourEffectColours.GunshipGlow];
}
