namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The phase of Phantoon's encounter the body runs each update. Each member names the bank-$A7 routine it replaced;
/// the values are arbitrary identities (#1413), and the main AI dispatches on the member.
/// </summary>
public enum PhantoonAiFunction
{
    /// <summary>$A7:D4A9: allocates the introductory circle of eight flames from the physical projectile pool.</summary>
    SpawnStartingFlames = 1,
    /// <summary>$A7:D4EE: waits before activating the introductory flame circle's spin.</summary>
    WaitBeforeActivatingStartingFlames,
    /// <summary>$A7:D508: waits for the introductory flame orbit to finish before wavy materialization.</summary>
    WaitForStartingFlamesToDisappear,
    /// <summary>$A7:D54A: advances the introductory wavy palette fade-in.</summary>
    WavyFadeIn,
    /// <summary>$A7:D596: selects the first figure-eight round's movement pattern.</summary>
    PickFirstRoundPattern,
    /// <summary>$A7:D5E7: advances the selected figure-eight path until the eye-opening window.</summary>
    MoveInFigureEightThenOpenEye,
    /// <summary>$A7:D60D: tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    EyeTracksSamus,
    /// <summary>$A7:D65C: makes Phantoon opaque and initializes its triggered swoop.</summary>
    BecomeSolidAndSwoop,
    /// <summary>$A7:D678: advances the opaque swoop toward Samus.</summary>
    Swooping,
    /// <summary>$A7:D6B9: continues the swoop while fading the body out.</summary>
    FadeOutWhileSwooping,
    /// <summary>$A7:D6D4: counts the hidden delay before another figure-eight appearance.</summary>
    WaitAfterFadeOut,
    /// <summary>$A7:D6E2: places Phantoon for the next figure-eight round.</summary>
    PickNextAppearance,
    /// <summary>$A7:D72D: materializes the next figure-eight appearance.</summary>
    FadeInBeforeFigureEight,
    /// <summary>$A7:D73F: the flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    BecomeSolidAfterFlameRain,
    /// <summary>$A7:D767: advances the flame-rain materialization before entering its vulnerability window.</summary>
    FadeInDuringFlameRain,
    /// <summary>$A7:D788: advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    TrackSamusDuringFlameRain,
    /// <summary>$A7:D7D5: fades out between successive flame-rain attacks.</summary>
    FadeOutDuringFlameRain,
    /// <summary>$A7:D7F7: schedules another flame rain while Phantoon is hidden.</summary>
    SpawnFlameRain,
    /// <summary>$A7:D82A: initializes the first flame rain while fading away from the preceding round.</summary>
    FadeOutBeforeFirstFlameRain,
    /// <summary>$A7:D85C: fades out before the enraged attack sequence.</summary>
    FadeOutBeforeRage,
    /// <summary>$A7:D874: positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    MoveToTopCenterForRage,
    /// <summary>$A7:D891: fades into the enraged attack's top-center appearance.</summary>
    FadeInForRage,
    /// <summary>$A7:D8AC: advances the enraged flame pattern before the post-rage fade.</summary>
    Enraged,
    /// <summary>$A7:D916: fades out after the enraged attack.</summary>
    FadeOutAfterRage,
    /// <summary>$A7:D92E: completes the fatal-damage swoop before starting the death choreography.</summary>
    FinishFatalSwoop,
    /// <summary>$A7:D948: alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    DyingFadeInOut,
    /// <summary>$A7:D98B: schedules physical death explosions and prepares the final distortion.</summary>
    DyingExplosions,
    /// <summary>$A7:DA51: initializes the final wavy HDMA and mosaic death presentation.</summary>
    BeginFinalWavyDeath,
    /// <summary>$A7:DA86: advances the final wavy/mosaic palette fade-out.</summary>
    DyingFadeOut,
    /// <summary>$A7:DAD7: clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    AlmostDead,
    /// <summary>$A7:DB3D: restores the powered Wrecked Ship palette, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    Dead,
    /// <summary>$A7:D4A8: an explicit return entry with no encounter-state mutation.</summary>
    NoOperation,
}
