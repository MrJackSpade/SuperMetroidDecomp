using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification set-up of Crocomire's tongue for fixtures that place his encounter without his room's setup.</summary>
internal static class CrocomireTongueFixtures
{
    /// <summary>Fixture set-up of the tongue of an encounter.</summary>
    extension(CrocomireEnemyState state)
    {
        /// <summary>
        /// Joins <paramref name="tongue"/>, the encounter's own tongue place, to the encounter as his tongue's setup does in
        /// a living room (#2400): the tongue holds the encounter at its body offset.
        /// </summary>
        /// <exception cref="InvalidOperationException">The encounter's tongue is another place.</exception>
        internal void JoinFixtureTongue(RoomEnemyPlace tongue)
        {
            if (!ReferenceEquals(state.Tongue, tongue))
                throw new InvalidOperationException($"The encounter's tongue is enemy {state.Tongue.Ordinal}, not {tongue.Ordinal}.");
            tongue.SetSpeciesState(new CrocomireTongueState(state));
        }
    }
}
