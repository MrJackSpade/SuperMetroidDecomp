namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Verification's reads of the phase Ridley runs (#2227): his reveal's until he lifts off, then his encounter's. A check
/// that watches him through both asks whether he runs a given phase, which is false while he is in the other stage.
/// </summary>
internal static class RidleyPhaseAccess
{
    /// <summary>Whether Ceres Ridley has lifted off and runs <paramref name="phase"/>.</summary>
    internal static bool Runs(this CeresRidleyState state, CeresRidleyPhase phase) => state.LiftedOff && state.Phase == phase;

    /// <summary>Whether Lower Norfair Ridley has lifted off and runs <paramref name="phase"/>.</summary>
    internal static bool Runs(this NorfairRidleyState state, NorfairRidleyPhase phase) => state.LiftedOff && state.Phase == phase;

    /// <summary>The phase Ceres Ridley runs, for a diagnostic: his reveal's or his own.</summary>
    internal static string PhaseName(this CeresRidleyState state) =>
        state.LiftedOff ? state.Phase.ToString() : $"reveal {state.RevealPhase}";

    /// <summary>The phase Lower Norfair Ridley runs, for a diagnostic: his reveal's or his own.</summary>
    internal static string PhaseName(this NorfairRidleyState state) =>
        state.LiftedOff ? state.Phase.ToString() : $"reveal {state.RevealPhase}";

    /// <summary>The phase either Ridley runs, for a diagnostic.</summary>
    internal static string PhaseName(this RidleyEncounterState state) => state switch
    {
        CeresRidleyState ceres => ceres.PhaseName(),
        NorfairRidleyState norfair => norfair.PhaseName(),
        _ => throw new InvalidOperationException($"Undefined Ridley encounter {state.GetType().Name}."),
    };
}
