namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The behaviour a morph-ball eye's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum MorphBallEyeAction
{
    /// <summary>Waits until Samus is strictly within 128 room pixels on both axes before starting the opening animation.</summary>
    WaitForSamus,
    /// <summary>Decrements the 32-update opening countdown, then requests the reveal beam and begins tracking Samus.</summary>
    Activating,
    /// <summary>Aims toward Samus and selects angle-dependent eye artwork while within 176 horizontal and 128 vertical pixels, otherwise begins closing.</summary>
    Active,
    /// <summary>Waits for the 32-update closing countdown to reach zero or wrap negative, then resumes proximity waiting.</summary>
    Deactivating,
    /// <summary>A beam mount shows its mount pose; it does not track Samus or own the beam.</summary>
    ShowingMount,
}
