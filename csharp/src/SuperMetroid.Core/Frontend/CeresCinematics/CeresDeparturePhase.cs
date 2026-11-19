namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Named phases owned by <see cref="CeresDepartureState"/>.</summary>
public enum CeresDeparturePhase
{
    /// <summary>The state before the Ceres room requests departure; ordinary Ceres gameplay and elevator arrival are not owned by this sequence.</summary>
    Inactive,
    /// <summary>Samus has made it to the Ceres elevator: gameplay continues, then the elevator hold (<see cref="Areas.Ceres.CeresElevatorDepartureDefinitions.DepartureHoldFrames"/>) counts down after each update before the blackout.</summary>
    HoldingOnElevator,
    /// <summary>The blackout from Ceres: gameplay continues, then the screen fades one brightness step each update, with no wait between steps.</summary>
    FadingToBlack,
    /// <summary>The screen has gone black; the outer game owner may save the Ceres departure checkpoint and enter the destruction cinematic, without restarting this timer.</summary>
    Complete,
}
