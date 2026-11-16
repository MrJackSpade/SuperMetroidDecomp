using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification set-up of Phantoon's encounter for fixtures that place his state without his room's initialization.</summary>
internal static class PhantoonEncounterSetUp
{
    /// <summary>Fixture set-up of Phantoon's encounter.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Begins <paramref name="state"/>'s encounter as his parts' setups do: each part holds its own part of it (#2252),
        /// and the room reads it from the room's boss encounters (#2194).
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal PhantoonEnemyState BeginPhantoonAsSetUp(PhantoonEnemyState state)
        {
            foreach (PhantoonPart part in new[] { state.BodyPart, state.EyePart, state.TentaclesPart, state.MouthPart })
                part.Place.SetSpeciesState(part);
            enemies.BossEncounters.Attach(state);
            return state;
        }
    }
}
