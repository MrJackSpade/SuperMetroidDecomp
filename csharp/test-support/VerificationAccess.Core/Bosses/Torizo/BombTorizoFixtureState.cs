using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Binds a Torizo state to a fixture slot that skips Torizo's initializer.</summary>
internal static class BombTorizoFixtureState
{
    /// <summary>
    /// Installs a new Torizo state as <paramref name="torizo"/>'s species state, with the posture latches of the authored
    /// <paramref name="parameter1"/> word.
    /// </summary>
    /// <returns>The bound state.</returns>
    internal static TorizoEnemyState BindTorizoFixtureState(this RoomEnemySlot torizo, int parameter1, bool isGolden = false)
    {
        var state = new TorizoEnemyState(isGolden)
        {
            Posture = TorizoParameterFlagWords.PostureOf(parameter1),
        };
        torizo.SetSpeciesState(state);
        return state;
    }
}
