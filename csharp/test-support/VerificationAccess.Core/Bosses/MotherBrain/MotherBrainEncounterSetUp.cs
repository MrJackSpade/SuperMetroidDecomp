using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification set-up of Mother Brain's encounter for fixtures that place her state without her room's initialization.</summary>
internal static class MotherBrainEncounterSetUp
{
    /// <summary>Fixture set-up of Mother Brain's encounter.</summary>
    extension(MotherBrainEncounter encounter)
    {
        /// <summary>
        /// Begins <paramref name="state"/>'s encounter as her body's setup does, her body's place holding it, and gives each
        /// of <paramref name="parts"/> (her head, a tube or the Baby) the encounter it joins, as their setups would (#2218).
        /// </summary>
        /// <returns>The encounter begun.</returns>
        internal MotherBrainEnemyState BeginAsSetUp(MotherBrainEnemyState state, params RoomEnemyPlace[] parts)
        {
            encounter.Begin(state);
            _ = parts;
            return state;
        }
    }
}
