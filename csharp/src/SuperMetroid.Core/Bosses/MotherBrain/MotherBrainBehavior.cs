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

    /// <summary>Mother Brain's head and brain: their setup, neck, programs, cues and drawing.</summary>
    internal MotherBrainHead Head { get; }

    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and room-object requests.</summary>
    internal MotherBrainEncounterAccess Access { get; }

    /// <summary>Mother Brain's body and tank: their setup, programs, movement, footsteps and the first phase.</summary>
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



    /// <summary>Ports the common body main/hurt entry.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainEnemyState state = Access.RequireCompleteMotherBrainState(body);

        // The body main advances the independent room-palette animation script before it
        // dispatches the body function. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomFlash(state);
        RunMotherBrainBodyFunction(state, samus, sharedProjectiles);
        // Every body function returns through the common contact tail. The first-phase
        // function also has its own authored contact call; retain both sites.
        if (samus is not null)
            MotherBrainBody.ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>Dispatches the active body phase while preserving the phase-specific timer and attack order.</summary>
    private void RunMotherBrainBodyFunction(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FirstPhase:
                Body.RunMotherBrainFirstPhase(state, samus);
                return;
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows1And2:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows3And4:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows5And6:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows7And8:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows9And10:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows11And12:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                RunMotherBrainFakeDeath(state, samus);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyFunction.FakeDeathAscentReadyStandingBody:
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
            case MotherBrainBodyFunction.SecondPhaseThinking:
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
            case MotherBrainBodyFunction.SecondPhaseBombFired:
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                RunMotherBrainPhaseTwoAscent(
                    state,
                    samus,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain body function {state.Function} is not translated.");
        }
    }

    /// <summary>Four authored jitter offsets selected by the low shake-state bits.</summary>
    internal static readonly Offset16<RoomPixels>[] MotherBrainShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Advances MotherBrainBody's typed animation.</summary>
    internal void AdvanceMotherBrainBodyTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        Body.AdvanceMotherBrainSlotAnimation(slot, context.Samus);
    }

    /// <summary>Ports MotherBrainBody's main AI.</summary>
    internal void RunMotherBrainBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMotherBrainBodyMain(slot, frame.Samus, frame.SharedProjectiles);
    }

    /// <summary>Ports MotherBrainHead's main AI.</summary>
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
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
                // The fake-death start installs the following countdown and falls through to it. The
                // first observable value is therefore 63, not the host-friendly value 64.
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
                state.FunctionTimer = 64;
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;
                _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
                _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.MotherBrain), MusicQueueDelay.Shortest);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock;
                state.FunctionTimer = 12;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;
                // Samus command one restores the normal handler pair.
                samus?.Control.Unlock();
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash;
                state.FunctionTimer = 8;
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;

                // The fake-death flash starts the looping room flash, drains the room's acid,
                // arms the head's independent tube sequence, and clears both fade timers.
                state.RoomFlashFrame = 0;
                state.RoomFlashTimer = 1;
                _frameResults.MotherBrainAcidChange = MotherBrainRoomFxDefinitions.AcidDrains;
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
                state.TubeCollapseTimer = 0;
                state.Function = MotherBrainBodyFunction.FakeDeathDescentFadeToGray;
                state.FunctionTimer = 0;
                state.GrayFadeIndex = 0;
                Access.RequestMotherBrainObject(new(14, 2), MotherBrainRoomMutation.ClearCeilingBlock);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
                FakeDeath.RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
                FakeDeath.RunMotherBrainTubeCollapse(state);
                FakeDeath.RunMotherBrainFakeDeathExplosion(state);
                return;

            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows1And2:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow1,
                    MotherBrainRoomMutation.MuralRow2,
                    MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows3And4);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows3And4:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow3,
                    MotherBrainRoomMutation.MuralRow4,
                    MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows5And6);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows5And6:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow5,
                    MotherBrainRoomMutation.MuralRow6,
                    MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows7And8);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows7And8:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow7,
                    MotherBrainRoomMutation.MuralRow8,
                    MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows9And10);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows9And10:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow9,
                    MotherBrainRoomMutation.MuralRow10,
                    MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows11And12);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows11And12:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    MotherBrainRoomMutation.MuralRow11,
                    MotherBrainRoomMutation.MuralRow12,
                    MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                Ascent.SetupMotherBrainPhaseTwoGraphics(state);
                return;
            case MotherBrainBodyFunction.FirstPhase:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyFunction.FakeDeathAscentReadyStandingBody:
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
            case MotherBrainBodyFunction.SecondPhaseThinking:
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
            case MotherBrainBodyFunction.SecondPhaseBombFired:
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
            default:
                throw new InvalidDataException(
                    $"Mother Brain fake-death function {state.Function} is not translated.");
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

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic;
        state.FunctionTimer = 32;
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

        if (state.RoomFlashTimer == MotherBrainRoomPaletteProgramDefinitions.FlashImageTicks)
        {
            frame = (frame + 1) % MotherBrainRoomPaletteProgramDefinitions.FlashFrameCount;
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
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                MotherBrainAscent.SetupMotherBrainPhaseTwoBrain(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                MotherBrainAscent.PauseBeforeMotherBrainRises(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                MotherBrainAscent.PrepareMotherBrainForRising(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentReadyStandingBody:
                MotherBrainAscent.ReadyMotherBrainStandingBody(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
                MotherBrainAscent.ContinueMotherBrainAscentPause(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
                Ascent.StartMotherBrainAscent(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
                Ascent.RaiseMotherBrain(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
                MotherBrainAscent.WaitForMotherBrainToFinishUncrouching(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
                MotherBrainFakeDeath.TransitionMotherBrainFromGray(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
                MotherBrainAscent.ShakeMotherBrainHeadMenacingly(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
                MotherBrainAscent.BringMotherBrainHeadBackUp(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
                MotherBrainAscent.FinishMotherBrainStretching(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseThinking:
                Attacks.RunMotherBrainSecondPhaseThinking(state, samus, sharedProjectiles);
                return;
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
                Attacks.RunMotherBrainSecondPhaseTryAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
                Attacks.DecideMotherBrainBombWalking(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
                Attacks.ContinueMotherBrainBombWalk(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
                MotherBrainAttacks.CrouchMotherBrainForBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombFired:
                MotherBrainAttacks.WaitAfterMotherBrainBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
                MotherBrainAttacks.StandMotherBrainAfterBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
                MotherBrainAttacks.PositionMotherBrainHeadQuicklyForLaser(
                    state,
                    MotherBrainAttacks.RequireMotherBrainCombatSamus(samus));
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
                MotherBrainAttacks.PositionMotherBrainHeadSlowlyAndFireLaser(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
                Attacks.FinishMotherBrainLaserAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
                MotherBrainAttacks.RunMotherBrainHandBeamAttack(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffReadyBaby:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyCorpse:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathReadyEscapeWarning:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                RainbowAndDeath.RunLiveMotherBrainRainbowBeam(
                    state,
                    MotherBrainAttacks.RequireMotherBrainCombatSamus(samus),
                    state.Body.FrameCounter,
                    sharedProjectiles);
                return;
            case MotherBrainBodyFunction.FirstPhase:
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows1And2:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows3And4:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows5And6:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows7And8:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows9And10:
            case MotherBrainBodyFunction.FakeDeathAscentDrawMuralRows11And12:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
            default:
                throw new InvalidDataException(
                    $"Mother Brain phase-two function {state.Function} is not translated.");
        }
    }
    /// <summary>Six authored drool spawn offsets from the brain's center.</summary>
    internal static readonly Offset16<RoomPixels>[] MotherBrainDroolOffsets =
        [new(6, 20), new(14, 18), new(8, 23), new(10, 19), new(11, 25), new(12, 18)];
}
