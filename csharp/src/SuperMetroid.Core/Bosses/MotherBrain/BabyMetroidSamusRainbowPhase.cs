namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby's two rainbow palette phases.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    /// <summary>The auxiliary Samus rainbow-palette updater is disabled.</summary>
    Inactive,

    /// <summary>Waits for the Baby's energy reserve to become low before beginning the terminal palette cadence.</summary>
    ActivateWhenEnemyIsLow,

    /// <summary>Progressively lengthens the rainbow animation interval as the Baby finishes transferring energy.</summary>
    GraduallySlowAnimationDown,
}
