namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Samus's rainbow during the Baby's death.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    /// <summary>Samus's rainbow has not started.</summary>
    Inactive,

    /// <summary>Waits for the falling Baby to come within its margin above Samus, which starts the rainbow.</summary>
    ActivateWhenEnemyIsLow,

    /// <summary>Progressively lengthens the rainbow animation interval as the Baby finishes transferring energy.</summary>
    GraduallySlowAnimationDown,
}
