using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification set-up of Crocomire's tongue for fixtures that place his encounter without his room's setup.</summary>
internal static class CrocomireTongueFixtures
{
    /// <summary>Fixture set-up of the tongue of an encounter.</summary>
    extension(CrocomireEnemyState state)
    {
        /// <summary>
        /// Joins <paramref name="tongue"/> to the encounter as his tongue's setup does in a living room (#2400): the tongue
        /// holds the encounter at its body offset and the encounter requires it from here.
        /// </summary>
        internal void JoinFixtureTongue(RoomEnemyPlace tongue)
        {
            tongue.SetSpeciesState(new CrocomireTongueState(state));
            state.JoinTongue(tongue);
        }
    }
}
