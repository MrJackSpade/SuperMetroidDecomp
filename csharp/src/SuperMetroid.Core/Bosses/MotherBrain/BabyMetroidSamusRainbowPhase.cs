namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby's two rainbow colour phases.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    /// <summary>Samus's rainbow has not started.</summary>
    Inactive,

    /// <summary>Waits for the Baby's energy reserve to become low before beginning the terminal colour cadence.</summary>
    ActivateWhenEnemyIsLow,

    /// <summary>Progressively lengthens the rainbow animation interval as the Baby finishes transferring energy.</summary>
    GraduallySlowAnimationDown,
}
