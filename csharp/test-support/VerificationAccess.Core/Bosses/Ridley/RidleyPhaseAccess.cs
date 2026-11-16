namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Verification's reads of the phase Ridley runs (#2227): his reveal's until he lifts off, then his encounter's. A check
/// that watches him through both asks whether he runs a given phase, which is false while he is in the other stage.
/// </summary>
internal static class RidleyPhaseAccess
{
    /// <summary>Whether Ceres Ridley has lifted off and runs <paramref name="phase"/>.</summary>
    internal static bool Runs(this RidleyEnemyState state, CeresRidleyPhase phase) => state.LiftedOff && state.CeresPhase == phase;

    /// <summary>Whether Lower Norfair Ridley has lifted off and runs <paramref name="phase"/>.</summary>
    internal static bool Runs(this RidleyEnemyState state, NorfairRidleyPhase phase) => state.LiftedOff && state.NorfairPhase == phase;

    /// <summary>The phase Ceres Ridley runs, for a diagnostic: his reveal's or his own.</summary>
    internal static string CeresPhaseName(this RidleyEnemyState state) =>
        state.LiftedOff ? state.CeresPhase.ToString() : $"reveal {state.RevealPhase}";

    /// <summary>The phase Lower Norfair Ridley runs, for a diagnostic: his reveal's or his own.</summary>
    internal static string NorfairPhaseName(this RidleyEnemyState state) =>
        state.LiftedOff ? state.NorfairPhase.ToString() : $"reveal {state.RevealPhase}";
}
