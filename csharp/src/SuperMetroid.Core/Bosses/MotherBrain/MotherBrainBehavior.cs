using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Mother Brain species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MotherBrainBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. MotherBrainPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MotherBrainEntity(this, slot, common, _bossArtwork);
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    internal MotherBrainProjectiles Projectiles { get; }

    /// <summary>Mother Brain's head and brain: their setup, neck, animations, cues and drawing.</summary>
    internal MotherBrainHead Head { get; }

    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    internal MotherBrainEncounterAccess Access { get; }

    /// <summary>Mother Brain's body and tank: their setup, animations, movement, footsteps and the first phase.</summary>
    internal MotherBrainBody Body { get; }

    /// <summary>Mother Brain's false death: the fade to gray, the tubes collapsing and falling, the room rows and the fade back.</summary>
    internal MotherBrainFakeDeath FakeDeath { get; }

    /// <summary>Mother Brain rising into her second form: its graphics, brain setup, the rise and the menacing head shake.</summary>
    internal MotherBrainAscent Ascent { get; }

    /// <summary>Mother Brain's second-phase attacks: choosing one, the bomb walk, the lasers and the hand beam.</summary>
    internal MotherBrainAttacks Attacks { get; }

    /// <summary>Mother Brain's rainbow beam, its palette, and her death.</summary>
    internal MotherBrainRainbowAndDeath RainbowAndDeath { get; }

    /// <summary>Mother Brain's hitboxes and touch, and what Samus's weapons do to her head and body.</summary>
    internal MotherBrainCombat Combat { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Binds the encounter's room services while keeping one state owner for body and head.</summary>
    internal MotherBrainBehavior(MotherBrainBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _presentation = dependencies.Presentation.Surfaces;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        Projectiles = new MotherBrainProjectiles(_room, dependencies.Projectiles, dependencies.Earthquake, dependencies.DeathsAndDrops, _motherBrainEncounter, dependencies.RoomActors);
        Access = new MotherBrainEncounterAccess(_frameResults, _motherBrainEncounter);
        Head = new MotherBrainHead(new MotherBrainHeadDependencies { Room = _room, Projectiles = dependencies.Projectiles, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access  });
        Body = new MotherBrainBody(new MotherBrainBodyDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , Presentation = _presentation });
        FakeDeath = new MotherBrainFakeDeath(new MotherBrainFakeDeathDependencies { FrameResults = _frameResults, Dispatch = dependencies.Dispatch, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, Access = Access });
        Ascent = new MotherBrainAscent(_room, dependencies.Projectiles, dependencies.Earthquake, _presentation, dependencies.FrameContext, _motherBrainEncounter);
        RainbowAndDeath = new MotherBrainRainbowAndDeath(new MotherBrainRainbowAndDeathDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , BossArtwork = _bossArtwork });
        Attacks = new MotherBrainAttacks(_room, RainbowAndDeath);
        Combat = new MotherBrainCombat(_room, dependencies.Projectiles, dependencies.Audio, dependencies.Collision, _motherBrainEncounter);
    }



    /// <summary>Runs the common body main/hurt entry.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainEnemyState state = Access.RequireCompleteMotherBrainState(body);

        // The body main advances the independent room-palette animation before it
        // dispatches the body function. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomFlash(state);
        RunMotherBrainBodyPhase(state, samus, sharedProjectiles);
        // Every body function returns through the common contact tail. The first-phase
        // function also has its own authored contact call; retain both sites.
        if (samus is not null)
            MotherBrainBody.ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>Dispatches the active body phase while preserving the phase-specific timer and attack order.</summary>
    private void RunMotherBrainBodyPhase(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        switch (state.Phase)
        {
            case MotherBrainBodyPhase.FirstPhase:
                Body.RunMotherBrainFirstPhase(state, samus);
                return;
            case MotherBrainBodyPhase.FakeDeathDescentInitialPause:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash:
            case MotherBrainBodyPhase.FakeDeathDescentFadeToGray:
            case MotherBrainBodyPhase.FakeDeathDescentCollapseTubes:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12:
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics:
                RunMotherBrainFakeDeath(state, samus);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyPhase.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody:
            case MotherBrainBodyPhase.FakeDeathAscentContinuePausing:
            case MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake:
            case MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain:
            case MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched:
            case MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray:
            case MotherBrainBodyPhase.SecondPhaseStretchingShakeHead:
            case MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp:
            case MotherBrainBodyPhase.SecondPhaseStretchingFinish:
            case MotherBrainBodyPhase.SecondPhaseThinking:
            case MotherBrainBodyPhase.SecondPhaseTryAttack:
            case MotherBrainBodyPhase.SecondPhaseBombDecideWalking:
            case MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards:
            case MotherBrainBodyPhase.SecondPhaseBombCrouch:
            case MotherBrainBodyPhase.SecondPhaseBombFired:
            case MotherBrainBodyPhase.SecondPhaseBombStandUp:
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly:
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire:
            case MotherBrainBodyPhase.SecondPhaseLaserFinishAttack:
            case MotherBrainBodyPhase.SecondPhaseHandBeam:
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyPhase.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOff:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUp:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyPhase.ThirdPhaseFightingMain:
            case MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyPhase.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing:
                RunMotherBrainPhaseTwoAscent(
                    state,
                    samus,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain body has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Four authored jitter offsets selected by the low shake-state bits.</summary>
    internal static readonly Offset16<RoomPixels>[] MotherBrainShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Advances MotherBrainBody's typed animation.</summary>
    internal void AdvanceMotherBrainBodyTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context)
    {
        Body.AdvanceMotherBrainSlotAnimation(slot, context.Samus);
    }

    /// <summary>Runs MotherBrainBody's main AI.</summary>
    internal void RunMotherBrainBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMotherBrainBodyMain(slot, frame.Samus, frame.SharedProjectiles);
    }

    /// <summary>Runs MotherBrainHead's main AI.</summary>
    internal void RunMotherBrainHeadMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        Head.RunMotherBrainHeadMain(slot, frame.Samus);
    }
    /// <summary>Sequence of explosion positions used by the fake-death collapse.</summary>
    internal static readonly Position16<RoomPixels>[] MotherBrainFakeDeathExplosionPositions =
    [
        new Position16<RoomPixels>(136, 116),
        new Position16<RoomPixels>(120, 132),
        new Position16<RoomPixels>(124, 90),
        new Position16<RoomPixels>(138, 146),
        new Position16<RoomPixels>(120, 52),
        new Position16<RoomPixels>(124, 170),
        new Position16<RoomPixels>(138, 72),
        new Position16<RoomPixels>(120, 206),
    ];
    /// <summary>Authored smoke puff X offsets cycled while a tube lands.</summary>
    internal static readonly int[] MotherBrainFallingTubeSmokeXOffsets = [-8, 2, -4, 6];

    /// <summary>Dispatches the body function after the phase-one function changes to fake death.</summary>
    internal void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState? samus)
    {
        switch (state.Phase)
        {
            case MotherBrainBodyPhase.FakeDeathDescentInitialPause:
                // The fake-death start installs the following countdown and falls through to it. The
                // first observable value is therefore 63, not the host-friendly value 64.
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock;
                state.PhaseTimer = 64;
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock:
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;
                _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
                _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.MotherBrain), MusicQueueDelay.Shortest);
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock;
                state.PhaseTimer = 12;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;
                // Samus command one restores the normal handler pair.
                samus?.Control.Unlock();
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash;
                state.PhaseTimer = 8;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;

                // The fake-death flash starts the looping room flash, drains the room's acid,
                // arms the head's independent tube sequence, and clears both fade timers.
                state.RoomFlashFrame = 0;
                state.RoomFlashTimer = 1;
                _frameResults.MotherBrainAcidChange = MotherBrainRoomFxDefinitions.AcidDrains;
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.WaitForFourFreeProjectileSlots;
                state.TubeCollapseTimer = 0;
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentFadeToGray;
                state.PhaseTimer = 0;
                state.GrayFadeIndex = 0;
                Access.RequestMotherBrainObject(new(14, 2), MotherBrainRoomMutation.ClearCeilingBlock);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentFadeToGray:
                FakeDeath.RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainBodyPhase.FakeDeathDescentCollapseTubes:
                FakeDeath.RunMotherBrainTubeCollapse(state);
                FakeDeath.RunMotherBrainFakeDeathExplosion(state);
                return;

            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow1,
                    MotherBrainRoomMutation.MuralRow2,
                    MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow3,
                    MotherBrainRoomMutation.MuralRow4,
                    MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow5,
                    MotherBrainRoomMutation.MuralRow6,
                    MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow7,
                    MotherBrainRoomMutation.MuralRow8,
                    MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow9,
                    MotherBrainRoomMutation.MuralRow10,
                    MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow11,
                    MotherBrainRoomMutation.MuralRow12,
                    MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics:
                Ascent.SetupMotherBrainPhaseTwoGraphics(state);
                return;
            case MotherBrainBodyPhase.FirstPhase:
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyPhase.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody:
            case MotherBrainBodyPhase.FakeDeathAscentContinuePausing:
            case MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake:
            case MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain:
            case MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched:
            case MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray:
            case MotherBrainBodyPhase.SecondPhaseStretchingShakeHead:
            case MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp:
            case MotherBrainBodyPhase.SecondPhaseStretchingFinish:
            case MotherBrainBodyPhase.SecondPhaseThinking:
            case MotherBrainBodyPhase.SecondPhaseTryAttack:
            case MotherBrainBodyPhase.SecondPhaseBombDecideWalking:
            case MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards:
            case MotherBrainBodyPhase.SecondPhaseBombCrouch:
            case MotherBrainBodyPhase.SecondPhaseBombFired:
            case MotherBrainBodyPhase.SecondPhaseBombStandUp:
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly:
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire:
            case MotherBrainBodyPhase.SecondPhaseLaserFinishAttack:
            case MotherBrainBodyPhase.SecondPhaseHandBeam:
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyPhase.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOff:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUp:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyPhase.ThirdPhaseFightingMain:
            case MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyPhase.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing:
            default:
                throw new InvalidDataException(
                    $"Mother Brain fake-death has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Waits through the fake-death pause before control is locked for the next phase.</summary>
    private void RunMotherBrainPauseBeforeLock(MotherBrainEnemyState state, SamusState? samus)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        // The arena lock loads two scroll cells, masks the second, then stores both. This
        // preserves scroll zero and clears scroll one, locking the arena to screen zero.
        SamusState requiredSamus = samus ?? throw new InvalidOperationException(
            "Mother Brain's fake-death input lock requires the active Samus state.");
        // The arena lock runs Samus command zero, the stationary lock: its alpha handler places
        // no bombs and fires nothing new while the arena drains.
        requiredSamus.Control.LockStationary();
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        state.Phase = MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic;
        state.PhaseTimer = 32;
        RunMotherBrainFakeDeath(state, samus);
    }

    /// <summary>
    /// Runs Mother Brain's room flash: each of its fourteen frames stays two updates, the timer counting
    /// up from one, and the last loops to the first.
    /// </summary>
    internal static void RunMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        if (state.RoomFlashFrame is not int frame)
            return;

        if (state.RoomFlashTimer == MotherBrainRoomPaletteAnimationDefinitions.FlashImageTicks)
        {
            frame = (frame + 1) % MotherBrainRoomPaletteAnimationDefinitions.FlashFrameCount;
            state.RoomFlashFrame = frame;
            state.RoomFlashTimer = 0;
        }
        state.RoomFlashTimer++;
        state.Colours.ShowRoomFlash(frame);
    }

    /// <summary>Dispatches the phase-two ascent without collapsing same-frame fallthroughs.</summary>
    private void RunMotherBrainPhaseTwoAscent(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        switch (state.Phase)
        {
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain:
                MotherBrainAscent.SetupMotherBrainPhaseTwoBrain(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense:
                MotherBrainAscent.PauseBeforeMotherBrainRises(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentPrepareForRising:
                MotherBrainAscent.PrepareMotherBrainForRising(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody:
                MotherBrainAscent.ReadyMotherBrainStandingBody(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentContinuePausing:
                MotherBrainAscent.ContinueMotherBrainAscentPause(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake:
                Ascent.StartMotherBrainAscent(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain:
                Ascent.RaiseMotherBrain(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched:
                MotherBrainAscent.WaitForMotherBrainToFinishUncrouching(state);
                return;
            case MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray:
                MotherBrainFakeDeath.TransitionMotherBrainFromGray(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseStretchingShakeHead:
                MotherBrainAscent.ShakeMotherBrainHeadMenacingly(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp:
                MotherBrainAscent.BringMotherBrainHeadBackUp(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseStretchingFinish:
                MotherBrainAscent.FinishMotherBrainStretching(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseThinking:
                Attacks.RunMotherBrainSecondPhaseThinking(state, samus, sharedProjectiles);
                return;
            case MotherBrainBodyPhase.SecondPhaseTryAttack:
                Attacks.RunMotherBrainSecondPhaseTryAttack(state, samus);
                return;
            case MotherBrainBodyPhase.SecondPhaseBombDecideWalking:
                Attacks.DecideMotherBrainBombWalking(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards:
                Attacks.ContinueMotherBrainBombWalk(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseBombCrouch:
                MotherBrainAttacks.CrouchMotherBrainForBomb(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseBombFired:
                MotherBrainAttacks.WaitAfterMotherBrainBomb(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseBombStandUp:
                MotherBrainAttacks.StandMotherBrainAfterBomb(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly:
                MotherBrainAttacks.PositionMotherBrainHeadQuicklyForLaser(
                    state,
                    MotherBrainAttacks.RequireMotherBrainCombatSamus(samus));
                return;
            case MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire:
                MotherBrainAttacks.PositionMotherBrainHeadSlowlyAndFireLaser(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseLaserFinishAttack:
                Attacks.FinishMotherBrainLaserAttack(state, samus);
                return;
            case MotherBrainBodyPhase.SecondPhaseHandBeam:
                MotherBrainAttacks.RunMotherBrainHandBeamAttack(state);
                return;
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyPhase.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOff:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUp:
            case MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyPhase.ThirdPhaseFightingMain:
            case MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyPhase.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing:
                RainbowAndDeath.RunLiveMotherBrainRainbowBeam(
                    state,
                    MotherBrainAttacks.RequireMotherBrainCombatSamus(samus),
                    state.Body.FrameCounter,
                    sharedProjectiles);
                return;
            case MotherBrainBodyPhase.FirstPhase:
            case MotherBrainBodyPhase.FakeDeathDescentInitialPause:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock:
            case MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash:
            case MotherBrainBodyPhase.FakeDeathDescentFadeToGray:
            case MotherBrainBodyPhase.FakeDeathDescentCollapseTubes:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10:
            case MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12:
            case MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics:
            default:
                throw new InvalidDataException(
                    $"Mother Brain phase-two has no behaviour for phase {state.Phase}.");
        }
    }
    /// <summary>Six authored drool spawn offsets from the brain's center.</summary>
    internal static readonly Offset16<RoomPixels>[] MotherBrainDroolOffsets =
        [new(6, 20), new(14, 18), new(8, 23), new(10, 19), new(11, 25), new(12, 18)];
}
