namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers used by Mother Brain's body dispatcher in bank <c>$A9</c>.
/// Keeping the cartridge addresses as enum values makes a debugger watch line up with the
/// disassembly without pretending that unrelated phases are interchangeable host states.
/// </summary>
public enum MotherBrainBodyFunction : ushort
{
    /// <summary><c>$A9:87E1 Function_MotherBrain_FirstPhase</c>: observes the tank-bound head's health and hands zero-health defeat to the fake-death descent.</summary>
    FirstPhase = 0x87e1,
    /// <summary><c>$A9:881D Function_MotherBrainBody_FakeDeath_Descent_InitialPause</c>: seeds a 64-count wait and immediately falls into its first decrement.</summary>
    FakeDeathDescentInitialPause = 0x881d,
    /// <summary><c>$A9:8829 Function_MBBody_FakeDeath_Descent_LockSamus_SetScrollRegion</c>: waits for signed timer underflow, then locks Samus and closes scroll region 1.</summary>
    FakeDeathDescentPauseBeforeLock = 0x8829,
    /// <summary><c>$A9:884D Function_MotherBrainBody_FakeDeath_Descent_QueueMusic</c>: expires the 32-count pause, queues music stop/data load, and starts the 12-count unlock wait.</summary>
    FakeDeathDescentPauseBeforeMusic = 0x884d,
    /// <summary><c>$A9:886C Function_MotherBrainBody_FakeDeath_Descent_UnlockSamus</c>: releases Samus input after the music pause and begins the eight-count screen-flash wait.</summary>
    FakeDeathDescentPauseBeforeUnlock = 0x886c,
    /// <summary><c>$A9:8884 Function_MBBody_FakeDeath_Descent_BeginScnFlashing_LowerAcid</c>: starts room flashing, selects lowered-acid FX, clears the ceiling block, and arms tube collapse.</summary>
    FakeDeathDescentPauseBeforeFlash = 0x8884,
    /// <summary><c>$A9:88B2 Function_MBBody_FakeDeath_Descent_TransitionMBPaletteToGrey</c>: advances the fake-death grey palette on timer underflow while also running tube collapse and explosions.</summary>
    FakeDeathDescentFadeToGray = 0x88b2,
    /// <summary><c>$A9:88D3 Function_MBBody_FakeDeath_Descent_CollapseTubes</c>: runs the independent tube sequence and dust/explosion cadence until the falling main tube starts ascent.</summary>
    FakeDeathDescentCollapseTubes = 0x88d3,
    /// <summary><c>$A9:8C87 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows2_3</c>: requests BG1 room-mutation PLMs for rows 2/3, then selects the next pair for the following body update.</summary>
    FakeDeathAscentDrawRows2And3 = 0x8c87,
    /// <summary><c>$A9:8C9E Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows4_5</c>: requests the row-4/5 BG1 mutation pair and advances to rows 6/7.</summary>
    FakeDeathAscentDrawRows4And5 = 0x8c9e,
    /// <summary><c>$A9:8CB5 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows6_7</c>: requests the row-6/7 BG1 mutation pair and advances to rows 8/9.</summary>
    FakeDeathAscentDrawRows6And7 = 0x8cb5,
    /// <summary><c>$A9:8CCC Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_Rows8_9</c>: requests the row-8/9 BG1 mutation pair and advances to rows $A/$B.</summary>
    FakeDeathAscentDrawRows8And9 = 0x8ccc,
    /// <summary><c>$A9:8CE3 Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_RowsA_B</c>: requests the row-$A/$B BG1 mutation pair and advances to rows $C/$D.</summary>
    FakeDeathAscentDrawRowsAAndB = 0x8ce3,
    /// <summary><c>$A9:8CFA Function_MotherBrainBody_FakeDeath_Ascent_DrawRoomBG_RowsC_D</c>: requests the final BG1 row pair and hands off to phase-two graphics setup.</summary>
    FakeDeathAscentDrawRowsCAndD = 0x8cfa,
    /// <summary><c>$A9:8D11 Function_MotherBrainBody_FakeDeath_Ascent_SetupMBPhase2GFX</c>: installs attack/back-leg colors, enables the unpause hook, and transfers the prepared enemy BG2 tilemap.</summary>
    FakeDeathAscentSetupPhase2Graphics = 0x8d11,
    /// <summary><c>$A9:8D49 Function_MotherBrainBody_FakeDeath_Ascent_SetupMBPhase2Brain</c>: configures articulated drawing and blending, makes head/body tangible, sets head health $4650, and immediately starts the $80-count suspense wait.</summary>
    FakeDeathAscentSetupPhase2Brain = 0x8d49,
    /// <summary><c>$A9:8D79 Function_MotherBrainBody_FakeDeath_Ascent_PauseForSuspense</c>: waits for signed underflow, then seeds the $20-count rising-preparation delay and falls into it.</summary>
    FakeDeathAscentPauseForSuspense = 0x8d79,
    /// <summary><c>$A9:8D8B Function_MotherBrainBody_FakeDeath_Ascent_PrepareMBForRising</c>: creates floor-clipping HDMA, installs the hidden head list, and starts leg-tile transfer with a $100-count later pause.</summary>
    FakeDeathAscentPrepareForRising = 0x8d8b,
    /// <summary><c>$A9:8DB4 Function_MotherBrainBody_FakeDeath_Ascent_LoadMBLegTiles</c>: processes one leg-tile transfer per call and decrements the suspense timer on the same call that finishes the list.</summary>
    FakeDeathAscentLoadLegTiles = 0x8db4,
    /// <summary><c>$A9:8DC3 Function_MotherBrainBody_FakeDeath_Ascent_ContinuePausing</c>: waits for the $100 timer's signed underflow before placing the hidden body/BG2 and enabling its hitboxes.</summary>
    FakeDeathAscentContinuePausing = 0x8dc3,
    /// <summary><c>$A9:8DEC Function_MotherBrainBody_FakeDeath_Ascent_StartMusic_Quake</c>: reveals the crouched body, starts boss music and earthquake, and enables the rising neck motion.</summary>
    FakeDeathAscentStartMusicAndEarthquake = 0x8dec,
    /// <summary><c>$A9:8E4D Function_MotherBrainBody_FakeDeath_Ascent_RaiseMotherBrain</c>: every fourth NMI count raises the body two pixels with dust and matching BG2 scroll, then removes rising HDMA and starts uncrouching.</summary>
    FakeDeathAscentRaiseMotherBrain = 0x8e4d,
    /// <summary><c>$A9:8E95 Function_MBBody_FakeDeath_Ascent_WaitForMBUncrouch</c>: waits for body bytecode to publish the standing pose before initializing grey-palette restoration.</summary>
    FakeDeathAscentWaitUntilUncrouched = 0x8e95,
    /// <summary><c>$A9:8EAA Function_MBBody_FakeDeath_Ascent_TransitionFromGreyLowerHead</c>: restores brain sprite colors on the four-count timer cadence, then enables phase-two palette handling and drool.</summary>
    FakeDeathAscentTransitionFromGray = 0x8eaa,
    /// <summary><c>$A9:8EF5 Function_MotherBrainBody_Phase2_Stretching_ShakeHeadMenacing</c>: expires the $17-count menacing shake and installs the stretching head list with a $100-count hold.</summary>
    SecondPhaseStretchingShakeHead = 0x8ef5,
    /// <summary><c>$A9:8F14 Function_MotherBrainBody_Phase2_Stretching_BringHeadBackUp</c>: expires the long stretch hold, changes both neck targets, and begins the $40-count final wait.</summary>
    SecondPhaseStretchingBringHeadUp = 0x8f14,
    /// <summary><c>$A9:8F33 Function_MotherBrainBody_Phase2_Stretching_FinishStretching</c>: completes the final wait, enables small purple breath, and enters ordinary phase-two thinking.</summary>
    SecondPhaseStretchingFinish = 0x8f33,
    /// <summary><c>$A9:B605 Function_MotherBrainBody_Phase2_Thinking</c>: zero head health starts the rainbow sequence; otherwise standing pose, health, and the current RNG choose walking, ordinary attacks, or the red hand beam.</summary>
    SecondPhaseThinking = 0xb605,
    /// <summary><c>$A9:B64B Function_MotherBrainBody_Phase2_TryAttack</c>: runs the choose/cooldown/end sub-dispatcher, selecting head bytecode or a bomb/laser body state from Samus movement and sampled RNG.</summary>
    SecondPhaseTryAttack = 0xb64b,
    /// <summary><c>$A9:B781 Function_MotherBrainBody_FiringBomb_DecideOnWalking</c>: samples an optional backward target, then advances RNG for the independent crouch-or-fire choice.</summary>
    SecondPhaseBombDecideWalking = 0xb781,
    /// <summary><c>$A9:B7AC Function_MotherBrainBody_FiringBomb_WalkingBackwards</c>: waits for the stored backward target before choosing crouch or immediate bomb firing.</summary>
    SecondPhaseBombWalkingBackwards = 0xb7ac,
    /// <summary><c>$A9:B7C6 Function_MotherBrainBody_FiringBomb_Crouch</c>: requests the slow crouch and waits for its pose before installing the bomb-firing head list.</summary>
    SecondPhaseBombCrouch = 0xb7c6,
    /// <summary><c>$A9:B7E8 Function_MotherBrainBody_FiringBomb_FiredBomb</c>: holds the $2C-count post-fire pause, then attempts stand-up with the native same-call fallthrough.</summary>
    SecondPhaseBombFired = 0xb7e8,
    /// <summary><c>$A9:B7F8 Function_MotherBrainBody_FiringBomb_StandUp</c>: retries the posture helper until standing and returns to phase-two thinking.</summary>
    SecondPhaseBombStandUp = 0xb7f8,
    /// <summary><c>$A9:B80E Function_MBBody_Phase2_FiringLaser_PositionHeadQuickly</c>: chooses neck targets from head/Samus Y ordering and seeds a four-count quick-positioning wait.</summary>
    SecondPhaseLaserPositionHeadQuickly = 0xb80e,
    /// <summary><c>$A9:B839 Function_MBBody_Phase2_FiringLaser_PositionHeadSlowlyAndFire</c>: on timer underflow slows neck rotation, installs laser head bytecode, and seeds a $10-count recovery wait.</summary>
    SecondPhaseLaserPositionHeadSlowlyAndFire = 0xb839,
    /// <summary><c>$A9:B863 Function_MotherBrainBody_Phase2_FiringLaser_FinishAttack</c>: restores neutral neck targets after the wait and immediately re-enters thinking without the ordinary attack sub-dispatcher's end phase.</summary>
    SecondPhaseLaserFinishAttack = 0xb863,
    /// <summary><c>$A9:B87D Function_MotherBrainBody_Phase2_FiringDeathBeam</c>: owns the ordinary red hand-beam attack sub-dispatcher, separate from the health-zero rainbow cutscene.</summary>
    SecondPhaseHandBeam = 0xb87d,
    /// <summary><c>$A9:B8EB Function_MotherBrainBody_Phase2_FiringRainbowBeam_ExtendNeck</c>: sets the initial extended neck geometry and installs the $100-count charging-start pause.</summary>
    SecondPhaseRainbowExtendNeck = 0xb8eb,
    /// <summary><c>$A9:B91A Function_MBBody_Phase2_FiringRainbowBeam_StartCharging</c>: expires the extension pause, installs charging head bytecode, and falls into backward-walk/neck retraction.</summary>
    SecondPhaseRainbowStartCharging = 0xb91a,
    /// <summary><c>$A9:B92B Function_MotherBrainBody_Phase2_FiringRainbowBeam_RetractNeck</c>: requests really-slow backward walking toward X $28, then retracts the head and starts a $100-count charge wait.</summary>
    SecondPhaseRainbowRetractNeck = 0xb92b,
    /// <summary><c>$A9:B93F Function_MBBody_Phase2_FiringRainbowBeam_WaitForBeamToCharge</c>: waits for signed timer underflow, queues charge sound $71, and falls into downward neck extension.</summary>
    SecondPhaseRainbowWaitForCharge = 0xb93f,
    /// <summary><c>$A9:B951 Function_MBBody_Phase2_FiringRainbowBeam_ExtendNeckDown</c>: sets downward neck targets and projectile cooldown, seeds the NTSC $10-count firing delay, and immediately runs its first call.</summary>
    SecondPhaseRainbowExtendNeckDown = 0xb951,
    /// <summary><c>$A9:B975 Function_MBBody_Phase2_FiringRainbowBeam_StartFiringRainbowBeam</c>: widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    SecondPhaseRainbowStartFiring = 0xb975,
    /// <summary><c>$A9:B9E5 Function_MBBody_Phase2_FiringRainbowBeam_MoveSamusTowardWall</c>: maintains beam effects and forced wallward movement until its carry result selects the one-call delay.</summary>
    SecondPhaseRainbowMoveSamusTowardWall = 0xb9e5,
    /// <summary><c>$A9:BA00 Function_MBBody_Phase2_FiringRainbowBeam_1FrameDelay</c>: runs another beam/movement call, underflows the zero timer, and starts an eight-count earthquake before draining setup.</summary>
    SecondPhaseRainbowOneFrameDelay = 0xba00,
    /// <summary><c>$A9:BA27 Function_MBBody_Phase2_FiringRainbowBeam_StartDrainingSamus</c>: seeds both drain and earthquake timers with $012B and immediately executes the first draining call.</summary>
    SecondPhaseRainbowStartDrainingSamus = 0xba27,
    /// <summary><c>$A9:BA3C Function_MBBody_Phase2_FiringRainbowBeam_DrainingSamus</c>: applies beam damage, ammunition drain, forced wall-centering movement, and effects until timer underflow.</summary>
    SecondPhaseRainbowDrainingSamus = 0xba3c,
    /// <summary><c>$A9:BA5E Function_MBBody_Phase2_FiringRainbowBeam_FinishFiring</c>: narrows the beam to angular width $0200, stops HDMA/sound/earthquake, releases input, and starts the falling handoff.</summary>
    SecondPhaseRainbowFinishFiring = 0xba5e,
    /// <summary><c>$A9:BAC4 Function_MBBody_Phase2_FiringRainbowBeam_LetSamusFall</c>: changes the drained Samus controller/pose and falls directly into the first custom falling movement call.</summary>
    SecondPhaseRainbowLetSamusFall = 0xbac4,
    /// <summary><c>$A9:BAD1 Function_MBBody_Phase2_FiringRainbowBeam_WaitForSamusToLand</c>: advances forced falling until its native carry result reports landing.</summary>
    SecondPhaseRainbowWaitForSamusToLand = 0xbad1,
    /// <summary><c>$A9:BADD Function_MBBody_Phase2_FiringRainbowBeam_LowerHead</c>: lowers the neck targets and installs the $80-count next-action pause.</summary>
    SecondPhaseRainbowLowerHead = 0xbadd,
    /// <summary><c>$A9:BB06 Function_MBBody_Phase2_FiringRainbowBeam_DecideNextAction</c>: after timer underflow, repeats the beam at health $190 or above; otherwise begins the low-health finish-off cutscene.</summary>
    SecondPhaseRainbowDecideNextAction = 0xbb06,
    /// <summary><c>$A9:BD45 Function_MBBody_Phase2_FinishSamusOff_GetSamusToLowEnergy</c>: uses suit-adjusted health thresholds and sampled RNG to choose onion rings/bombs until Samus reaches the intended low-health floor.</summary>
    SecondPhaseFinishSamusOff = 0xbd45,
    /// <summary><c>$A9:BD98 Function_MotherBrainBody_Phase2_FinishSamusOff_StandUp</c>: waits for standing, then immediately enters the $10-count admiration pause.</summary>
    SecondPhaseFinishSamusOffStandUp = 0xbd98,
    /// <summary><c>$A9:BDA9 Function_MBBody_Phase2_FinishSamusOff_AdmireJobWellDone</c>: expires the admiration pause, installs the stretching head list, and starts a $100-count final-charge delay.</summary>
    SecondPhaseFinishSamusOffAdmire = 0xbda9,
    /// <summary><c>$A9:BDC1 Function_MBBody_Phase2_FinishSamusOff_ChargeFinalRainbowBeam</c>: on timer underflow installs charging head bytecode and immediately begins baby-Metroid tile loading.</summary>
    SecondPhaseFinishSamusOffChargeFinalBeam = 0xbdc1,
    /// <summary><c>$A9:BDD2 Function_MBBody_Phase2_FinishSamusOff_LoadBabyMetroid</c>: transfers one baby sprite-tile record per call; list completion retracts the head, spawns the baby, and seeds a $100-count firing wait.</summary>
    SecondPhaseFinishSamusOffLoadBabyTiles = 0xbdd2,
    /// <summary><c>$A9:BDED Function_MBBody_Phase2_FinishSamusOff_FireFinalRainbowBeam</c>: expires the final wait, starts firing head bytecode/neck extension and sound $71, then installs its own RTS as the holding function.</summary>
    SecondPhaseFinishSamusOffFireFinalBeam = 0xbded,
    /// <summary><c>$A9:BE1A Function_MBBody_Phase2_FinishSamusOff_FireFinalRainbowBeam.return</c>: inert RTS that waits until the independently running baby latches on and replaces the body function.</summary>
    SecondPhaseFinalRainbowBeamHolding = 0xbe1a,
    /// <summary><c>$A9:BE38 Function_MotherBrainBody_DrainedByBabyMetroid_TakenAback</c>: the body's next turn after baby attachment changes form/neck motion and immediately starts the $30-count regain-balance pause.</summary>
    SecondPhaseDrainedByBabyTakenAback = 0xbe38,
    /// <summary><c>$A9:BE5D Function_MotherBrainBody_DrainedByBabyMetroid_RegainBalance</c>: maintains the rainbow palette until timer underflow, then initializes the independent painful-walking sequence.</summary>
    SecondPhaseDrainedByBabyRegainBalance = 0xbe5d,
    /// <summary><c>$A9:BE96 Function_MBBody_DrainedByBabyMetroid_FiringRainbowBeam</c>: refreshes head shake, applies rainbow palettes, and advances painful walking until stage 6 disables beam sound and brain palette handling.</summary>
    SecondPhaseDrainedByBabyFiringRainbowBeam = 0xbe96,
    /// <summary><c>$A9:BF0E Function_MBBody_DrainedByBabyMetroid_RainbowBeamHasRunOut</c>: continues painful walking through stage 8, then selects dying drool and the retreating neck posture.</summary>
    SecondPhaseDrainedByBabyRainbowBeamRunOut = 0xbf0e,
    /// <summary><c>$A9:BF41 Function_MBBody_DrainedByBabyMetroid_MoveToBackOfRoom</c>: requests really-slow backward walking to X $28 before disabling upper-neck motion and entering low-power setup.</summary>
    SecondPhaseDrainedByBabyMoveToBackOfRoom = 0xbf41,
    /// <summary><c>$A9:BF56 Function_MBBody_DrainedByBabyMetroid_GoIntoLowPowerMode</c>: waits for both neck movement indices to clear, disables drool, and requests fast crouching once standing.</summary>
    SecondPhaseDrainedByBabyGoIntoLowPowerMode = 0xbf56,
    /// <summary><c>$A9:BF7D Function_MBBody_DrainedByBabyMetroid_PrepareTransitionToGrey</c>: expires a $40-count crouch pause, clears the palette counter, and immediately begins the $10-count grey-transition cadence.</summary>
    SecondPhaseDrainedByBabyPrepareTransitionToGrey = 0xbf7d,
    /// <summary><c>$A9:BF95 Function_MBBody_DrainedByBabyMetroid_TransitionToGrey</c>: steps eight drained-grey palettes plus the terminator, restores head health $8CA0, and enters the inanimate revival setup.</summary>
    SecondPhaseDrainedByBabyTransitionToGrey = 0xbf95,
    /// <summary><c>$A9:C059 Function_MotherBrainBody_Phase2_ReviveSelf_InanimateGrey</c>: installs the $300-count revival wait and returns without decrementing it on this setup call.</summary>
    SecondPhaseReviveInanimateGrey = 0xc059,
    /// <summary><c>$A9:C066 Function_MotherBrainBody_Phase2_ReviveSelf_ShowSignsOfLife</c>: on signed wait underflow re-enables breath/drool, clears the palette counter, and starts the $E0-count color-restoration delay.</summary>
    SecondPhaseReviveShowSignsOfLife = 0xc066,
    /// <summary><c>$A9:C08F Function_MBBody_Phase2_ReviveSelf_TransitionFromGrey</c>: reverses the eight drained-grey palettes on timer underflow, then re-enables brain palette handling and falls into wake-up.</summary>
    SecondPhaseReviveTransitionFromGrey = 0xc08f,
    /// <summary><c>$A9:C0BA Function_MotherBrainBody_Phase2_ReviveSelf_WakeUp</c>: waits for the posture helper to observe standing, then extends the neck and immediately begins a $10-count wake-up stretch.</summary>
    SecondPhaseReviveWakeUp = 0xc0ba,
    /// <summary><c>$A9:C0E4 Function_MotherBrainBody_Phase2_ReviveSelf_WakeUpStretch</c>: expires the short stretch delay, installs phase-three stretching head bytecode, and seeds a $80-count walking delay.</summary>
    SecondPhaseReviveWakeUpStretch = 0xc0e4,
    /// <summary><c>$A9:C0FB Function_MBBody_Phase2_ReviveSelf_WalkUpToBabyMetroid</c>: after the wait repeatedly requests forward walking until X passes $50, then enables health-based palettes and prepares the neck.</summary>
    SecondPhaseReviveWalkUpToBaby = 0xc0fb,
    /// <summary><c>$A9:C11E Func_MBBody_Phase2_ReviveSelf_PrepareNeckForBabyMetroidDeath</c>: clears the baby attack counter and configures both neck targets before falling into the stand-up handshake.</summary>
    SecondPhaseRevivePrepareNeckForBabyDeath = 0xc11e,
    /// <summary><c>$A9:C147 Func_MBBody_Phase2_ReviveSelf_FinishPrepForBabyMetroidDeath</c>: waits for standing, requests really-slow forward walking, and immediately enters the baby-attack chooser.</summary>
    SecondPhaseReviveFinishPreparingForBabyDeath = 0xc147,
    /// <summary><c>$A9:C15C Function_MotherBrainBody_Phase2_KillBabyMetroid_Attack</c>: sampled RNG may adjust posture or fire the targeted onion-ring head list, using Samus's list when no baby index exists.</summary>
    SecondPhaseMurderBabyAttack = 0xc15c,
    /// <summary><c>$A9:C182 Function_MBBody_Phase2_KillBabyMetroid_AttackCooldown</c>: waits for the $40-count timer's signed underflow before returning to baby-attack selection.</summary>
    SecondPhaseMurderBabyAttackCooldown = 0xc182,
    /// <summary><c>$A9:C18E Function_MBBody_Phase2_PrepareForFinalBabyMetroidAttack</c>: repeatedly requests standing and backward walking to X $40; the baby owns replacement of this body function.</summary>
    SecondPhasePrepareForFinalBabyAttack = 0xc18e,
    /// <summary><c>$A9:C19A Function_MBBody_Phase2_ExecuteFinalBabyMetroidAttack</c>: installs the final targeted head volley and selects the following inert RTS.</summary>
    SecondPhaseExecuteFinalBabyAttack = 0xc19a,
    /// <summary><c>$A9:C1A6 Function_MBBody_Phase2_ExecuteFinalBabyMetroidAttack.return</c>: holds while the head volley and baby actor independently own the final damage, death, and recovery sequence.</summary>
    SecondPhaseFinalBabyAttackHolding = 0xc1a6,
    /// <summary><c>$A9:C1CF Function_MBBody_Phase3_RecoverFromCutscene_MakeSomeDistance</c>: changes body form to 4, requests a fourteen-pixel backward target, and seeds the $20-count combat-setup wait.</summary>
    ThirdPhaseRecoverMakeSomeDistance = 0xc1cf,
    /// <summary><c>$A9:C1F0 Function_MBBody_Phase3_RecoverFromCutscene_SetupForFighting</c>: on timer underflow installs the normal neck and walking sub-handlers and immediately runs the first phase-three fighting call.</summary>
    ThirdPhaseRecoverSetupForFighting = 0xc1f0,
    /// <summary><c>$A9:C209 Function_MBBody_Phase3_Fighting_Main</c>: checks head death before neck/walking work, then uses standing pose, attack admission, and sampled RNG to choose bombs or four onion rings.</summary>
    ThirdPhaseFightingMain = 0xc209,
    /// <summary><c>$A9:C24E Function_MBBody_Phase3_Fighting_AttackCooldown</c>: pauses the neck/walking sub-handlers for the $40-count timer through zero and resumes main fighting only after signed underflow.</summary>
    ThirdPhaseFightingAttackCooldown = 0xc24e,
    /// <summary>$A9:AEE1, native phase-three death/escape: MoveToBackOfRoom.</summary>
    ThirdPhaseDeathMoveToBackOfRoom = 0xaee1,
    /// <summary>$A9:AF12, native phase-three death/escape: IdleWhilstExploding.</summary>
    ThirdPhaseDeathIdleWhilstExploding = 0xaf12,
    /// <summary>$A9:AF21, native phase-three death/escape: StumbleToMiddleOfRoom.</summary>
    ThirdPhaseDeathStumbleToMiddleOfRoom = 0xaf21,
    /// <summary>$A9:AF54, native phase-three death/escape: DisableBrainEffects.</summary>
    ThirdPhaseDeathDisableBrainEffects = 0xaf54,
    /// <summary>$A9:AF9D, native phase-three death/escape: SetupBodyFadeOut.</summary>
    ThirdPhaseDeathSetupBodyFadeOut = 0xaf9d,
    /// <summary>$A9:AFB6, native phase-three death/escape: FadeOutBody.</summary>
    ThirdPhaseDeathFadeOutBody = 0xafb6,
    /// <summary>$A9:B013, native phase-three death/escape: FinalFewExplosions.</summary>
    ThirdPhaseDeathFinalFewExplosions = 0xb013,
    /// <summary>$A9:B115, native phase-three death/escape: RealizeDecapitation.</summary>
    ThirdPhaseDeathRealizeDecapitation = 0xb115,
    /// <summary>$A9:B12D, native phase-three death/escape: BrainFallsToGround.</summary>
    ThirdPhaseDeathBrainFallsToGround = 0xb12d,
    /// <summary>$A9:B15E, native phase-three death/escape: LoadCorpseTiles.</summary>
    ThirdPhaseDeathLoadCorpseTiles = 0xb15e,
    /// <summary>$A9:B173, native phase-three death/escape: SetupFadeToGrey.</summary>
    ThirdPhaseDeathSetupFadeToGrey = 0xb173,
    /// <summary>$A9:B189, native phase-three death/escape: FadeToGrey.</summary>
    ThirdPhaseDeathFadeToGrey = 0xb189,
    /// <summary>$A9:B1B8, native phase-three death/escape: CorpseTipsOver.</summary>
    ThirdPhaseDeathCorpseTipsOver = 0xb1b8,
    /// <summary>$A9:B1D5, native phase-three death/escape: CorpseRotsAway.</summary>
    ThirdPhaseDeathCorpseRotsAway = 0xb1d5,
    /// <summary>$A9:B211, native phase-three death/escape: 20FrameDelay.</summary>
    ThirdPhaseDeath20FrameDelay = 0xb211,
    /// <summary>$A9:B258, native phase-three death/escape: LoadEscapeTimerTiles.</summary>
    ThirdPhaseDeathLoadEscapeTimerTiles = 0xb258,
    /// <summary>$A9:B26D, native phase-three death/escape: StartEscape.</summary>
    ThirdPhaseDeathStartEscape = 0xb26d,
    /// <summary>$A9:B2D1, native phase-three death/escape: SpawnTimeBombSetSubtitle.</summary>
    ThirdPhaseDeathSpawnTimeBombSetSubtitle = 0xb2d1,
    /// <summary>$A9:B2E3, native phase-three death/escape: TypeOutZebesEscapeText.</summary>
    ThirdPhaseDeathTypeOutZebesEscapeText = 0xb2e3,
    /// <summary>$A9:B2F9, native phase-three death/escape: DoorExplodingStartTimer.</summary>
    ThirdPhaseDeathDoorExplodingStartTimer = 0xb2f9,
    /// <summary>$A9:B32A, native phase-three death/escape: BlowUpEscapeDoor.</summary>
    ThirdPhaseDeathBlowUpEscapeDoor = 0xb32a,
    /// <summary>$A9:B33C, native phase-three death/escape: KeepEarthquakeGoing.</summary>
    ThirdPhaseDeathKeepEarthquakeGoing = 0xb33c,
}
