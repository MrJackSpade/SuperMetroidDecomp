using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Binds a Torizo state to a fixture slot that skips Torizo's initializer.</summary>
internal static class BombTorizoFixtureState
{
    /// <summary>
    /// Binds a new Torizo state to <paramref name="torizo"/>, with the posture and low bits of the authored
    /// <paramref name="parameter1"/> word.
    /// </summary>
    /// <returns>The bound state.</returns>
    internal static TorizoEnemyState BindFixtureState(this BombTorizoBehavior behavior, RoomEnemySlot torizo, int parameter1, bool isGolden = false)
    {
        var state = new TorizoEnemyState(torizo, isGolden)
        {
            Posture = TorizoParameterFlagWords.PostureOf(parameter1),
            PostureParameterLowBits = TorizoParameterFlagWords.PostureLowBitsOf(parameter1),
        };
        behavior.Access.TorizoState = state;
        return state;
    }
}
