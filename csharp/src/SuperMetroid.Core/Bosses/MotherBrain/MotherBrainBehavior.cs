using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Mother Brain species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
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
        Body = new MotherBrainBody(new MotherBrainBodyDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Presentation = _presentation, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , BossArtwork = _bossArtwork });
        FakeDeath = new MotherBrainFakeDeath(new MotherBrainFakeDeathDependencies { FrameResults = _frameResults, Dispatch = dependencies.Dispatch, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Presentation = _presentation, MotherBrainEncounter = _motherBrainEncounter, Access = Access , BossArtwork = _bossArtwork });
        Ascent = new MotherBrainAscent(_room, dependencies.Projectiles, dependencies.Earthquake, _presentation, dependencies.FrameContext, _motherBrainEncounter, _bossArtwork);
        RainbowAndDeath = new MotherBrainRainbowAndDeath(new MotherBrainRainbowAndDeathDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , BossArtwork = _bossArtwork });
        Attacks = new MotherBrainAttacks(_room, RainbowAndDeath);
        Combat = new MotherBrainCombat(_room, dependencies.Projectiles, dependencies.Audio, dependencies.Collision, _motherBrainEncounter);
    }



    /// <summary>Ports the common body main/hurt entry at <c>$A9:873E</c>.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainEnemyState state = Access.RequireCompleteMotherBrainState(body);

        // `$A9:873E` advances the independent room-palette instruction list before it
        // dispatches the body function. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomPalette(state);
        RunMotherBrainBodyFunction(state, samus, sharedProjectiles);
        // Every body function returns through the common contact tail. The first-phase
        // function also has its own authored contact call; retain both native sites.
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
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                RunMotherBrainFakeDeath(state, samus);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
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
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
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
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
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
    /// <summary>Native sequence of explosion positions used by the fake-death collapse.</summary>
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
    /// <summary>$A9:8C61: authored smoke puff X offsets cycled while a tube lands.</summary>
    internal static readonly int[] MotherBrainFallingTubeSmokeXOffsets = [-8, 2, -4, 6];

    /// <summary>Dispatches the body pointer after the phase-one function changes to $881D.</summary>
    internal void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
                // `$A9:881D` installs the following countdown and falls through to it. The
                // first observable value is therefore 63, not the host-friendly value 64.
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
                state.FunctionTimer = WrappingCountdown.Start(64);
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
                state.FunctionTimer = WrappingCountdown.Start(12);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;
                // $A9:8874: Samus command one restores the normal handler pair.
                samus?.Control.Unlock();
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash;
                state.FunctionTimer = WrappingCountdown.Start(8);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
                if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
                    return;

                // `$A9:88A0-$88AF` starts the looping room flash, selects FX entry two,
                // arms the head's independent tube sequence, and clears both fade words.
                state.RoomFlashFrame = 0;
                state.RoomFlashTimer = 1;
                _frameResults.MotherBrainFxEntry = 2;
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
                state.TubeCollapseTimer = WrappingCountdown.Start(0);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentFadeToGray;
                state.FunctionTimer = WrappingCountdown.Start(0);
                state.GrayFadeIndex = 0;
                Access.RequestMotherBrainPlm(new(14, 2), PlmHeaderId.ClearMotherBrainCeilingBlock);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
                FakeDeath.RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
                FakeDeath.RunMotherBrainTubeCollapse(state);
                FakeDeath.RunMotherBrainFakeDeathExplosion(state);
                return;

            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow2,
                    PlmHeaderId.MotherBrainsBackgroundRow3,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow4,
                    PlmHeaderId.MotherBrainsBackgroundRow5,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow6,
                    PlmHeaderId.MotherBrainsBackgroundRow7,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow8,
                    PlmHeaderId.MotherBrainsBackgroundRow9,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRowA,
                    PlmHeaderId.MotherBrainsBackgroundRowB,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
                FakeDeath.RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRowC,
                    PlmHeaderId.MotherBrainsBackgroundRowD,
                    MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                Ascent.SetupMotherBrainPhaseTwoGraphics(state);
                return;
            case MotherBrainBodyFunction.FirstPhase:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
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
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
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
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
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

        // $A9:8836 loads TWO bytes, masks with $00FF, then stores TWO bytes. This
        // preserves scroll zero and clears scroll one, locking the arena to screen zero.
        SamusState requiredSamus = samus ?? throw new InvalidOperationException(
            "Mother Brain's fake-death input lock requires the active Samus state.");
        // $A9:8832 runs Samus command zero, the stationary lock: its alpha handler places
        // no bombs and fires nothing new while the arena drains.
        requiredSamus.Control.LockStationary();
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic;
        state.FunctionTimer = WrappingCountdown.Start(32);
        RunMotherBrainFakeDeath(state, samus);
    }

    /// <summary>
    /// Runs Mother Brain's room flash ($A9:D192): each of its fourteen frames stays two updates, the timer counting
    /// up from one, and the last loops to the first.
    /// </summary>
    internal void RunMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        if (state.RoomFlashFrame is not int frame)
            return;

        if (state.RoomFlashTimer == MotherBrainRoomPaletteProgramDefinitions.FlashImageTicks)
        {
            frame = (frame + 1) % MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount;
            state.RoomFlashFrame = frame;
            state.RoomFlashTimer = 0;
        }
        state.RoomFlashTimer++;
        (_bossArtwork.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room flash requires installed colors."))
            .ApplyFlash(_presentation.Colours, frame);
    }
    /// <summary>Native X positions cycled when Mother Brain's ascent emits floor dust.</summary>
    internal static readonly ushort[] MotherBrainAscentDustXPositions =
        [61, 84, 32, 53, 90, 67, 103, 41];

    /// <summary>Dispatches $A9:8D49-$8F45 without collapsing native same-frame fallthroughs.</summary>
    private void RunMotherBrainPhaseTwoAscent(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                Ascent.SetupMotherBrainPhaseTwoBrain(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                Ascent.PauseBeforeMotherBrainRises(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                Ascent.PrepareMotherBrainForRising(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
                Ascent.LoadMotherBrainLegTiles(state);
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
                FakeDeath.TransitionMotherBrainFromGray(state);
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
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
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
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                RainbowAndDeath.RunLiveMotherBrainRainbowBeam(
                    state,
                    MotherBrainAttacks.RequireMotherBrainCombatSamus(samus),
                    WordWrap.LowByte(state.Body.FrameCounter),
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
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
            default:
                throw new InvalidDataException(
                    $"Mother Brain phase-two function {state.Function} is not translated.");
        }
    }
    /// <summary>Six authored drool spawn offsets from the brain's center.</summary>
    internal static readonly Offset16<RoomPixels>[] MotherBrainDroolOffsets =
        [new(6, 20), new(14, 18), new(8, 23), new(10, 19), new(11, 25), new(12, 18)];

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        _motherBrainEncounter.MotherBrain?.BeginFrame();
    }
}
