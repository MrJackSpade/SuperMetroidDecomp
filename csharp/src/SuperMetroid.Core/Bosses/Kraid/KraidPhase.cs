namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phase each of Kraid's parts runs, held in each part's phase and the phase it resumes after its timer. Each
/// member names the behaviour it runs; the part stores the member, and a part with no phase runs nothing.
/// </summary>
public enum KraidPhase
{
    /// <summary>Breaks ceiling platforms into their authored room mutations as Kraid grows.</summary>
    GrowBreakCeilingPlatforms,
    /// <summary>Brings Kraid's grown body in front of the room's sprites and shows its upper half.</summary>
    GrowBringBodyInFront,
    /// <summary>Shows the lower half of his body in front of the sprites and settles his grown parts.</summary>
    GrowShowLowerBodyInFront,
    /// <summary>Installs the room background behind the enlarged Kraid.</summary>
    GrowDrawRoomBackground,
    /// <summary>Fades the room background into the enlarged encounter.</summary>
    GrowFadeInRoomBackground,
    /// <summary>The first-phase body thinker counting down to its next mouth attack.</summary>
    MainloopThinking,
    /// <summary>The enlarged body's second-phase thinking entry.</summary>
    SecondPhaseThinking,
    /// <summary>Handles the shot/open-mouth reaction and pending mouth-reopen count.</summary>
    MouthOpenReaction,
    /// <summary>Initializes the collision-driven eye glow before another mouth opening.</summary>
    InitializeEyeGlow,
    /// <summary>Advances the glowing-eye palette sequence.</summary>
    GlowEye,
    /// <summary>Restores the eye palette after its glow sequence.</summary>
    UnglowEye,
    /// <summary>The inactive lint entry before projectile production begins.</summary>
    LintInactive,
    /// <summary>Produces the lint and aligns its starting position to Kraid.</summary>
    LintProduce,
    /// <summary>Advances the lint's charge preparation before firing.</summary>
    LintCharge,
    /// <summary>Moves the fired lint away from Kraid.</summary>
    LintFire,
    /// <summary>Waits until the top lint's X position reaches at least 256 before firing a nail.</summary>
    FingernailWaitForLint,
    /// <summary>Horizontally aligns the current physical part to Kraid's body.</summary>
    AlignPartToKraid,
    /// <summary>Decrements the current part's phase timer and resumes its saved phase on expiry.</summary>
    WaitToResumePhase,
    /// <summary>The first-phase foot's start-retreat timer entry.</summary>
    FootWaitBeforeWalk,
    /// <summary>The first-phase foot's thinking entry before its independently timed lunge.</summary>
    FootFirstPhaseThinking,
    /// <summary>Processes Kraid's private head animation and its timer.</summary>
    AdvanceHeadAnimationAndTimer,
    /// <summary>Counts the second-phase foot's think delay and selects a walking target.</summary>
    FootSecondPhaseThinking,
    /// <summary>The backward-walk entry moving the body toward its rightward target.</summary>
    FootSecondPhaseWalkingRight,
    /// <summary>Walks to the second-phase starting point before its setup delay.</summary>
    FootSecondPhaseWalkToStart,
    /// <summary>Initializes second-phase foot thinking after the starting-point walk.</summary>
    FootSecondPhaseInitialize,
    /// <summary>The forward-walk entry moving the body toward its leftward target.</summary>
    FootSecondPhaseWalkingLeft,
    /// <summary>Runs the open-mouth head stream, spat-rock cadence, and reopen continuation.</summary>
    MainAttackWithMouthOpen,
    /// <summary>Selects the fingernail's starting side, animation, and firing transition.</summary>
    FingernailInitialize,
    /// <summary>Advances the fingernail's independent firing animation.</summary>
    FingernailFire,
    /// <summary>Prepares the first-phase foot's forward lunge.</summary>
    FootPrepareFirstPhaseLunge,
    /// <summary>Advances the first-phase forward lunge before retreat.</summary>
    FootFirstPhaseLunge,
    /// <summary>Retreats from the first-phase lunge and restores the foot's thinking cycle.</summary>
    FootFirstPhaseRetreat,
    /// <summary>Releases the room camera restrictions for the enlarged second phase.</summary>
    GrowReleaseCamera,
    /// <summary>Initializes Kraid's zero-health sequence and the authored part shutdowns.</summary>
    DeathInitialize,
    /// <summary>Fades the room background out while continuing the private head stream.</summary>
    DeathFadeOut,
    /// <summary>Shows the upper half of his body, covering his head, and parks the lint parts for sinking.</summary>
    DeathShowUpperBody,
    /// <summary>Shows the lower half of his body and starts the death quake and sinking sound timer.</summary>
    DeathShowLowerBody,
    /// <summary>Sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    DeathSink,
    /// <summary>Hides the upper half of his body before the room's backdrop is restored.</summary>
    DeathHideUpperBody,
    /// <summary>Hides the lower half of his body before the death's four-update wait.</summary>
    DeathHideLowerBody,
    /// <summary>The first of the four updates Kraid's death waits before fading the room back in.</summary>
    DeathWaitBeforeFadeIn1,
    /// <summary>The second update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn2,
    /// <summary>The third update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn3,
    /// <summary>The last update of the wait before the fade-in.</summary>
    DeathWaitBeforeFadeIn4,
    /// <summary>Fades the ordinary background in, requests room music, and publishes boss persistence when newly defeated.</summary>
    DeathFadeInBackground,
    /// <summary>The terminal death entry for a Kraid that was alive when this encounter loaded.</summary>
    DeathFinishedWasAlive,
    /// <summary>The already-defeated terminal entry, also invalidating the foreground's column-streaming position.</summary>
    DeathFinishedWasDead,
    /// <summary>Restricts Samus to the room's first screen while waiting for the rise.</summary>
    RestrictSamusToFirstScreen,
    /// <summary>Shows the upper half of his body at the start of Kraid's rise.</summary>
    RaiseKraidThroughFloor,
    /// <summary>Shows the lower half of his body, starts the quake, and requests boss music.</summary>
    RaiseShowLowerBody,
    /// <summary>Requests rise rocks on sixteen-frame timer boundaries during the first pre-rise wait.</summary>
    RaiseRocksEvery16Frames,
    /// <summary>Requests rise rocks on eight-frame timer boundaries during the faster pre-rise wait.</summary>
    RaiseRocksEvery8Frames,
    /// <summary>Raises Kraid at half a pixel per update with horizontal shaking until first-phase combat starts.</summary>
    RaiseBody,
}
