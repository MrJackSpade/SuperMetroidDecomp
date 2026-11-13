namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The phases Mother Brain's body dispatcher runs; unrelated phases are not interchangeable.
/// Its values are arbitrary; verification's catalog relates each member to its source (#1413).
/// </summary>
public enum MotherBrainBodyFunction
{
    /// <summary>Observes the tank-bound head's health and hands zero-health defeat to the fake-death descent.</summary>
    FirstPhase = 1,
    /// <summary>Seeds a 64-count wait and immediately falls into its first decrement.</summary>
    FakeDeathDescentInitialPause,
    /// <summary>Waits for signed timer underflow, then locks Samus and closes scroll region 1.</summary>
    FakeDeathDescentPauseBeforeLock,
    /// <summary>Expires the 32-count pause, queues music stop/data load, and starts the 12-count unlock wait.</summary>
    FakeDeathDescentPauseBeforeMusic,
    /// <summary>Releases Samus input after the music pause and begins the eight-count screen-flash wait.</summary>
    FakeDeathDescentPauseBeforeUnlock,
    /// <summary>Starts room flashing, selects lowered-acid FX, clears the ceiling block, and arms tube collapse.</summary>
    FakeDeathDescentPauseBeforeFlash,
    /// <summary>Advances the fake-death grey palette on timer underflow while also running tube collapse and explosions.</summary>
    FakeDeathDescentFadeToGray,
    /// <summary>Runs the independent tube sequence and dust/explosion cadence until the falling main tube starts ascent.</summary>
    FakeDeathDescentCollapseTubes,
    /// <summary>Requests the first two rows of the mural behind her tank, then selects the next pair for the following body update.</summary>
    FakeDeathAscentDrawMuralRows1And2,
    /// <summary>Requests the mural's third and fourth rows and advances to the fifth and sixth.</summary>
    FakeDeathAscentDrawMuralRows3And4,
    /// <summary>Requests the mural's fifth and sixth rows and advances to the seventh and eighth.</summary>
    FakeDeathAscentDrawMuralRows5And6,
    /// <summary>Requests the mural's seventh and eighth rows and advances to the ninth and tenth.</summary>
    FakeDeathAscentDrawMuralRows7And8,
    /// <summary>Requests the mural's ninth and tenth rows and advances to the last two.</summary>
    FakeDeathAscentDrawMuralRows9And10,
    /// <summary>Requests the mural's last two rows and hands off to phase-two graphics setup.</summary>
    FakeDeathAscentDrawMuralRows11And12,
    /// <summary>Shows her phase-two attack and rear-leg colours and shows the prepared enemy background.</summary>
    FakeDeathAscentSetupPhase2Graphics,
    /// <summary>Configures articulated drawing and blending, makes head/body tangible, sets head health 18,000, and immediately starts the 128-count suspense wait.</summary>
    FakeDeathAscentSetupPhase2Brain,
    /// <summary>Waits for signed underflow, then seeds the 32-count rising-preparation delay and falls into it.</summary>
    FakeDeathAscentPauseForSuspense,
    /// <summary>Starts the floor-clipping effect, installs the hidden head list, and starts readying her standing body with a 256-count later pause.</summary>
    FakeDeathAscentPrepareForRising,
    /// <summary>Readies her standing body one update per call and decrements the suspense timer on the same call that finishes it.</summary>
    FakeDeathAscentReadyStandingBody,
    /// <summary>Waits for the 256-count timer's signed underflow before placing the hidden body/background and enabling its hitboxes.</summary>
    FakeDeathAscentContinuePausing,
    /// <summary>Reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    FakeDeathAscentStartMusicAndEarthquake,
    /// <summary>Every fourth frame of the frame counter raises the body two pixels with dust and matching background scroll, then ends the floor-clipping effect and starts uncrouching.</summary>
    FakeDeathAscentRaiseMotherBrain,
    /// <summary>Waits for the body program to publish the standing pose before initializing grey-palette restoration.</summary>
    FakeDeathAscentWaitUntilUncrouched,
    /// <summary>Restores brain sprite colors on the four-count timer cadence, then enables phase-two palette handling and drool.</summary>
    FakeDeathAscentTransitionFromGray,
    /// <summary>Expires the 23-count menacing shake and installs the stretching head list with a 256-count hold.</summary>
    SecondPhaseStretchingShakeHead,
    /// <summary>Expires the long stretch hold, changes both neck targets, and begins the 64-count final wait.</summary>
    SecondPhaseStretchingBringHeadUp,
    /// <summary>Completes the final wait, enables small purple breath, and enters ordinary phase-two thinking.</summary>
    SecondPhaseStretchingFinish,
    /// <summary>Zero head health starts the rainbow sequence; otherwise standing pose, health, and the current RNG choose walking, ordinary attacks, or the red hand beam.</summary>
    SecondPhaseThinking,
    /// <summary>Runs the choose/cooldown/end sub-dispatcher, selecting a head program or a bomb/laser body state from Samus movement and sampled RNG.</summary>
    SecondPhaseTryAttack,
    /// <summary>Samples an optional backward target, then advances RNG for the independent crouch-or-fire choice.</summary>
    SecondPhaseBombDecideWalking,
    /// <summary>Waits for the stored backward target before choosing crouch or immediate bomb firing.</summary>
    SecondPhaseBombWalkingBackwards,
    /// <summary>Requests the slow crouch and waits for its pose before installing the bomb-firing head list.</summary>
    SecondPhaseBombCrouch,
    /// <summary>Holds the 44-count post-fire pause, then attempts stand-up with the same-call fallthrough.</summary>
    SecondPhaseBombFired,
    /// <summary>Retries the posture helper until standing and returns to phase-two thinking.</summary>
    SecondPhaseBombStandUp,
    /// <summary>Chooses neck targets from head/Samus Y ordering and seeds a four-count quick-positioning wait.</summary>
    SecondPhaseLaserPositionHeadQuickly,
    /// <summary>On timer underflow slows neck rotation, installs the laser head program, and seeds a 16-count recovery wait.</summary>
    SecondPhaseLaserPositionHeadSlowlyAndFire,
    /// <summary>Restores neutral neck targets after the wait and immediately re-enters thinking without the ordinary attack sub-dispatcher's end phase.</summary>
    SecondPhaseLaserFinishAttack,
    /// <summary>Owns the ordinary red hand-beam attack sub-dispatcher, separate from the health-zero rainbow cutscene.</summary>
    SecondPhaseHandBeam,
    /// <summary>Sets the initial extended neck geometry and installs the 256-count charging-start pause.</summary>
    SecondPhaseRainbowExtendNeck,
    /// <summary>Expires the extension pause, installs the charging head program, and falls into backward-walk/neck retraction.</summary>
    SecondPhaseRainbowStartCharging,
    /// <summary>Requests really-slow backward walking toward X 40, then retracts the head and starts a 256-count charge wait.</summary>
    SecondPhaseRainbowRetractNeck,
    /// <summary>Waits for signed timer underflow, queues the charge sound, and falls into downward neck extension.</summary>
    SecondPhaseRainbowWaitForCharge,
    /// <summary>Sets downward neck targets and projectile cooldown, seeds the NTSC 16-count firing delay, and immediately runs its first call.</summary>
    SecondPhaseRainbowExtendNeckDown,
    /// <summary>Widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    SecondPhaseRainbowStartFiring,
    /// <summary>Maintains beam effects and forced wallward movement until Samus reaches the wall, which selects the one-call delay.</summary>
    SecondPhaseRainbowMoveSamusTowardWall,
    /// <summary>Runs another beam/movement call, underflows the zero timer, and starts an eight-count earthquake before draining setup.</summary>
    SecondPhaseRainbowOneFrameDelay,
    /// <summary>Seeds both drain and earthquake timers with 299 and immediately executes the first draining call.</summary>
    SecondPhaseRainbowStartDrainingSamus,
    /// <summary>Applies beam damage, ammunition drain, forced wall-centering movement, and effects until timer underflow.</summary>
    SecondPhaseRainbowDrainingSamus,
    /// <summary>Narrows the beam to an angular width of an eighth of a turn, stops the beam effect, sound and earthquake, releases input, and starts the falling handoff.</summary>
    SecondPhaseRainbowFinishFiring,
    /// <summary>Changes the drained Samus controller/pose and falls directly into the first custom falling movement call.</summary>
    SecondPhaseRainbowLetSamusFall,
    /// <summary>Advances forced falling until Samus lands.</summary>
    SecondPhaseRainbowWaitForSamusToLand,
    /// <summary>Lowers the neck targets and installs the 128-count next-action pause.</summary>
    SecondPhaseRainbowLowerHead,
    /// <summary>After timer underflow, repeats the beam at health 400 or above; otherwise begins the low-health finish-off cutscene.</summary>
    SecondPhaseRainbowDecideNextAction,
    /// <summary>Uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    SecondPhaseFinishSamusOff,
    /// <summary>Waits for standing, then immediately enters the 16-count admiration pause.</summary>
    SecondPhaseFinishSamusOffStandUp,
    /// <summary>Expires the admiration pause, installs the stretching head list, and starts a 256-count final-charge delay.</summary>
    SecondPhaseFinishSamusOffAdmire,
    /// <summary>On timer underflow installs the charging head program and immediately begins readying the Baby Metroid's arrival.</summary>
    SecondPhaseFinishSamusOffChargeFinalBeam,
    /// <summary>Readies the Baby Metroid's arrival one update per call; the last retracts the head, spawns the baby, and seeds a 256-count firing wait.</summary>
    SecondPhaseFinishSamusOffReadyBaby,
    /// <summary>Expires the final wait, starts the firing head program, neck extension and its sound, then installs its own empty step as the holding function.</summary>
    SecondPhaseFinishSamusOffFireFinalBeam,
    /// <summary>Inert step that waits until the independently running baby latches on and replaces the body function.</summary>
    SecondPhaseFinalRainbowBeamHolding,
    /// <summary>The body's next turn after baby attachment changes form/neck motion and immediately starts the 48-count regain-balance pause.</summary>
    SecondPhaseDrainedByBabyTakenAback,
    /// <summary>Maintains the rainbow palette until timer underflow, then initializes the independent painful-walking sequence.</summary>
    SecondPhaseDrainedByBabyRegainBalance,
    /// <summary>Refreshes head shake, applies rainbow palettes, and advances painful walking until stage 6 disables beam sound and brain palette handling.</summary>
    SecondPhaseDrainedByBabyFiringRainbowBeam,
    /// <summary>Continues painful walking through stage 8, then selects dying drool and the retreating neck posture.</summary>
    SecondPhaseDrainedByBabyRainbowBeamRunOut,
    /// <summary>Requests really-slow backward walking to X 40 before disabling upper-neck motion and entering low-power setup.</summary>
    SecondPhaseDrainedByBabyMoveToBackOfRoom,
    /// <summary>Waits for both neck movement indices to clear, disables drool, and requests fast crouching once standing.</summary>
    SecondPhaseDrainedByBabyGoIntoLowPowerMode,
    /// <summary>Expires a 64-count crouch pause, clears the palette counter, and immediately begins the 16-count grey-transition cadence.</summary>
    SecondPhaseDrainedByBabyPrepareTransitionToGrey,
    /// <summary>Steps through eight drained-grey palettes and one step past the last, restores head health 36,000, and enters the inanimate revival setup.</summary>
    SecondPhaseDrainedByBabyTransitionToGrey,
    /// <summary>Installs the 768-count revival wait and returns without decrementing it on this setup call.</summary>
    SecondPhaseReviveInanimateGrey,
    /// <summary>On signed wait underflow re-enables breath/drool, clears the palette counter, and starts the 224-count color-restoration delay.</summary>
    SecondPhaseReviveShowSignsOfLife,
    /// <summary>Reverses the eight drained-grey palettes on timer underflow, then re-enables brain palette handling and falls into wake-up.</summary>
    SecondPhaseReviveTransitionFromGrey,
    /// <summary>Waits for the posture helper to observe standing, then extends the neck and immediately begins a 16-count wake-up stretch.</summary>
    SecondPhaseReviveWakeUp,
    /// <summary>Expires the short stretch delay, installs the phase-three stretching head program, and seeds a 128-count walking delay.</summary>
    SecondPhaseReviveWakeUpStretch,
    /// <summary>After the wait repeatedly requests forward walking until X passes 80, then enables health-based palettes and prepares the neck.</summary>
    SecondPhaseReviveWalkUpToBaby,
    /// <summary>Clears the baby attack counter and configures both neck targets before falling into the stand-up handshake.</summary>
    SecondPhaseRevivePrepareNeckForBabyDeath,
    /// <summary>Waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    SecondPhaseReviveFinishPreparingForBabyDeath,
    /// <summary>Sampled RNG may adjust posture or fire the targeted onion-ring head list, using Samus's list when no baby index exists.</summary>
    SecondPhaseMurderBabyAttack,
    /// <summary>Waits for the 64-count timer's signed underflow before returning to baby-attack selection.</summary>
    SecondPhaseMurderBabyAttackCooldown,
    /// <summary>Repeatedly requests standing and backward walking to X 64; the baby owns replacement of this body function.</summary>
    SecondPhasePrepareForFinalBabyAttack,
    /// <summary>Installs the final targeted head volley and selects the following inert step.</summary>
    SecondPhaseExecuteFinalBabyAttack,
    /// <summary>Holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    SecondPhaseFinalBabyAttackHolding,
    /// <summary>Changes body form to 4, requests a fourteen-pixel backward target, and seeds the 32-count combat-setup wait.</summary>
    ThirdPhaseRecoverMakeSomeDistance,
    /// <summary>On timer underflow installs the normal neck and walking sub-handlers and immediately runs the first phase-three fighting call.</summary>
    ThirdPhaseRecoverSetupForFighting,
    /// <summary>Checks head death before neck/walking work, then uses standing pose, attack admission, and sampled RNG to choose bombs or four onion rings.</summary>
    ThirdPhaseFightingMain,
    /// <summary>Pauses the neck/walking sub-handlers for the 64-count timer through zero and resumes main fighting only after signed underflow.</summary>
    ThirdPhaseFightingAttackCooldown,
    /// <summary>Phase-three death/escape: moves to the back of the room.</summary>
    ThirdPhaseDeathMoveToBackOfRoom,
    /// <summary>Phase-three death/escape: idles while exploding.</summary>
    ThirdPhaseDeathIdleWhilstExploding,
    /// <summary>Phase-three death/escape: stumbles to the middle of the room.</summary>
    ThirdPhaseDeathStumbleToMiddleOfRoom,
    /// <summary>Phase-three death/escape: disables the brain's effects.</summary>
    ThirdPhaseDeathDisableBrainEffects,
    /// <summary>Phase-three death/escape: sets up the body's fade-out.</summary>
    ThirdPhaseDeathSetupBodyFadeOut,
    /// <summary>Phase-three death/escape: fades out the body.</summary>
    ThirdPhaseDeathFadeOutBody,
    /// <summary>Phase-three death/escape: the final few explosions.</summary>
    ThirdPhaseDeathFinalFewExplosions,
    /// <summary>Phase-three death/escape: the brain realizes its decapitation.</summary>
    ThirdPhaseDeathRealizeDecapitation,
    /// <summary>Phase-three death/escape: the brain falls to the ground.</summary>
    ThirdPhaseDeathBrainFallsToGround,
    /// <summary>Phase-three death/escape: readies her corpse one update per call.</summary>
    ThirdPhaseDeathReadyCorpse,
    /// <summary>Phase-three death/escape: sets up the fade to grey.</summary>
    ThirdPhaseDeathSetupFadeToGrey,
    /// <summary>Phase-three death/escape: fades to grey.</summary>
    ThirdPhaseDeathFadeToGrey,
    /// <summary>Phase-three death/escape: the corpse tips over.</summary>
    ThirdPhaseDeathCorpseTipsOver,
    /// <summary>Phase-three death/escape: the corpse rots away.</summary>
    ThirdPhaseDeathCorpseRotsAway,
    /// <summary>Phase-three death/escape: waits twenty frames.</summary>
    ThirdPhaseDeath20FrameDelay,
    /// <summary>Phase-three death/escape: readies the escape timer and message one update per call.</summary>
    ThirdPhaseDeathReadyEscapeWarning,
    /// <summary>Phase-three death/escape: starts the escape.</summary>
    ThirdPhaseDeathStartEscape,
    /// <summary>Phase-three death/escape: spawns the time bomb and sets its subtitle.</summary>
    ThirdPhaseDeathSpawnTimeBombSetSubtitle,
    /// <summary>Phase-three death/escape: types out the Zebes escape text.</summary>
    ThirdPhaseDeathTypeOutZebesEscapeText,
    /// <summary>Phase-three death/escape: the door explodes and the timer starts.</summary>
    ThirdPhaseDeathDoorExplodingStartTimer,
    /// <summary>Phase-three death/escape: blows up the escape door.</summary>
    ThirdPhaseDeathBlowUpEscapeDoor,
    /// <summary>Phase-three death/escape: keeps the earthquake going.</summary>
    ThirdPhaseDeathKeepEarthquakeGoing,
}
