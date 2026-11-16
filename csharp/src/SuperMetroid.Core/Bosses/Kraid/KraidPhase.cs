namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The phase each of Kraid's parts runs, held in each part's phase and the phase it resumes after its timer. Each
/// member names the behaviour it runs; the part stores the member.
/// </summary>
public enum KraidPhase
{
    /// <summary>
    /// The part rests, doing nothing on its turn: the arm while it rises, the foot between its walks and once Kraid grows
    /// or dies (#1870).
    /// </summary>
    Resting,
    /// <summary>Breaks ceiling platforms into their authored room mutations as Kraid grows.</summary>
    GrowBreakCeilingPlatforms,
    /// <summary>Brings Kraid's grown body in front of the room's sprites, shows it whole and lets his arm touch Samus.</summary>
    GrowBringBodyInFront,
    /// <summary>Settles his grown parts: his foot stands neutral and his lints take their grown pose.</summary>
    GrowSettleParts,
    /// <summary>Shows the room's backdrop behind the grown Kraid and starts its fade in from the first step.</summary>
    GrowShowRoomBackdrop,
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
    /// <summary>Advances the glowing-eye colour sequence.</summary>
    GlowEye,
    /// <summary>Restores the eye colours after its glow sequence.</summary>
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
    /// <summary>
    /// The walk moving the body toward its target its foot's way (<see cref="KraidFootState.WalkDirection"/>): backward
    /// toward a rightward target, forward toward a leftward one.
    /// </summary>
    FootSecondPhaseWalking,
    /// <summary>Walks to the second-phase starting point before its setup delay.</summary>
    FootSecondPhaseWalkToStart,
    /// <summary>Initializes second-phase foot thinking after the starting-point walk.</summary>
    FootSecondPhaseInitialize,
    /// <summary>Runs the open-mouth head animation, spat-rock cadence, and reopen continuation.</summary>
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
    /// <summary>Fades the room background out while continuing the head animation.</summary>
    DeathFadeOut,
    /// <summary>Shows his body whole and parks his lints for sinking.</summary>
    DeathShowBody,
    /// <summary>Starts the death quake and the sinking sound's countdown, makes his body solid and sets his arm sinking.</summary>
    DeathStartSinking,
    /// <summary>Sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    DeathSink,
    /// <summary>Hides his body once he has sunk, before the room's backdrop is restored.</summary>
    DeathHideBody,
    /// <summary>
    /// Waits the updates his death keeps the room dark before fading it back in (#1888), counted down in the body's phase
    /// timer.
    /// </summary>
    DeathWaitBeforeFadeIn,
    /// <summary>Fades the ordinary background in, requests room music, and publishes boss persistence when newly defeated.</summary>
    DeathFadeInBackground,
    /// <summary>The terminal death entry for a Kraid that was alive when this encounter loaded.</summary>
    DeathFinishedWasAlive,
    /// <summary>The terminal death entry for a Kraid that was already defeated when this encounter loaded.</summary>
    DeathFinishedWasDead,
    /// <summary>Restricts Samus to the room's first screen while waiting for the rise.</summary>
    RestrictSamusToFirstScreen,
    /// <summary>Shows his body whole at the start of Kraid's rise through the floor.</summary>
    RaiseShowBody,
    /// <summary>Starts the rise's quake and its first rock wait, and requests the boss music.</summary>
    RaiseStartQuakeAndMusic,
    /// <summary>Requests rise rocks on sixteen-frame timer boundaries during the first pre-rise wait.</summary>
    RaiseRocksEvery16Frames,
    /// <summary>Requests rise rocks on eight-frame timer boundaries during the faster pre-rise wait.</summary>
    RaiseRocksEvery8Frames,
    /// <summary>Raises Kraid at half a pixel per update with horizontal shaking until first-phase combat starts.</summary>
    RaiseBody,
}
