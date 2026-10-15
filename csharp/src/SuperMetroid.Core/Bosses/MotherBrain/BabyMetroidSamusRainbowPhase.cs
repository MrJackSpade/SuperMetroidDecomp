namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Named equivalents of `$A9:CD30/$CD4B`'s indirect palette functions.</summary>
public enum BabyMetroidSamusRainbowPhase
{
    /// <summary>The auxiliary Samus rainbow-palette updater is disabled.</summary>
    Inactive,

    /// <summary>Waits for the Baby's energy reserve to become low before beginning the terminal palette cadence.</summary>
    ActivateWhenEnemyIsLow,

    /// <summary>Progressively lengthens the rainbow animation interval as the Baby finishes transferring energy.</summary>
    GraduallySlowAnimationDown,
}
