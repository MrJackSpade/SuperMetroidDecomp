namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Named bank-$82 phases owned by <see cref="CeresDepartureState"/>.</summary>
public enum CeresDeparturePhase
{
    /// <summary>Host state before $89:ACC3 requests departure; ordinary Ceres gameplay and elevator arrival are not owned by this sequence.</summary>
    Inactive,
    /// <summary>GameState_20_MadeItToCeresElevator: gameplay continues, then the 60-call hold decrements after each update before entering state $21.</summary>
    HoldingOnElevator,
    /// <summary>GameState_21_BlackoutFromCeres: gameplay continues, then one INIDISP brightness step fades the screen per call with the native fade-delay words zero.</summary>
    FadingToBlack,
    /// <summary>Forced blank has been reached; the outer game owner may save the Ceres departure checkpoint and enter the destruction cinematic, without restarting this timer.</summary>
    Complete,
}
