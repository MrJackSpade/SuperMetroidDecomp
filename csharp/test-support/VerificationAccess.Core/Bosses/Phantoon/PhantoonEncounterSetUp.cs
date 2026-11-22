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
