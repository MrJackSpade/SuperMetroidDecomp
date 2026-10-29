namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's part functions, held in each part's <c>$0FA8</c> word and the <c>$7800</c> next-function alias. Each
/// member names the bank-$A7 routine it ports; the word stores the member, not the routine's address. Members are
/// declared in the cartridge's address order, which Kraid's death tests compare, and start at one.
/// </summary>
public enum KraidAiFunction
{
    /// <summary>A cleared record holds no function.</summary>
    None = 0,
    /// <summary>$A7:804B RTS: an explicit no-op function retaining the part slot.</summary>
    NoOperation = 1,
    /// <summary>$A7:AC4D: breaks ceiling platforms into their authored room mutations as Kraid grows.</summary>
    GrowBreakCeilingPlatforms,
    /// <summary>$A7:AD3A: sets Kraid's private BG2 tilemap priority bits during growth.</summary>
    GrowSetBg2Priority,
    /// <summary>$A7:AD61: completes the growth sequence's BG2 tilemap updates.</summary>
    GrowFinishBg2Update,
    /// <summary>$A7:AD8E: installs the room background behind the enlarged Kraid.</summary>
    GrowDrawRoomBackground,
    /// <summary>$A7:AE23: fades the room background into the enlarged encounter.</summary>
    GrowFadeInRoomBackground,
    /// <summary>$A7:AEA4: the first-phase body thinker counting down to its next mouth attack.</summary>
    MainloopThinking,
    /// <summary>$A7:AEC4: the enlarged body's second-phase thinking entry.</summary>
    SecondPhaseThinking,
    /// <summary>$A7:AEE4: handles the shot/open-mouth reaction and pending mouth-reopen count.</summary>
    MouthOpenReaction,
    /// <summary>$A7:B6BF: initializes the collision-driven eye glow before another mouth opening.</summary>
    InitializeEyeGlow,
    /// <summary>$A7:B6D7: advances the glowing-eye palette sequence.</summary>
    GlowEye,
    /// <summary>$A7:B73D: restores the eye palette after its glow sequence.</summary>
    UnglowEye,
    /// <summary>$A7:B831: the inactive lint entry before projectile production begins.</summary>
    LintInactive,
    /// <summary>$A7:B832: produces the lint and aligns its starting position to Kraid.</summary>
    LintProduce,
    /// <summary>$A7:B868: advances the lint's charge preparation before firing.</summary>
    LintCharge,
    /// <summary>$A7:B89B: moves the fired lint away from Kraid.</summary>
    LintFire,
    /// <summary>$A7:B907: waits until the top lint's X position reaches at least $100 before firing a nail.</summary>
    FingernailWaitForLint,
    /// <summary>$A7:B923: horizontally aligns the current physical part to Kraid's body.</summary>
    AlignPartToKraid,
    /// <summary>$A7:B92D: decrements the current part's function timer and installs its saved next function on expiry.</summary>
    HandleFunctionTimer,
    /// <summary>$A7:B93F: the first-phase foot's start-retreat timer entry.</summary>
    DecrementFunctionTimerAndStartWalk,
    /// <summary>$A7:B960: the first-phase foot's thinking entry before its independently timed lunge.</summary>
    FootFirstPhaseThinking,
    /// <summary>$A7:B965: processes Kraid's private head instruction stream and its timer.</summary>
    ProcessHeadInstructionAndTimer,
    /// <summary>$A7:BA2E: counts the second-phase foot's think delay and selects a walking target.</summary>
    FootSecondPhaseThinking,
    /// <summary>$A7:BB45: the native backward-walk entry moving the body toward its rightward target.</summary>
    FootSecondPhaseWalkingRight,
    /// <summary>$A7:BB6E: walks to the second-phase starting point before its setup delay.</summary>
    FootSecondPhaseWalkToStart,
    /// <summary>$A7:BBA4: initializes second-phase foot thinking after the starting-point walk.</summary>
    FootSecondPhaseInitialize,
    /// <summary>$A7:BBAE: the native forward-walk entry moving the body toward its leftward target.</summary>
    FootSecondPhaseWalkingLeft,
    /// <summary>$A7:BBEA: runs the open-mouth head stream, spat-rock cadence, and reopen continuation.</summary>
    MainAttackWithMouthOpen,
    /// <summary>$A7:BD60: selects the fingernail's starting side, animation, and firing transition.</summary>
    FingernailInitialize,
    /// <summary>$A7:BE8E: advances the fingernail's independent firing instruction stream.</summary>
    FingernailFire,
    /// <summary>$A7:BF2D: prepares the first-phase foot's forward lunge.</summary>
    FootPrepareFirstPhaseLunge,
    /// <summary>$A7:BF5D: advances the first-phase forward lunge before retreat.</summary>
    FootFirstPhaseLunge,
    /// <summary>$A7:BFAB: retreats from the first-phase lunge and restores the foot's thinking cycle.</summary>
    FootFirstPhaseRetreat,
    /// <summary>$A7:C0A1: releases the room camera restrictions for the enlarged second phase.</summary>
    GrowReleaseCamera,
    /// <summary>$A7:C360: initializes Kraid's zero-health sequence and the authored part shutdowns.</summary>
    DeathInitialize,
    /// <summary>$A7:C3F9: fades the room background out while continuing the private head stream.</summary>
    DeathFadeOut,
    /// <summary>$A7:C4A4: updates the top BG2 half and parks the lint parts for sinking.</summary>
    DeathUpdateTopTilemap,
    /// <summary>$A7:C4C8: updates the bottom BG2 half and starts the death quake and sinking sound timer.</summary>
    DeathUpdateBottomTilemap,
    /// <summary>$A7:C537: sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    DeathSink,
    /// <summary>$A7:C715: clears the top private BG2 half before ordinary room background restoration.</summary>
    DeathClearTopTilemap,
    /// <summary>$A7:C751: clears the bottom private BG2 half before the death's four-update wait.</summary>
    DeathClearBottomTilemap,
    /// <summary>$A7:C777: the first of the four updates Kraid's death waits before fading the room back in.</summary>
    DeathWaitBeforeFadeIn1,
    /// <summary>$A7:C7A3: the second update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn2,
    /// <summary>$A7:C7C9: the third update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn3,
    /// <summary>$A7:C7EF: the last update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn4,
    /// <summary>$A7:C815: fades the ordinary background in, requests room music, and publishes boss persistence when newly defeated.</summary>
    DeathFadeInBackground,
    /// <summary>$A7:C843: the terminal death entry for a Kraid that was alive when this encounter loaded.</summary>
    DeathFinishedWasAlive,
    /// <summary>$A7:C851: the already-defeated terminal entry, also invalidating BG1 column-streaming position.</summary>
    DeathFinishedWasDead,
    /// <summary>$A7:C865: restricts Samus to the room's first screen while waiting for the rise.</summary>
    RestrictSamusToFirstScreen,
    /// <summary>$A7:C86B: installs the top BG2 tilemap half at the start of Kraid's rise.</summary>
    RaiseKraidThroughFloor,
    /// <summary>$A7:C89A: installs the bottom BG2 half, starts the quake, and requests boss music.</summary>
    RaiseLoadBottomTilemap,
    /// <summary>$A7:C8E0: requests rise rocks on sixteen-frame timer boundaries during the first pre-rise wait.</summary>
    RaiseRocksEvery16Frames,
    /// <summary>$A7:C902: requests rise rocks on eight-frame timer boundaries during the faster pre-rise wait.</summary>
    RaiseRocksEvery8Frames,
    /// <summary>$A7:C924: raises Kraid at half a pixel per update with horizontal shaking until first-phase combat starts.</summary>
    RaiseBody,
}
