namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The behaviour a morph-ball eye body's main AI runs; its beam mount runs none.
/// Its values are arbitrary.
/// </summary>
public enum MorphBallEyeAction
{
    /// <summary>Waits until Samus is strictly within its opening reach on both axes (<see cref="MorphBallEyeWatchDefinitions.OpeningReachX"/>) before starting the opening animation.</summary>
    WaitForSamus,
    /// <summary>Counts down its opening (<see cref="MorphBallEyeWatchDefinitions.TransitionDuration"/>), then requests the reveal beam and begins tracking Samus.</summary>
    Activating,
    /// <summary>Aims toward Samus and selects angle-dependent eye artwork while she stays within its stay-open reach (<see cref="MorphBallEyeWatchDefinitions.StayOpenReachX"/>), otherwise begins closing.</summary>
    Active,
    /// <summary>Waits out its closing (<see cref="MorphBallEyeWatchDefinitions.TransitionDuration"/>), then resumes proximity waiting.</summary>
    Deactivating,
}
