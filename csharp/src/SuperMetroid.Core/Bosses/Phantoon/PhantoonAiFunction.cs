namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The phase of Phantoon's encounter the body runs each update. The values are arbitrary
/// identities (#1413), and the main AI dispatches on the member.
/// </summary>
public enum PhantoonAiFunction
{
    /// <summary>Allocates the introductory circle of eight flames from the physical projectile pool.</summary>
    SpawnStartingFlames = 1,
    /// <summary>Waits before activating the introductory flame circle's spin.</summary>
    WaitBeforeActivatingStartingFlames,
    /// <summary>Waits for the introductory flame orbit to finish before wavy materialization.</summary>
    WaitForStartingFlamesToDisappear,
    /// <summary>Advances the introductory wavy palette fade-in.</summary>
    WavyFadeIn,
    /// <summary>Selects the first figure-eight round's movement pattern.</summary>
    PickFirstRoundPattern,
    /// <summary>Advances the selected figure-eight path until the eye-opening window.</summary>
    MoveInFigureEightThenOpenEye,
    /// <summary>Tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    EyeTracksSamus,
    /// <summary>Makes Phantoon opaque and initializes its triggered swoop.</summary>
    BecomeSolidAndSwoop,
    /// <summary>Advances the opaque swoop toward Samus.</summary>
    Swooping,
    /// <summary>Continues the swoop while fading the body out.</summary>
    FadeOutWhileSwooping,
    /// <summary>Counts the hidden delay before another figure-eight appearance.</summary>
    WaitAfterFadeOut,
    /// <summary>Places Phantoon for the next figure-eight round.</summary>
    PickNextAppearance,
    /// <summary>Materializes the next figure-eight appearance.</summary>
    FadeInBeforeFigureEight,
    /// <summary>The flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    BecomeSolidAfterFlameRain,
    /// <summary>Advances the flame-rain materialization before entering its vulnerability window.</summary>
    FadeInDuringFlameRain,
    /// <summary>Advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    TrackSamusDuringFlameRain,
    /// <summary>Fades out between successive flame-rain attacks.</summary>
    FadeOutDuringFlameRain,
    /// <summary>Schedules another flame rain while Phantoon is hidden.</summary>
    SpawnFlameRain,
    /// <summary>Initializes the first flame rain while fading away from the preceding round.</summary>
    FadeOutBeforeFirstFlameRain,
    /// <summary>Fades out before the enraged attack sequence.</summary>
    FadeOutBeforeRage,
    /// <summary>Positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    MoveToTopCenterForRage,
    /// <summary>Fades into the enraged attack's top-center appearance.</summary>
    FadeInForRage,
    /// <summary>Advances the enraged flame pattern before the post-rage fade.</summary>
    Enraged,
    /// <summary>Fades out after the enraged attack.</summary>
    FadeOutAfterRage,
    /// <summary>Completes the fatal-damage swoop before starting the death choreography.</summary>
    FinishFatalSwoop,
    /// <summary>Alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    DyingFadeInOut,
    /// <summary>Schedules physical death explosions and prepares the final distortion.</summary>
    DyingExplosions,
    /// <summary>Initializes the final wave and mosaic death presentation.</summary>
    BeginFinalWavyDeath,
    /// <summary>Advances the final wavy/mosaic palette fade-out.</summary>
    DyingFadeOut,
    /// <summary>Clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    AlmostDead,
    /// <summary>Restores the powered Wrecked Ship palette, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    Dead,
    /// <summary>An explicit return entry with no encounter-state mutation.</summary>
    NoOperation,
}
