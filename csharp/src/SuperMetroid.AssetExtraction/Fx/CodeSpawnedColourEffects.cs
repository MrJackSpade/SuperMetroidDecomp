using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// Colour objects code spawns in a room outside any FX record (#2630, #2638), with what places them: the room setup that
/// spawns them, or the enemy whose code spawns them, which the room's population places.
/// </summary>
/// <param name="Setup">The room setup that spawns the objects, or null where an enemy spawns them.</param>
/// <param name="Enemy">The enemy whose code spawns the objects, or null where a setup spawns them.</param>
/// <param name="Effects">The colour objects spawned.</param>
internal sealed partial record CodeSpawnedColourEffects(RoomSetupBehavior? Setup, EnemyDefinitionId? Enemy, ImmutableArray<ColourEffectId> Effects)
{
    /// <summary>Whether a room loading <paramref name="state"/> runs the setup, or places the enemy, that spawns the objects.</summary>
    internal bool RunsIn(RoomStateSettings state)
    {
        Ensure.NotNull(state);
        if (Setup is not null && state.SetupBehavior == Setup)
            return true;
        if (Enemy is not { } enemy)
            return false;
        foreach (RoomEnemyPlacement placement in state.EnemyPopulation.Placements.Span)
        {
            if (placement.DefinitionId == enemy)
                return true;
        }
        return false;
    }
}
