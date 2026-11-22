using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification set-up of Phantoon's encounter for fixtures that place his state without his room's initialization.</summary>
internal static class PhantoonEncounterSetUp
{
    /// <summary>Fixture set-up of Phantoon's encounter.</summary>
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

        /// <summary>
        /// Begins <paramref name="state"/>'s encounter as his parts' setups do: each part's place holds the part his room's
        /// population places there, which holds its own part of the encounter (#2252), and the room reads it from the room's
        /// boss encounters (#2194).
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal PhantoonEnemyState BeginPhantoonAsSetUp(PhantoonEnemyState state)
        {
            (PhantoonPart Part, Enemies.EnemyDefinitionId Species)[] parts =
            [
                (state.BodyPart, Enemies.EnemyDefinitionId.PhantoonBody), (state.EyePart, Enemies.EnemyDefinitionId.PhantoonEye),
                (state.TentaclesPart, Enemies.EnemyDefinitionId.PhantoonTentacles), (state.MouthPart, Enemies.EnemyDefinitionId.PhantoonMouth),
            ];
            foreach ((PhantoonPart part, Enemies.EnemyDefinitionId species) in parts)
            {
                // A place a check has not bound holds the part the population places there.
                if (!part.Place.Holds(species))
                    part.Place.Bind(species, part.Place.KeptDefinition);
                part.Place.SetSpeciesState(part);
            }
            enemies.BossEncounters.Attach(state);
            return state;
        }
    }
}
