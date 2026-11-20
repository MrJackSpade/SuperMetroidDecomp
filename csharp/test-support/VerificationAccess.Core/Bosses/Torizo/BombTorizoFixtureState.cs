using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Binds a Torizo state to a fixture slot that skips Torizo's initializer.</summary>
internal static class BombTorizoFixtureState
{
    /// <summary>
    /// Binds the Torizo of <paramref name="variant"/> in <paramref name="torizo"/> (#2558) and installs a new Torizo state as
    /// its species state, with the posture latches of the authored <paramref name="parameter1"/> word.
    /// </summary>
    /// <returns>The bound state.</returns>
    internal static TorizoEnemyState BindTorizoFixtureState(this RoomEnemyPlace torizo, int parameter1, TorizoVariant? variant = null)
    {
        TorizoVariant bound = variant ?? TorizoVariant.Bomb;
        torizo.Bind(bound == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo, torizo.KeptDefinition);
        // The Torizo is set up through its species as the room places it, its variant its definition's (#2554).
        TorizoVariant shown = bound;
        if (torizo.DefinitionId == EnemyDefinitionId.None)
            torizo.Bind(shown == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo,
                RoomEnemyDefinitionLookup.Get(shown == TorizoVariant.Golden ? EnemyDefinitionId.GoldenTorizo : EnemyDefinitionId.BombTorizo));
        TorizoEnemyState state = torizo.SetUpPlaced<TorizoEnemyState>();
        state.Posture = TorizoParameterFlagWords.PostureOf(parameter1);
        return state;
    }
}
