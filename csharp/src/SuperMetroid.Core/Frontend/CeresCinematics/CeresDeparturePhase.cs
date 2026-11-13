namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Named phases owned by <see cref="CeresDepartureState"/>.</summary>
public enum CeresDeparturePhase
{
    /// <summary>The state before the Ceres room requests departure; ordinary Ceres gameplay and elevator arrival are not owned by this sequence.</summary>
    Inactive,
    /// <summary>Samus has made it to the Ceres elevator: gameplay continues, then the 60-call hold decrements after each update before the blackout.</summary>
    HoldingOnElevator,
    /// <summary>The blackout from Ceres: gameplay continues, then one brightness step fades the screen per call with the fade-delay counters zero.</summary>
    FadingToBlack,
    /// <summary>The screen has gone black; the outer game owner may save the Ceres departure checkpoint and enter the destruction cinematic, without restarting this timer.</summary>
    Complete,
}
