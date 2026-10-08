namespace SuperMetroid.Core.Game;

/// <summary>Named equivalents of the entrance's bank-$A9 function pointers.</summary>
public enum BabyMetroidCutscenePhase
{
    /// <summary>No Baby Metroid cutscene function is active.</summary>
    Inactive,

    /// <summary>Moves the Baby rapidly into the room until it reaches the first scripted X threshold.</summary>
    DashOntoScreen,

    /// <summary>Curves the Baby toward Mother Brain's head using the native angle and speed updates.</summary>
    CurveTowardMotherBrainHead,

    /// <summary>Closes the final gap to Mother Brain before the attachment state begins.</summary>
    GetRightUpInMotherBrainsFace,

    /// <summary>Anchors the Baby to Mother Brain's head and queues the latch sound edge.</summary>
    LatchOntoMotherBrain,

    /// <summary>Requests Mother Brain's stumble-back body function before draining begins.</summary>
    SetMotherBrainToStumbleBack,

    /// <summary>Activates Mother Brain's body and rainbow-beam owners for the energy-drain sequence.</summary>
    ActivateRainbowBeamAndMotherBrainBody,

    /// <summary>Retains the drain attachment until Mother Brain's body function reaches its corpse state.</summary>
    WaitForMotherBrainToTurnToCorpse,

    /// <summary>Stops transferring energy from Mother Brain and starts the release delay.</summary>
    StopDraining,

    /// <summary>Detaches from Mother Brain and emits the three scripted dust-cloud projectiles.</summary>
    LetGoAndSpawnDustClouds,

    /// <summary>Moves the Baby upward to its ceiling waypoint before it crosses toward Samus.</summary>
    MoveToTheCeiling,

    /// <summary>Flies from the ceiling waypoint to Samus's scripted attachment point.</summary>
    MoveToSamus,

    /// <summary>Anchors the Baby to Samus and begins transferring drained energy to her.</summary>
    LatchOntoSamus,

    /// <summary>Raises Samus's energy toward the native full-health target while the Baby remains attached.</summary>
    HealSamusToFullHealth,

    /// <summary>Holds the attachment until the Baby's transfer reserve reaches zero.</summary>
    IdleUntilNoHealth,

    /// <summary>Detaches the depleted Baby from Samus and restores her post-heal state.</summary>
    ReleaseSamus,

    /// <summary>Turns the Baby toward Mother Brain for the pause before its return attack.</summary>
    StareDownMotherBrain,

    /// <summary>Moves the Baby beyond the visible playfield to set up the final charge.</summary>
    FlyOffScreen,

    /// <summary>Moves to the off-screen waypoint from which the final charge is launched.</summary>
    MoveToFinalChargeStart,

    /// <summary>Initializes the final-charge velocity, animation, and attack presentation.</summary>
    InitiateFinalCharge,

    /// <summary>Advances the charge toward Mother Brain until the fatal projectile collision edge.</summary>
    FinalCharge,

    /// <summary>Applies Mother Brain's killing blow and runs the Baby's impact-shake delay.</summary>
    TakeFinalBlow,

    /// <summary>Waits out the post-impact timer, then queues Samus's theme.</summary>
    PlaySamusTheme,

    /// <summary>Transfers the Hyper Beam setup to Samus while preparing the Baby's death fall.</summary>
    PrepareSamusForHyperBeam,

    /// <summary>Runs the falling, palette-fading death sequence and periodic dust explosions.</summary>
    DeathSequence,

    /// <summary>Removes the Baby's sprite tiles after its body leaves the active cutscene.</summary>
    UnloadTiles,

    /// <summary>Keeps Samus's restored rainbow palette cycling through the remaining scripted delay.</summary>
    LetSamusRainbowSomeMore,

    /// <summary>Hands control to the final Mother Brain combat/cutscene state after the Baby sequence completes.</summary>
    FinalCutscene,
}

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

/// <summary>Health/flash mutation produced by one colliding Mother Brain blue ring.</summary>
public readonly record struct BabyMetroidOnionRingHitResult();

/// <summary>Whole/subpixel coordinates before or after one cutscene-enemy main-AI call.</summary>
public readonly record struct BabyMetroidCutscenePoint();

/// <summary>One `$86:E509` dust explosion requested during the Baby's death.</summary>
public readonly record struct BabyMetroidDeathExplosionRequest(
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter,
    ushort SoundEffect);

/// <summary>
/// One of the three parameter-nine `$86:E509` dust clouds emitted by
/// <c>$A9:C98C-C9C2</c> when the Baby releases Mother Brain's head.
/// </summary>
public readonly record struct BabyMetroidReleaseDustRequest(
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter);

/// <summary>One fourteen-colour write from `$AD:E90C-$E998` to sprite palette seven.</summary>
public readonly record struct BabyMetroidPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort DestinationColorIndex,
    ushort ColorCount);

/// <summary>One phase-three room-light palette pair selected by `$AD:F24B`.</summary>
public readonly record struct MotherBrainBackgroundPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort FirstDestinationColorIndex,
    ushort SecondDestinationColorIndex,
    ushort ColorsPerDestination);

/// <summary>One-call debugger witness for the Baby entrance and latch chain.</summary>
public readonly record struct BabyMetroidCutsceneStepResult(
    BabyMetroidCutscenePhase PhaseBefore,
    BabyMetroidCutscenePhase PhaseAfter,
    bool BodyStumbleRequested,
    bool LatchSoundQueued,
    IReadOnlyList<BabyMetroidReleaseDustRequest> ReleaseDustClouds,
    bool AmbientCrySoundQueued,
    BabyMetroidDeathExplosionRequest? DeathExplosion,
    BabyMetroidPaletteTransferRequest? BabyPaletteTransfer,
    MotherBrainSpriteTileTransferRequest? AttackTileTransfer,
    MotherBrainBackgroundPaletteTransferRequest? BackgroundPaletteTransfer)
{
    /// <summary>The one-call fatal-impact edge at $A9:CBF2-CC3D, before the ongoing shake phase.</summary>
    public bool FatalBlowStarted => PhaseBefore == BabyMetroidCutscenePhase.FinalCharge &&
        PhaseAfter == BabyMetroidCutscenePhase.TakeFinalBlow;

    /// <summary>The $A9:CC60 timer expiry queues the theme before entering Hyper Beam preparation.</summary>
    public bool SamusThemeStarted => PhaseBefore == BabyMetroidCutscenePhase.PlaySamusTheme &&
        PhaseAfter == BabyMetroidCutscenePhase.PrepareSamusForHyperBeam;
}
