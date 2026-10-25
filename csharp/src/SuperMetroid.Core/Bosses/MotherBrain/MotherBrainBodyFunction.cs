namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The phases Mother Brain's body dispatcher runs, one per cartridge routine; unrelated phases are not interchangeable.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1413).
/// </summary>
public enum MotherBrainBodyFunction : ushort
{
    /// <summary><c>$A9:87E1 Function_MotherBrain_FirstPhase</c>: observes the tank-bound head's health and hands zero-health defeat to the fake-death descent.</summary>
    FirstPhase = 1,
    /// <summary><c>$A9:881D Function_MotherBrainBody_FakeDeath_Descent_InitialPause</c>: seeds a 64-count wait and immediately falls into its first decrement.</summary>
    FakeDeathDescentInitialPause,
    /// <summary><c>$A9:8829 Function_MBBody_FakeDeath_Descent_LockSamus_SetScrollRegion</c>: waits for signed timer underflow, then locks Samus and closes scroll region 1.</summary>
    FakeDeathDescentPauseBeforeLock,
    /// <summary><c>$A9:884D Function_MotherBrainBody_FakeDeath_Descent_QueueMusic</c>: expires the 32-count pause, queues music stop/data load, and starts the 12-count unlock wait.</summary>
    FakeDeathDescentPauseBeforeMusic,
    /// <summary><c>$A9:886C Function_MotherBrainBody_FakeDeath_Descent_UnlockSamus</c>: releases Samus input after the music pause and begins the eight-count screen-flash wait.</summary>
    FakeDeathDescentPauseBeforeUnlock,
    /// <summary><c>$A9:8884 Function_MBBody_FakeDeath_Descent_BeginScnFlashing_LowerAcid</c>: starts room flashing, selects lowered-acid FX, clears the ceiling block, and arms tube collapse.</summary>
    FakeDeathDescentPauseBeforeFlash,
    /// <summary><c>$A9:88B2 Function_MBBody_FakeDeath_Descent_TransitionMBPaletteToGrey</c>: advances the fake-death grey palette on timer underflow while also running tube collapse and explosions.</summary>
    FakeDeathDescentFadeToGray,
    /// <summary><c>$A9:88D3 Function_MBBody_FakeDeath_Descent_CollapseTubes</c>: runs the independent tube sequence and dust/explosion cadence until the falling main tube starts ascent.</summary>
    FakeDeathDescentCollapseTubes,
    /// <summary><c>$A9:8C87 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows2_3</c>: requests BG1 room-mutation PLMs for rows 2/3, then selects the next pair for the following body update.</summary>
    FakeDeathAscentDrawRows2And3,
    /// <summary><c>$A9:8C9E Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows4_5</c>: requests the row-4/5 BG1 mutation pair and advances to rows 6/7.</summary>
    FakeDeathAscentDrawRows4And5,
    /// <summary><c>$A9:8CB5 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows6_7</c>: requests the row-6/7 BG1 mutation pair and advances to rows 8/9.</summary>
    FakeDeathAscentDrawRows6And7,
    /// <summary><c>$A9:8CCC Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows8_9</c>: requests the row-8/9 BG1 mutation pair and advances to rows $A/$B.</summary>
    FakeDeathAscentDrawRows8And9,
    /// <summary><c>$A9:8CE3 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_RowsA_B</c>: requests the row-$A/$B BG1 mutation pair and advances to rows $C/$D.</summary>
    FakeDeathAscentDrawRowsAAndB,
    /// <summary><c>$A9:8CFA Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_RowsC_D</c>: requests the final BG1 row pair and hands off to phase-two graphics setup.</summary>
    FakeDeathAscentDrawRowsCAndD,
    /// <summary><c>$A9:8D11 Function_MotherBrainBody_FakeDeath_Ascent_SetupMBPhase2GFX</c>: installs attack/back-leg colors, enables the unpause hook, and transfers the prepared enemy BG2 tilemap.</summary>
    FakeDeathAscentSetupPhase2Graphics,
    /// <summary><c>$A9:8D49 Function_MotherBrainBody_FakeDeath_Ascent_SetupMBPhase2Brain</c>: configures articulated drawing and blending, makes head/body tangible, sets head health $4650, and immediately starts the $80-count suspense wait.</summary>
    FakeDeathAscentSetupPhase2Brain,
    /// <summary><c>$A9:8D79 Function_MotherBrainBody_FakeDeath_Ascent_PauseForSuspense</c>: waits for signed underflow, then seeds the $20-count rising-preparation delay and falls into it.</summary>
    FakeDeathAscentPauseForSuspense,
    /// <summary><c>$A9:8D8B Function_MotherBrainBody_FakeDeath_Ascent_PrepareMBForRising</c>: creates floor-clipping HDMA, installs the hidden head list, and starts leg-tile transfer with a $100-count later pause.</summary>
    FakeDeathAscentPrepareForRising,
    /// <summary><c>$A9:8DB4 Function_MotherBrainBody_FakeDeath_Ascent_LoadMBLegTiles</c>: processes one leg-tile transfer per call and decrements the suspense timer on the same call that finishes the list.</summary>
    FakeDeathAscentLoadLegTiles,
    /// <summary><c>$A9:8DC3 Function_MotherBrainBody_FakeDeath_Ascent_ContinuePausing</c>: waits for the $100 timer's signed underflow before placing the hidden body/BG2 and enabling its hitboxes.</summary>
    FakeDeathAscentContinuePausing,
    /// <summary><c>$A9:8DEC Function_MotherBrainBody_FakeDeath_Ascent_StartMusic_Quake</c>: reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    FakeDeathAscentStartMusicAndEarthquake,
    /// <summary><c>$A9:8E4D Function_MotherBrainBody_FakeDeath_Ascent_RaiseMotherBrain</c>: every fourth NMI count raises the body two pixels with dust and matching BG2 scroll, then removes rising HDMA and starts uncrouching.</summary>
    FakeDeathAscentRaiseMotherBrain,
    /// <summary><c>$A9:8E95 Function_MBBody_FakeDeath_Ascent_WaitForMBUncrouch</c>: waits for body bytecode to publish the standing pose before initializing grey-palette restoration.</summary>
    FakeDeathAscentWaitUntilUncrouched,
    /// <summary><c>$A9:8EAA Function_MBBody_FakeDeath_Ascent_TransitionFromGreyLowerHead</c>: restores brain sprite colors on the four-count timer cadence, then enables phase-two palette handling and drool.</summary>
    FakeDeathAscentTransitionFromGray,
    /// <summary><c>$A9:8EF5 Function_MotherBrainBody_Phase2_Stretching_ShakeHeadMenacing</c>: expires the $17-count menacing shake and installs the stretching head list with a $100-count hold.</summary>
    SecondPhaseStretchingShakeHead,
    /// <summary><c>$A9:8F14 Function_MotherBrainBody_Phase2_Stretching_BringHeadBackUp</c>: expires the long stretch hold, changes both neck targets, and begins the $40-count final wait.</summary>
    SecondPhaseStretchingBringHeadUp,
    /// <summary><c>$A9:8F33 Function_MotherBrainBody_Phase2_Stretching_FinishStretching</c>: completes the final wait, enables small purple breath, and enters ordinary phase-two thinking.</summary>
    SecondPhaseStretchingFinish,
    /// <summary><c>$A9:B605 Function_MotherBrainBody_Phase2_Thinking</c>: zero head health starts the rainbow sequence; otherwise standing pose, health, and the current RNG choose walking, ordinary attacks, or the red hand beam.</summary>
    SecondPhaseThinking,
    /// <summary><c>$A9:B64B Function_MotherBrainBody_Phase2_TryAttack</c>: runs the choose/cooldown/end sub-dispatcher, selecting head bytecode or a bomb/laser body state from Samus movement and sampled RNG.</summary>
    SecondPhaseTryAttack,
    /// <summary><c>$A9:B781 Function_MotherBrainBody_FiringBomb_DecideOnWalking</c>: samples an optional backward target, then advances RNG for the independent crouch-or-fire choice.</summary>
    SecondPhaseBombDecideWalking,
    /// <summary><c>$A9:B7AC Function_MotherBrainBody_FiringBomb_WalkingBackwards</c>: waits for the stored backward target before choosing crouch or immediate bomb firing.</summary>
    SecondPhaseBombWalkingBackwards,
    /// <summary><c>$A9:B7C6 Function_MotherBrainBody_FiringBomb_Crouch</c>: requests the slow crouch and waits for its pose before installing the bomb-firing head list.</summary>
    SecondPhaseBombCrouch,
    /// <summary><c>$A9:B7E8 Function_MotherBrainBody_FiringBomb_FiredBomb</c>: holds the $2C-count post-fire pause, then attempts stand-up with the native same-call fallthrough.</summary>
    SecondPhaseBombFired,
    /// <summary><c>$A9:B7F8 Function_MotherBrainBody_FiringBomb_StandUp</c>: retries the posture helper until standing and returns to phase-two thinking.</summary>
    SecondPhaseBombStandUp,
    /// <summary><c>$A9:B80E Function_MBBody_Phase2_FiringLaser_PositionHeadQuickly</c>: chooses neck targets from head/Samus Y ordering and seeds a four-count quick-positioning wait.</summary>
    SecondPhaseLaserPositionHeadQuickly,
    /// <summary><c>$A9:B839 Function_MBBody_Phase2_FiringLaser_PositionHeadSlowlyAndFire</c>: on timer underflow slows neck rotation, installs laser head bytecode, and seeds a $10-count recovery wait.</summary>
    SecondPhaseLaserPositionHeadSlowlyAndFire,
    /// <summary><c>$A9:B863 Function_MotherBrainBody_Phase2_FiringLaser_FinishAttack</c>: restores neutral neck targets after the wait and immediately re-enters thinking without the ordinary attack sub-dispatcher's end phase.</summary>
    SecondPhaseLaserFinishAttack,
    /// <summary><c>$A9:B87D Function_MotherBrainBody_Phase2_FiringDeathBeam</c>: owns the ordinary red hand-beam attack sub-dispatcher, separate from the health-zero rainbow cutscene.</summary>
    SecondPhaseHandBeam,
    /// <summary><c>$A9:B8EB Function_MotherBrainBody_Phase2_FiringRainbowBeam_ExtendNeck</c>: sets the initial extended neck geometry and installs the $100-count charging-start pause.</summary>
    SecondPhaseRainbowExtendNeck,
    /// <summary><c>$A9:B91A Function_MBBody_Phase2_FiringRainbowBeam_StartCharging</c>: expires the extension pause, installs charging head bytecode, and falls into backward-walk/neck retraction.</summary>
    SecondPhaseRainbowStartCharging,
    /// <summary><c>$A9:B92B Function_MotherBrainBody_Phase2_FiringRainbowBeam_RetractNeck</c>: requests really-slow backward walking toward X $28, then retracts the head and starts a $100-count charge wait.</summary>
    SecondPhaseRainbowRetractNeck,
    /// <summary><c>$A9:B93F Function_MBBody_Phase2_FiringRainbowBeam_WaitForBeamToCharge</c>: waits for signed timer underflow, queues charge sound $71, and falls into downward neck extension.</summary>
    SecondPhaseRainbowWaitForCharge,
    /// <summary><c>$A9:B951 Function_MBBody_Phase2_FiringRainbowBeam_ExtendNeckDown</c>: sets downward neck targets and projectile cooldown, seeds the NTSC $10-count firing delay, and immediately runs its first call.</summary>
    SecondPhaseRainbowExtendNeckDown,
    /// <summary><c>$A9:B975 Function_MBBody_Phase2_FiringRainbowBeam_StartFiringRainbowBeam</c>: widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    SecondPhaseRainbowStartFiring,
    /// <summary><c>$A9:B9E5 Function_MBBody_Phase2_FiringRainbowBeam_MoveSamusTowardWall</c>: maintains beam effects and forced wallward movement until its carry result selects the one-call delay.</summary>
    SecondPhaseRainbowMoveSamusTowardWall,
    /// <summary><c>$A9:BA00 Function_MBBody_Phase2_FiringRainbowBeam_1FrameDelay</c>: runs another beam/movement call, underflows the zero timer, and starts an eight-count earthquake before draining setup.</summary>
    SecondPhaseRainbowOneFrameDelay,
    /// <summary><c>$A9:BA27 Function_MBBody_Phase2_FiringRainbowBeam_StartDrainingSamus</c>: seeds both drain and earthquake timers with $012B and immediately executes the first draining call.</summary>
    SecondPhaseRainbowStartDrainingSamus,
    /// <summary><c>$A9:BA3C Function_MBBody_Phase2_FiringRainbowBeam_DrainingSamus</c>: applies beam damage, ammunition drain, forced wall-centering movement, and effects until timer underflow.</summary>
    SecondPhaseRainbowDrainingSamus,
    /// <summary><c>$A9:BA5E Function_MBBody_Phase2_FiringRainbowBeam_FinishFiring</c>: narrows the beam to angular width $0200, stops HDMA/sound/earthquake, releases input, and starts the falling handoff.</summary>
    SecondPhaseRainbowFinishFiring,
    /// <summary><c>$A9:BAC4 Function_MBBody_Phase2_FiringRainbowBeam_LetSamusFall</c>: changes the drained Samus controller/pose and falls directly into the first custom falling movement call.</summary>
    SecondPhaseRainbowLetSamusFall,
    /// <summary><c>$A9:BAD1 Function_MBBody_Phase2_FiringRainbowBeam_WaitForSamusToLand</c>: advances forced falling until its native carry result reports landing.</summary>
    SecondPhaseRainbowWaitForSamusToLand,
    /// <summary><c>$A9:BADD Function_MBBody_Phase2_FiringRainbowBeam_LowerHead</c>: lowers the neck targets and installs the $80-count next-action pause.</summary>
    SecondPhaseRainbowLowerHead,
    /// <summary><c>$A9:BB06 Function_MBBody_Phase2_FiringRainbowBeam_DecideNextAction</c>: after timer underflow, repeats the beam at health $190 or above; otherwise begins the low-health finish-off cutscene.</summary>
    SecondPhaseRainbowDecideNextAction,
    /// <summary><c>$A9:BD45 Function_MBBody_Phase2_FinishSamusOff_GetSamusToLowEnergy</c>: uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    SecondPhaseFinishSamusOff,
    /// <summary><c>$A9:BD98 Function_MotherBrainBody_Phase2_FinishSamusOff_StandUp</c>: waits for standing, then immediately enters the $10-count admiration pause.</summary>
    SecondPhaseFinishSamusOffStandUp,
    /// <summary><c>$A9:BDA9 Function_MBBody_Phase2_FinishSamusOff_AdmireJobWellDone</c>: expires the admiration pause, installs the stretching head list, and starts a $100-count final-charge delay.</summary>
    SecondPhaseFinishSamusOffAdmire,
    /// <summary><c>$A9:BDC1 Function_MBBody_Phase2_FinishSamusOff_ChargeFinalRainbowBeam</c>: on timer underflow installs charging head bytecode and immediately begins baby-Metroid tile loading.</summary>
    SecondPhaseFinishSamusOffChargeFinalBeam,
    /// <summary><c>$A9:BDD2 Function_MBBody_Phase2_FinishSamusOff_LoadBabyMetroid</c>: transfers one baby sprite-tile record per call; list completion retracts the head, spawns the baby, and seeds a $100-count firing wait.</summary>
    SecondPhaseFinishSamusOffLoadBabyTiles,
    /// <summary><c>$A9:BDED Function_MBBody_Phase2_FinishSamusOff_FireFinalRainbowBeam</c>: expires the final wait, starts firing head bytecode/neck extension and sound $71, then installs its own RTS as the holding function.</summary>
    SecondPhaseFinishSamusOffFireFinalBeam,
    /// <summary><c>$A9:BE1A Function_MBBody_Phase2_FinishSamusOff_FireFinalRainbowBeam.return</c>: inert RTS that waits until the independently running baby latches on and replaces the body function.</summary>
    SecondPhaseFinalRainbowBeamHolding,
    /// <summary><c>$A9:BE38 Function_MotherBrainBody_DrainedByBabyMetroid_TakenAback</c>: the body's next turn after baby attachment changes form/neck motion and immediately starts the $30-count regain-balance pause.</summary>
    SecondPhaseDrainedByBabyTakenAback,
    /// <summary><c>$A9:BE5D Function_MotherBrainBody_DrainedByBabyMetroid_RegainBalance</c>: maintains the rainbow palette until timer underflow, then initializes the independent painful-walking sequence.</summary>
    SecondPhaseDrainedByBabyRegainBalance,
    /// <summary><c>$A9:BE96 Function_MBBody_DrainedByBabyMetroid_FiringRainbowBeam</c>: refreshes head shake, applies rainbow palettes, and advances painful walking until stage 6 disables beam sound and brain palette handling.</summary>
    SecondPhaseDrainedByBabyFiringRainbowBeam,
    /// <summary><c>$A9:BF0E Function_MBBody_DrainedByBabyMetroid_RainbowBeamHasRunOut</c>: continues painful walking through stage 8, then selects dying drool and the retreating neck posture.</summary>
    SecondPhaseDrainedByBabyRainbowBeamRunOut,
    /// <summary><c>$A9:BF41 Function_MBBody_DrainedByBabyMetroid_MoveToBackOfRoom</c>: requests really-slow backward walking to X $28 before disabling upper-neck motion and entering low-power setup.</summary>
    SecondPhaseDrainedByBabyMoveToBackOfRoom,
    /// <summary><c>$A9:BF56 Function_MBBody_DrainedByBabyMetroid_GoIntoLowPowerMode</c>: waits for both neck movement indices to clear, disables drool, and requests fast crouching once standing.</summary>
    SecondPhaseDrainedByBabyGoIntoLowPowerMode,
    /// <summary><c>$A9:BF7D Function_MBBody_DrainedByBabyMetroid_PrepareTransitionToGrey</c>: expires a $40-count crouch pause, clears the palette counter, and immediately begins the $10-count grey-transition cadence.</summary>
    SecondPhaseDrainedByBabyPrepareTransitionToGrey,
    /// <summary><c>$A9:BF95 Function_MBBody_DrainedByBabyMetroid_TransitionToGrey</c>: steps eight drained-grey palettes plus the terminator, restores head health $8CA0, and enters the inanimate revival setup.</summary>
    SecondPhaseDrainedByBabyTransitionToGrey,
    /// <summary><c>$A9:C059 Function_MotherBrainBody_Phase2_ReviveSelf_InanimateGrey</c>: installs the $300-count revival wait and returns without decrementing it on this setup call.</summary>
    SecondPhaseReviveInanimateGrey,
    /// <summary><c>$A9:C066 Function_MotherBrainBody_Phase2_ReviveSelf_ShowSignsOfLife</c>: on signed wait underflow re-enables breath/drool, clears the palette counter, and starts the $E0-count color-restoration delay.</summary>
    SecondPhaseReviveShowSignsOfLife,
    /// <summary><c>$A9:C08F Function_MBBody_Phase2_ReviveSelf_TransitionFromGrey</c>: reverses the eight drained-grey palettes on timer underflow, then re-enables brain palette handling and falls into wake-up.</summary>
    SecondPhaseReviveTransitionFromGrey,
    /// <summary><c>$A9:C0BA Function_MotherBrainBody_Phase2_ReviveSelf_WakeUp</c>: waits for the posture helper to observe standing, then extends the neck and immediately begins a $10-count wake-up stretch.</summary>
    SecondPhaseReviveWakeUp,
    /// <summary><c>$A9:C0E4 Function_MotherBrainBody_Phase2_ReviveSelf_WakeUpStretch</c>: expires the short stretch delay, installs phase-three stretching head bytecode, and seeds a $80-count walking delay.</summary>
    SecondPhaseReviveWakeUpStretch,
    /// <summary><c>$A9:C0FB Function_MBBody_Phase2_ReviveSelf_WalkUpToBabyMetroid</c>: after the wait repeatedly requests forward walking until X passes $50, then enables health-based palettes and prepares the neck.</summary>
    SecondPhaseReviveWalkUpToBaby,
    /// <summary><c>$A9:C11E Func_MBBody_Phase2_ReviveSelf_PrepareNeckForBabyMetroidDeath</c>: clears the baby attack counter and configures both neck targets before falling into the stand-up handshake.</summary>
    SecondPhaseRevivePrepareNeckForBabyDeath,
    /// <summary><c>$A9:C147 Func_MBBody_Phase2_ReviveSelf_FinishPrepForBabyMetroidDeath</c>: waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    SecondPhaseReviveFinishPreparingForBabyDeath,
    /// <summary><c>$A9:C15C Function_MotherBrainBody_Phase2_KillBabyMetroid_Attack</c>: sampled RNG may adjust posture or fire the targeted onion-ring head list, using Samus's list when no baby index exists.</summary>
    SecondPhaseMurderBabyAttack,
    /// <summary><c>$A9:C182 Function_MBBody_Phase2_KillBabyMetroid_AttackCooldown</c>: waits for the $40-count timer's signed underflow before returning to baby-attack selection.</summary>
    SecondPhaseMurderBabyAttackCooldown,
    /// <summary><c>$A9:C18E Function_MBBody_Phase2_PrepareForFinalBabyMetroidAttack</c>: repeatedly requests standing and backward walking to X $40; the baby owns replacement of this body function.</summary>
    SecondPhasePrepareForFinalBabyAttack,
    /// <summary><c>$A9:C19A Function_MBBody_Phase2_ExecuteFinalBabyMetroidAttack</c>: installs the final targeted head volley and selects the following inert RTS.</summary>
    SecondPhaseExecuteFinalBabyAttack,
    /// <summary><c>$A9:C1A6 Function_MBBody_Phase2_ExecuteFinalBabyMetroidAttack.return</c>: holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    SecondPhaseFinalBabyAttackHolding,
    /// <summary><c>$A9:C1CF Function_MBBody_Phase3_RecoverFromCutscene_MakeSomeDistance</c>: changes body form to 4, requests a fourteen-pixel backward target, and seeds the $20-count combat-setup wait.</summary>
    ThirdPhaseRecoverMakeSomeDistance,
    /// <summary><c>$A9:C1F0 Function_MBBody_Phase3_RecoverFromCutscene_SetupForFighting</c>: on timer underflow installs the normal neck and walking sub-handlers and immediately runs the first phase-three fighting call.</summary>
    ThirdPhaseRecoverSetupForFighting,
    /// <summary><c>$A9:C209 Function_MBBody_Phase3_Fighting_Main</c>: checks head death before neck/walking work, then uses standing pose, attack admission, and sampled RNG to choose bombs or four onion rings.</summary>
    ThirdPhaseFightingMain,
    /// <summary><c>$A9:C24E Function_MBBody_Phase3_Fighting_AttackCooldown</c>: pauses the neck/walking sub-handlers for the $40-count timer through zero and resumes main fighting only after signed underflow.</summary>
    ThirdPhaseFightingAttackCooldown,
    /// <summary>$A9:AEE1, native phase-three death/escape: MoveToBackOfRoom.</summary>
    ThirdPhaseDeathMoveToBackOfRoom,
    /// <summary>$A9:AF12, native phase-three death/escape: IdleWhilstExploding.</summary>
    ThirdPhaseDeathIdleWhilstExploding,
    /// <summary>$A9:AF21, native phase-three death/escape: StumbleToMiddleOfRoom.</summary>
    ThirdPhaseDeathStumbleToMiddleOfRoom,
    /// <summary>$A9:AF54, native phase-three death/escape: DisableBrainEffects.</summary>
    ThirdPhaseDeathDisableBrainEffects,
    /// <summary>$A9:AF9D, native phase-three death/escape: SetupBodyFadeOut.</summary>
    ThirdPhaseDeathSetupBodyFadeOut,
    /// <summary>$A9:AFB6, native phase-three death/escape: FadeOutBody.</summary>
    ThirdPhaseDeathFadeOutBody,
    /// <summary>$A9:B013, native phase-three death/escape: FinalFewExplosions.</summary>
    ThirdPhaseDeathFinalFewExplosions,
    /// <summary>$A9:B115, native phase-three death/escape: RealizeDecapitation.</summary>
    ThirdPhaseDeathRealizeDecapitation,
    /// <summary>$A9:B12D, native phase-three death/escape: BrainFallsToGround.</summary>
    ThirdPhaseDeathBrainFallsToGround,
    /// <summary>$A9:B15E, native phase-three death/escape: LoadCorpseTiles.</summary>
    ThirdPhaseDeathLoadCorpseTiles,
    /// <summary>$A9:B173, native phase-three death/escape: SetupFadeToGrey.</summary>
    ThirdPhaseDeathSetupFadeToGrey,
    /// <summary>$A9:B189, native phase-three death/escape: FadeToGrey.</summary>
    ThirdPhaseDeathFadeToGrey,
    /// <summary>$A9:B1B8, native phase-three death/escape: CorpseTipsOver.</summary>
    ThirdPhaseDeathCorpseTipsOver,
    /// <summary>$A9:B1D5, native phase-three death/escape: CorpseRotsAway.</summary>
    ThirdPhaseDeathCorpseRotsAway,
    /// <summary>$A9:B211, native phase-three death/escape: 20FrameDelay.</summary>
    ThirdPhaseDeath20FrameDelay,
    /// <summary>$A9:B258, native phase-three death/escape: LoadEscapeTimerTiles.</summary>
    ThirdPhaseDeathLoadEscapeTimerTiles,
    /// <summary>$A9:B26D, native phase-three death/escape: StartEscape.</summary>
    ThirdPhaseDeathStartEscape,
    /// <summary>$A9:B2D1, native phase-three death/escape: SpawnTimeBombSetSubtitle.</summary>
    ThirdPhaseDeathSpawnTimeBombSetSubtitle,
    /// <summary>$A9:B2E3, native phase-three death/escape: TypeOutZebesEscapeText.</summary>
    ThirdPhaseDeathTypeOutZebesEscapeText,
    /// <summary>$A9:B2F9, native phase-three death/escape: DoorExplodingStartTimer.</summary>
    ThirdPhaseDeathDoorExplodingStartTimer,
    /// <summary>$A9:B32A, native phase-three death/escape: BlowUpEscapeDoor.</summary>
    ThirdPhaseDeathBlowUpEscapeDoor,
    /// <summary>$A9:B33C, native phase-three death/escape: KeepEarthquakeGoing.</summary>
    ThirdPhaseDeathKeepEarthquakeGoing,
}
