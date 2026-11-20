namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Samus's rainbow during the Baby's death.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    /// <summary>The auxiliary Samus rainbow-colour updater is disabled.</summary>
    Inactive,

    /// <summary>Waits for the falling Baby to come within its margin above Samus, which starts the rainbow.</summary>
    ActivateWhenEnemyIsLow,

    /// <summary>Progressively lengthens the rainbow animation interval as the Baby finishes transferring energy.</summary>
    GraduallySlowAnimationDown,
}
