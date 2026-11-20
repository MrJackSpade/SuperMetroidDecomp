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
    internal static TorizoEnemyState BindTorizoFixtureState(this RoomEnemyPlace torizo, int parameter1, TorizoVariant? variant = null)
    {
        // The Torizo is set up through its species as the room places it, its variant its definition's (#2554).
        TorizoVariant shown = variant ?? TorizoVariant.Bomb;
        if (torizo.DefinitionId == EnemyDefinitionId.None)
            torizo.Bind(shown == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo,
                RoomEnemyDefinitionLookup.Get(shown == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo));
        TorizoEnemyState state = torizo.SetUpPlaced<TorizoEnemyState>();
        state.Posture = TorizoParameterFlagWords.PostureOf(parameter1);
        return state;
    }
}
