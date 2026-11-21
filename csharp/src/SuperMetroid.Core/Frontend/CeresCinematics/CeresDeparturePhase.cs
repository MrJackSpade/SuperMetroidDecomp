namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Named phases owned by <see cref="CeresDepartureState"/>.</summary>
public enum CeresDeparturePhase
{
    /// <summary>Samus has made it to the Ceres elevator: gameplay continues, then the elevator hold (<see cref="Areas.Ceres.CeresElevatorDepartureDefinitions.DepartureHoldFrames"/>) counts down after each update before the blackout.</summary>
    HoldingOnElevator,
    /// <summary>The blackout from Ceres: gameplay continues, then the screen fades one brightness step each update, with no wait between steps, until black, where the outer game owner saves the Ceres departure checkpoint and enters the destruction cinematic.</summary>
    FadingToBlack,
}
