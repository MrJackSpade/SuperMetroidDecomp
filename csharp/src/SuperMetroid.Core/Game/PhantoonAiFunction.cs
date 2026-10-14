namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A7 function pointers installed in Phantoon body variable F. Keeping the
/// actual addresses makes debugger watches line up with WRAM <c>$0FB2</c> and prevents a
/// host-only state machine from quietly diverging from the cartridge dispatcher.
/// </summary>
public enum PhantoonAiFunction : ushort
{
    /// <summary>$A7:D4A9: allocates the introductory circle of eight flames from the physical projectile pool.</summary>
    SpawnStartingFlames = 0xd4a9,
    /// <summary>$A7:D4EE: waits before activating the introductory flame circle's spin.</summary>
    WaitBeforeActivatingStartingFlames = 0xd4ee,
    /// <summary>$A7:D508: waits for the introductory flame orbit to finish before wavy materialization.</summary>
    WaitForStartingFlamesToDisappear = 0xd508,
    /// <summary>$A7:D54A: advances the introductory wavy palette fade-in.</summary>
    WavyFadeIn = 0xd54a,
    /// <summary>$A7:D596: selects the first figure-eight round's movement pattern.</summary>
    PickFirstRoundPattern = 0xd596,
    /// <summary>$A7:D5E7: advances the selected figure-eight path until the eye-opening window.</summary>
    MoveInFigureEightThenOpenEye = 0xd5e7,
    /// <summary>$A7:D60D: tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    EyeTracksSamus = 0xd60d,
    /// <summary>$A7:D65C: makes Phantoon opaque and initializes its triggered swoop.</summary>
    BecomeSolidAndSwoop = 0xd65c,
    /// <summary>$A7:D678: advances the opaque swoop toward Samus.</summary>
    Swooping = 0xd678,
    /// <summary>$A7:D6B9: continues the swoop while fading the body out.</summary>
    FadeOutWhileSwooping = 0xd6b9,
    /// <summary>$A7:D6D4: counts the hidden delay before another figure-eight appearance.</summary>
    WaitAfterFadeOut = 0xd6d4,
    /// <summary>$A7:D6E2: places Phantoon for the next figure-eight round.</summary>
    PickNextAppearance = 0xd6e2,
    /// <summary>$A7:D72D: materializes the next figure-eight appearance.</summary>
    FadeInBeforeFigureEight = 0xd72d,
    /// <summary>$A7:D73F: the flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    BecomeSolidAfterFlameRain = 0xd73f,
    /// <summary>$A7:D767: advances the flame-rain materialization before entering its vulnerability window.</summary>
    FadeInDuringFlameRain = 0xd767,
    /// <summary>$A7:D788: advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    TrackSamusDuringFlameRain = 0xd788,
    /// <summary>$A7:D7D5: fades out between successive flame-rain attacks.</summary>
    FadeOutDuringFlameRain = 0xd7d5,
    /// <summary>$A7:D7F7: schedules another flame rain while Phantoon is hidden.</summary>
    SpawnFlameRain = 0xd7f7,
    /// <summary>$A7:D82A: initializes the first flame rain while fading away from the preceding round.</summary>
    FadeOutBeforeFirstFlameRain = 0xd82a,
    /// <summary>$A7:D85C: fades out before the enraged attack sequence.</summary>
    FadeOutBeforeRage = 0xd85c,
    /// <summary>$A7:D874: positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    MoveToTopCenterForRage = 0xd874,
    /// <summary>$A7:D891: fades into the enraged attack's top-center appearance.</summary>
    FadeInForRage = 0xd891,
    /// <summary>$A7:D8AC: advances the enraged flame pattern before the post-rage fade.</summary>
    Enraged = 0xd8ac,
    /// <summary>$A7:D916: fades out after the enraged attack.</summary>
    FadeOutAfterRage = 0xd916,
    /// <summary>$A7:D92E: completes the fatal-damage swoop before starting the death choreography.</summary>
    FinishFatalSwoop = 0xd92e,
    /// <summary>$A7:D948: alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    DyingFadeInOut = 0xd948,
    /// <summary>$A7:D98B: schedules physical death explosions and prepares the final distortion.</summary>
    DyingExplosions = 0xd98b,
    /// <summary>$A7:DA51: initializes the final wavy HDMA and mosaic death presentation.</summary>
    BeginFinalWavyDeath = 0xda51,
    /// <summary>$A7:DA86: advances the final wavy/mosaic palette fade-out.</summary>
    DyingFadeOut = 0xda86,
    /// <summary>$A7:DAD7: clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    AlmostDead = 0xdad7,
    /// <summary>$A7:DB3D: restores the powered Wrecked Ship palette, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    Dead = 0xdb3d,
    /// <summary>$A7:D4A8: an explicit return entry with no encounter-state mutation.</summary>
    NoOperation = 0xd4a8,
}
