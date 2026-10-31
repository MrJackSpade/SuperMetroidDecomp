namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The phases Mother Brain's body dispatcher runs, one per cartridge routine; unrelated phases are not interchangeable.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1413).
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
    /// <summary>Requests BG1 room-mutation PLMs for rows 2/3, then selects the next pair for the following body update.</summary>
    FakeDeathAscentDrawRows2And3,
    /// <summary>Requests the row-4/5 BG1 mutation pair and advances to rows 6/7.</summary>
    FakeDeathAscentDrawRows4And5,
    /// <summary>Requests the row-6/7 BG1 mutation pair and advances to rows 8/9.</summary>
    FakeDeathAscentDrawRows6And7,
    /// <summary>Requests the row-8/9 BG1 mutation pair and advances to rows $A/$B.</summary>
    FakeDeathAscentDrawRows8And9,
    /// <summary>Requests the row-$A/$B BG1 mutation pair and advances to rows $C/$D.</summary>
    FakeDeathAscentDrawRowsAAndB,
    /// <summary>Requests the final BG1 row pair and hands off to phase-two graphics setup.</summary>
    FakeDeathAscentDrawRowsCAndD,
    /// <summary>Installs attack/back-leg colors, enables the unpause hook, and transfers the prepared enemy BG2 tilemap.</summary>
    FakeDeathAscentSetupPhase2Graphics,
    /// <summary>Configures articulated drawing and blending, makes head/body tangible, sets head health $4650, and immediately starts the $80-count suspense wait.</summary>
    FakeDeathAscentSetupPhase2Brain,
    /// <summary>Waits for signed underflow, then seeds the $20-count rising-preparation delay and falls into it.</summary>
    FakeDeathAscentPauseForSuspense,
    /// <summary>Starts the floor-clipping effect, installs the hidden head list, and starts leg-tile transfer with a $100-count later pause.</summary>
    FakeDeathAscentPrepareForRising,
    /// <summary>Processes one leg-tile transfer per call and decrements the suspense timer on the same call that finishes the list.</summary>
    FakeDeathAscentLoadLegTiles,
    /// <summary>Waits for the $100 timer's signed underflow before placing the hidden body/BG2 and enabling its hitboxes.</summary>
    FakeDeathAscentContinuePausing,
    /// <summary>Reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    FakeDeathAscentStartMusicAndEarthquake,
    /// <summary>Every fourth frame of the frame counter raises the body two pixels with dust and matching BG2 scroll, then ends the floor-clipping effect and starts uncrouching.</summary>
    FakeDeathAscentRaiseMotherBrain,
    /// <summary>Waits for body bytecode to publish the standing pose before initializing grey-palette restoration.</summary>
    FakeDeathAscentWaitUntilUncrouched,
    /// <summary>Restores brain sprite colors on the four-count timer cadence, then enables phase-two palette handling and drool.</summary>
    FakeDeathAscentTransitionFromGray,
    /// <summary>Expires the $17-count menacing shake and installs the stretching head list with a $100-count hold.</summary>
    SecondPhaseStretchingShakeHead,
    /// <summary>Expires the long stretch hold, changes both neck targets, and begins the $40-count final wait.</summary>
    SecondPhaseStretchingBringHeadUp,
    /// <summary>Completes the final wait, enables small purple breath, and enters ordinary phase-two thinking.</summary>
    SecondPhaseStretchingFinish,
    /// <summary>Zero head health starts the rainbow sequence; otherwise standing pose, health, and the current RNG choose walking, ordinary attacks, or the red hand beam.</summary>
    SecondPhaseThinking,
    /// <summary>Runs the choose/cooldown/end sub-dispatcher, selecting head bytecode or a bomb/laser body state from Samus movement and sampled RNG.</summary>
    SecondPhaseTryAttack,
    /// <summary>Samples an optional backward target, then advances RNG for the independent crouch-or-fire choice.</summary>
    SecondPhaseBombDecideWalking,
    /// <summary>Waits for the stored backward target before choosing crouch or immediate bomb firing.</summary>
    SecondPhaseBombWalkingBackwards,
    /// <summary>Requests the slow crouch and waits for its pose before installing the bomb-firing head list.</summary>
    SecondPhaseBombCrouch,
    /// <summary>Holds the $2C-count post-fire pause, then attempts stand-up with the native same-call fallthrough.</summary>
    SecondPhaseBombFired,
    /// <summary>Retries the posture helper until standing and returns to phase-two thinking.</summary>
    SecondPhaseBombStandUp,
    /// <summary>Chooses neck targets from head/Samus Y ordering and seeds a four-count quick-positioning wait.</summary>
    SecondPhaseLaserPositionHeadQuickly,
    /// <summary>On timer underflow slows neck rotation, installs laser head bytecode, and seeds a $10-count recovery wait.</summary>
    SecondPhaseLaserPositionHeadSlowlyAndFire,
    /// <summary>Restores neutral neck targets after the wait and immediately re-enters thinking without the ordinary attack sub-dispatcher's end phase.</summary>
    SecondPhaseLaserFinishAttack,
    /// <summary>Owns the ordinary red hand-beam attack sub-dispatcher, separate from the health-zero rainbow cutscene.</summary>
    SecondPhaseHandBeam,
    /// <summary>Sets the initial extended neck geometry and installs the $100-count charging-start pause.</summary>
    SecondPhaseRainbowExtendNeck,
    /// <summary>Expires the extension pause, installs charging head bytecode, and falls into backward-walk/neck retraction.</summary>
    SecondPhaseRainbowStartCharging,
    /// <summary>Requests really-slow backward walking toward X $28, then retracts the head and starts a $100-count charge wait.</summary>
    SecondPhaseRainbowRetractNeck,
    /// <summary>Waits for signed timer underflow, queues charge sound $71, and falls into downward neck extension.</summary>
    SecondPhaseRainbowWaitForCharge,
    /// <summary>Sets downward neck targets and projectile cooldown, seeds the NTSC $10-count firing delay, and immediately runs its first call.</summary>
    SecondPhaseRainbowExtendNeckDown,
    /// <summary>Widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    SecondPhaseRainbowStartFiring,
    /// <summary>Maintains beam effects and forced wallward movement until its carry result selects the one-call delay.</summary>
    SecondPhaseRainbowMoveSamusTowardWall,
    /// <summary>Runs another beam/movement call, underflows the zero timer, and starts an eight-count earthquake before draining setup.</summary>
    SecondPhaseRainbowOneFrameDelay,
    /// <summary>Seeds both drain and earthquake timers with $012B and immediately executes the first draining call.</summary>
    SecondPhaseRainbowStartDrainingSamus,
    /// <summary>Applies beam damage, ammunition drain, forced wall-centering movement, and effects until timer underflow.</summary>
    SecondPhaseRainbowDrainingSamus,
    /// <summary>Narrows the beam to angular width $0200, stops the beam effect, sound and earthquake, releases input, and starts the falling handoff.</summary>
    SecondPhaseRainbowFinishFiring,
    /// <summary>Changes the drained Samus controller/pose and falls directly into the first custom falling movement call.</summary>
    SecondPhaseRainbowLetSamusFall,
    /// <summary>Advances forced falling until its native carry result reports landing.</summary>
    SecondPhaseRainbowWaitForSamusToLand,
    /// <summary>Lowers the neck targets and installs the $80-count next-action pause.</summary>
    SecondPhaseRainbowLowerHead,
    /// <summary>After timer underflow, repeats the beam at health $190 or above; otherwise begins the low-health finish-off cutscene.</summary>
    SecondPhaseRainbowDecideNextAction,
    /// <summary>Uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    SecondPhaseFinishSamusOff,
    /// <summary>Waits for standing, then immediately enters the $10-count admiration pause.</summary>
    SecondPhaseFinishSamusOffStandUp,
    /// <summary>Expires the admiration pause, installs the stretching head list, and starts a $100-count final-charge delay.</summary>
    SecondPhaseFinishSamusOffAdmire,
    /// <summary>On timer underflow installs charging head bytecode and immediately begins baby-Metroid tile loading.</summary>
    SecondPhaseFinishSamusOffChargeFinalBeam,
    /// <summary>Transfers one baby sprite-tile record per call; list completion retracts the head, spawns the baby, and seeds a $100-count firing wait.</summary>
    SecondPhaseFinishSamusOffLoadBabyTiles,
    /// <summary>Expires the final wait, starts firing head bytecode/neck extension and sound $71, then installs its own RTS as the holding function.</summary>
    SecondPhaseFinishSamusOffFireFinalBeam,
    /// <summary><c>$A9:BE1A Function_MBBody_Phase2_FinishSamusOff_FireFinalRainbowBeam.return</c>: inert RTS that waits until the independently running baby latches on and replaces the body function.</summary>
    SecondPhaseFinalRainbowBeamHolding,
    /// <summary>The body's next turn after baby attachment changes form/neck motion and immediately starts the $30-count regain-balance pause.</summary>
    SecondPhaseDrainedByBabyTakenAback,
    /// <summary>Maintains the rainbow palette until timer underflow, then initializes the independent painful-walking sequence.</summary>
    SecondPhaseDrainedByBabyRegainBalance,
    /// <summary>Refreshes head shake, applies rainbow palettes, and advances painful walking until stage 6 disables beam sound and brain palette handling.</summary>
    SecondPhaseDrainedByBabyFiringRainbowBeam,
    /// <summary>Continues painful walking through stage 8, then selects dying drool and the retreating neck posture.</summary>
    SecondPhaseDrainedByBabyRainbowBeamRunOut,
    /// <summary>Requests really-slow backward walking to X $28 before disabling upper-neck motion and entering low-power setup.</summary>
    SecondPhaseDrainedByBabyMoveToBackOfRoom,
    /// <summary>Waits for both neck movement indices to clear, disables drool, and requests fast crouching once standing.</summary>
    SecondPhaseDrainedByBabyGoIntoLowPowerMode,
    /// <summary>Expires a $40-count crouch pause, clears the palette counter, and immediately begins the $10-count grey-transition cadence.</summary>
    SecondPhaseDrainedByBabyPrepareTransitionToGrey,
    /// <summary>Steps eight drained-grey palettes plus the terminator, restores head health $8CA0, and enters the inanimate revival setup.</summary>
    SecondPhaseDrainedByBabyTransitionToGrey,
    /// <summary>Installs the $300-count revival wait and returns without decrementing it on this setup call.</summary>
    SecondPhaseReviveInanimateGrey,
    /// <summary>On signed wait underflow re-enables breath/drool, clears the palette counter, and starts the $E0-count color-restoration delay.</summary>
    SecondPhaseReviveShowSignsOfLife,
    /// <summary>Reverses the eight drained-grey palettes on timer underflow, then re-enables brain palette handling and falls into wake-up.</summary>
    SecondPhaseReviveTransitionFromGrey,
    /// <summary>Waits for the posture helper to observe standing, then extends the neck and immediately begins a $10-count wake-up stretch.</summary>
    SecondPhaseReviveWakeUp,
    /// <summary>Expires the short stretch delay, installs phase-three stretching head bytecode, and seeds a $80-count walking delay.</summary>
    SecondPhaseReviveWakeUpStretch,
    /// <summary>After the wait repeatedly requests forward walking until X passes $50, then enables health-based palettes and prepares the neck.</summary>
    SecondPhaseReviveWalkUpToBaby,
    /// <summary>Clears the baby attack counter and configures both neck targets before falling into the stand-up handshake.</summary>
    SecondPhaseRevivePrepareNeckForBabyDeath,
    /// <summary>Waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    SecondPhaseReviveFinishPreparingForBabyDeath,
    /// <summary>Sampled RNG may adjust posture or fire the targeted onion-ring head list, using Samus's list when no baby index exists.</summary>
    SecondPhaseMurderBabyAttack,
    /// <summary>Waits for the $40-count timer's signed underflow before returning to baby-attack selection.</summary>
    SecondPhaseMurderBabyAttackCooldown,
    /// <summary>Repeatedly requests standing and backward walking to X $40; the baby owns replacement of this body function.</summary>
    SecondPhasePrepareForFinalBabyAttack,
    /// <summary>Installs the final targeted head volley and selects the following inert RTS.</summary>
    SecondPhaseExecuteFinalBabyAttack,
    /// <summary><c>$A9:C1A6 Function_MBBody_Phase2_ExecuteFinalBabyMetroidAttack.return</c>: holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    SecondPhaseFinalBabyAttackHolding,
    /// <summary>Changes body form to 4, requests a fourteen-pixel backward target, and seeds the $20-count combat-setup wait.</summary>
    ThirdPhaseRecoverMakeSomeDistance,
    /// <summary>On timer underflow installs the normal neck and walking sub-handlers and immediately runs the first phase-three fighting call.</summary>
    ThirdPhaseRecoverSetupForFighting,
    /// <summary>Checks head death before neck/walking work, then uses standing pose, attack admission, and sampled RNG to choose bombs or four onion rings.</summary>
    ThirdPhaseFightingMain,
    /// <summary>Pauses the neck/walking sub-handlers for the $40-count timer through zero and resumes main fighting only after signed underflow.</summary>
    ThirdPhaseFightingAttackCooldown,
    /// <summary>Native phase-three death/escape: MoveToBackOfRoom.</summary>
    ThirdPhaseDeathMoveToBackOfRoom,
    /// <summary>Native phase-three death/escape: IdleWhilstExploding.</summary>
    ThirdPhaseDeathIdleWhilstExploding,
    /// <summary>Native phase-three death/escape: StumbleToMiddleOfRoom.</summary>
    ThirdPhaseDeathStumbleToMiddleOfRoom,
    /// <summary>Native phase-three death/escape: DisableBrainEffects.</summary>
    ThirdPhaseDeathDisableBrainEffects,
    /// <summary>Native phase-three death/escape: SetupBodyFadeOut.</summary>
    ThirdPhaseDeathSetupBodyFadeOut,
    /// <summary>Native phase-three death/escape: FadeOutBody.</summary>
    ThirdPhaseDeathFadeOutBody,
    /// <summary>Native phase-three death/escape: FinalFewExplosions.</summary>
    ThirdPhaseDeathFinalFewExplosions,
    /// <summary>Native phase-three death/escape: RealizeDecapitation.</summary>
    ThirdPhaseDeathRealizeDecapitation,
    /// <summary>Native phase-three death/escape: BrainFallsToGround.</summary>
    ThirdPhaseDeathBrainFallsToGround,
    /// <summary>Native phase-three death/escape: LoadCorpseTiles.</summary>
    ThirdPhaseDeathLoadCorpseTiles,
    /// <summary>Native phase-three death/escape: SetupFadeToGrey.</summary>
    ThirdPhaseDeathSetupFadeToGrey,
    /// <summary>Native phase-three death/escape: FadeToGrey.</summary>
    ThirdPhaseDeathFadeToGrey,
    /// <summary>Native phase-three death/escape: CorpseTipsOver.</summary>
    ThirdPhaseDeathCorpseTipsOver,
    /// <summary>Native phase-three death/escape: CorpseRotsAway.</summary>
    ThirdPhaseDeathCorpseRotsAway,
    /// <summary>Native phase-three death/escape: 20FrameDelay.</summary>
    ThirdPhaseDeath20FrameDelay,
    /// <summary>Native phase-three death/escape: LoadEscapeTimerTiles.</summary>
    ThirdPhaseDeathLoadEscapeTimerTiles,
    /// <summary>Native phase-three death/escape: StartEscape.</summary>
    ThirdPhaseDeathStartEscape,
    /// <summary>Native phase-three death/escape: SpawnTimeBombSetSubtitle.</summary>
    ThirdPhaseDeathSpawnTimeBombSetSubtitle,
    /// <summary>Native phase-three death/escape: TypeOutZebesEscapeText.</summary>
    ThirdPhaseDeathTypeOutZebesEscapeText,
    /// <summary>Native phase-three death/escape: DoorExplodingStartTimer.</summary>
    ThirdPhaseDeathDoorExplodingStartTimer,
    /// <summary>Native phase-three death/escape: BlowUpEscapeDoor.</summary>
    ThirdPhaseDeathBlowUpEscapeDoor,
    /// <summary>Native phase-three death/escape: KeepEarthquakeGoing.</summary>
    ThirdPhaseDeathKeepEarthquakeGoing,
}
