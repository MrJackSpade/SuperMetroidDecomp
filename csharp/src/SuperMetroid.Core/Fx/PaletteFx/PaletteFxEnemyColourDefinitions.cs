using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX colour groups that recolour an enemy's own colours (#1512): the enemy lays the shade of each record its
/// group shows over its draws through its colour state, in the order the records and its own colour writes came.
/// </summary>
internal static class PaletteFxEnemyColourDefinitions
{
    /// <summary>The groups an enemy's colour state shows: the Torizos' glowing belly.</summary>
    internal static ImmutableHashSet<PaletteFxColours> EnemyColours { get; } = [PaletteFxColours.TorizoBelly];
}
