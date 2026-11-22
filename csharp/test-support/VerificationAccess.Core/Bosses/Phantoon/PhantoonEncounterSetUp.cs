using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification placement of Phantoon as his room's population places him.</summary>
internal static class PhantoonEncounterSetUp
{
    /// <summary>A check's enemies, which Phantoon is placed in.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Places Phantoon as his room's population places him (#2554): his body, eye, tentacles and mouth in the room's
        /// first four places, in his room's order, each set up as the load sets it up, his body's setup beginning the encounter
        /// the others join. The enemies load a room once, so a check places him in enemies of their own.
        /// </summary>
        /// <returns>The encounter his body's setup began.</returns>
        internal PhantoonEnemyState PlacePhantoon() =>
            Ensure.NotNull(enemies).LoadPopulation(PopulationPlacementAccess.Entry(Enemies.EnemyDefinitionId.PhantoonBody),
                PopulationPlacementAccess.Entry(Enemies.EnemyDefinitionId.PhantoonEye), PopulationPlacementAccess.Entry(Enemies.EnemyDefinitionId.PhantoonTentacles),
                PopulationPlacementAccess.Entry(Enemies.EnemyDefinitionId.PhantoonMouth))[0].EntityOf<PhantoonEntity>().Encounter;
    }
}
