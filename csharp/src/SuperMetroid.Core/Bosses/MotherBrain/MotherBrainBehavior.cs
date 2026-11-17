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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainTubes];

    /// <summary>Mother Brain's body, her head, or a tube of her fake death (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Mother Brain definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.MotherBrainBody)
            return new MotherBrainBodyEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.MotherBrainHead)
            return new MotherBrainHeadEntity(this, place, common);
        if (definition == EnemyDefinitionId.MotherBrainTubes)
            return new MotherBrainTubesEntity(this, place, common);
        throw new InvalidOperationException($"The Mother Brain species creates no {place.DefinitionId}.");
    }
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Draws encounter artwork and applies installed colours.</summary>
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

    /// <summary>Mother Brain's rainbow beam, its colours, and her death.</summary>
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
        Access = new MotherBrainEncounterAccess(_frameResults);
        Head = new MotherBrainHead(new MotherBrainHeadDependencies { Room = _room, Places = dependencies.Places, Projectiles = dependencies.Projectiles, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access  });
        Body = new MotherBrainBody(new MotherBrainBodyDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , Presentation = _presentation, Places = dependencies.Places });
        FakeDeath = new MotherBrainFakeDeath(new MotherBrainFakeDeathDependencies { FrameResults = _frameResults, Lifecycle = dependencies.Lifecycle, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, Access = Access, ProjectileAttacks = Projectiles });
        Ascent = new MotherBrainAscent(_room, dependencies.Projectiles, dependencies.Earthquake, _presentation, dependencies.FrameContext, _motherBrainEncounter);
        RainbowAndDeath = new MotherBrainRainbowAndDeath(new MotherBrainRainbowAndDeathDependencies { FrameResults = _frameResults, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , BossArtwork = _bossArtwork });
        Attacks = new MotherBrainAttacks(_room);
        Combat = new MotherBrainCombat(_room, dependencies.Projectiles, dependencies.Audio, dependencies.Collision);
    }



    /// <summary>Runs the common body main/hurt entry.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemyPlace body,
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles)
    {
        _ = Ensure.NotNull(body);
        // The body main advances the independent room-colour animation before it
        // runs the body phase. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomFlash(state);
        RunMotherBrainBodyPhase(state, samus, sharedProjectiles);
        // Every body phase returns through the common contact tail. The first-phase
        // phase also has its own authored contact call; retain both sites.
        MotherBrainBody.ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>Runs the stage Mother Brain's body is in (#2208), keeping its phase's own timer and attack order.</summary>
    private void RunMotherBrainBodyPhase(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles)
    {
        switch (state.Stage)
        {
            case MotherBrainBodyStage.Glass:
                Body.RunMotherBrainFirstPhase(state, samus);
                return;
            case MotherBrainBodyStage.FakeDeathDescent:
                RunMotherBrainFakeDeath(state, samus);
                return;
            case MotherBrainBodyStage.FakeDeathAscent:
                RunMotherBrainAscent(state);
                return;
            case MotherBrainBodyStage.Phase2Fight:
                RunMotherBrainPhase2Fight(state, samus);
                return;
            case MotherBrainBodyStage.RainbowBeam:
            case MotherBrainBodyStage.FinishingSamusOff:
            case MotherBrainBodyStage.DrainedByBaby:
            case MotherBrainBodyStage.Reviving:
            case MotherBrainBodyStage.Phase3Fight:
            case MotherBrainBodyStage.Dying:
                RainbowAndDeath.RunLiveMotherBrainRainbowBeam(
                    state,
                    samus,
                    state.Body.FrameCounter,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain's body has no behaviour for its {state.Stage} stage.");
        }
    }

    /// <summary>Runs the phase of her fake death's descent her body is in, after her first phase ends.</summary>
    internal void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState samus)
    {
        switch (state.DescentPhase)
        {
            case MotherBrainDescentPhase.InitialPause:
                // The fake death's start begins the pause before the lock, whose first update is this same update.
                state.DescentPhase = MotherBrainDescentPhase.PauseBeforeLock;
                state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeLockUpdates);
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainDescentPhase.PauseBeforeLock:
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainDescentPhase.PauseBeforeMusic:
                if (!state.PhaseTimer.StepRunsOut())
                    return;
                _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
                _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.MotherBrain), MusicQueueDelay.Shortest);
                state.DescentPhase = MotherBrainDescentPhase.PauseBeforeUnlock;
                state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeUnlockUpdates);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainDescentPhase.PauseBeforeUnlock:
                if (!state.PhaseTimer.StepRunsOut())
                    return;
                // Restoring Samus's control reinstalls the normal input and movement modes.
                samus.Control.Unlock();
                state.DescentPhase = MotherBrainDescentPhase.PauseBeforeFlash;
                state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeFlashUpdates);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainDescentPhase.PauseBeforeFlash:
                if (!state.PhaseTimer.StepRunsOut())
                    return;

                // The fake-death flash starts the looping room flash, drains the room's acid,
                // arms the head's independent tube sequence, and clears both fade timers.
                state.RoomFlashImage = MotherBrainRoomFlashImage.First;
                state.RoomFlashTimer = MotherBrainRoomFlashDefinitions.FirstImageStartingTicks;
                _frameResults.MotherBrainAcidChange = MotherBrainRoomFxDefinitions.AcidDrains;
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.SpawnBottomLeftTube;
                state.TubeCollapseTimer.LastFor(1);
                state.DescentPhase = MotherBrainDescentPhase.FadeToGray;
                state.PhaseTimer.LastFor(1);
                state.FakeDeathGreying.Restart();
                Access.RequestMotherBrainObject(MotherBrainTubeCollapseDefinitions.CeilingBlock.Block, MotherBrainTubeCollapseDefinitions.CeilingBlock.RoomObject);
                return;

            case MotherBrainDescentPhase.FadeToGray:
                FakeDeath.RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainDescentPhase.CollapseTubes:
                FakeDeath.RunMotherBrainTubeCollapse(state);
                FakeDeath.RunMotherBrainFakeDeathExplosion(state);
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain's fake death's descent has no behaviour for phase {state.DescentPhase}.");
        }
    }

    /// <summary>Waits through the fake-death pause before control is locked for the next phase.</summary>
    private void RunMotherBrainPauseBeforeLock(MotherBrainEnemyState state, SamusState samus)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        // The arena lock keeps the first screen of the room's row of four and closes the second,
        // locking the arena to the first screen.
        // The arena lock is the stationary lock: its input phase places
        // no bombs and fires nothing new while the arena drains.
        samus.Control.LockStationary();
        _room.Services.RoomScroll.Write(MotherBrainFightDefinitions.ArenaClosedScreen, RoomScrollState.Closed);

        state.DescentPhase = MotherBrainDescentPhase.PauseBeforeMusic;
        state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeMusicUpdates);
        RunMotherBrainFakeDeath(state, samus);
    }

    /// <summary>
    /// Runs Mother Brain's room flash: each of its fourteen images stays two updates, the timer counting
    /// up from one, and the last loops to the first.
    /// </summary>
    internal static void RunMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        if (state.RoomFlashImage is not { } image)
            return;

        if (state.RoomFlashTimer == MotherBrainRoomFlashDefinitions.FlashImageTicks)
        {
            image = image.Next;
            state.RoomFlashImage = image;
            state.RoomFlashTimer = 0;
        }
        state.RoomFlashTimer++;
        state.Colours.ShowRoomFlash(image);
    }

    /// <summary>Runs the phase of her rise out of the fake death her body is in, from the mural to her colours' return.</summary>
    private void RunMotherBrainAscent(MotherBrainEnemyState state)
    {
        switch (state.AscentPhase)
        {
            case MotherBrainAscentPhase.DrawMural:
                FakeDeath.DrawNextMuralRows(state);
                return;
            case MotherBrainAscentPhase.SetupPhase2Graphics:
                Ascent.SetupMotherBrainPhaseTwoGraphics(state);
                return;
            case MotherBrainAscentPhase.SetupPhase2Brain:
                MotherBrainAscent.SetupMotherBrainPhaseTwoBrain(state);
                return;
            case MotherBrainAscentPhase.PauseForSuspense:
                MotherBrainAscent.PauseBeforeMotherBrainRises(state);
                return;
            case MotherBrainAscentPhase.PrepareForRising:
                MotherBrainAscent.PrepareMotherBrainForRising(state);
                return;
            case MotherBrainAscentPhase.ReadyStandingBody:
                MotherBrainAscent.ReadyMotherBrainStandingBody(state);
                return;
            case MotherBrainAscentPhase.ContinuePausing:
                MotherBrainAscent.ContinueMotherBrainAscentPause(state);
                return;
            case MotherBrainAscentPhase.StartMusicAndEarthquake:
                Ascent.StartMotherBrainAscent(state);
                return;
            case MotherBrainAscentPhase.RaiseMotherBrain:
                Ascent.RaiseMotherBrain(state);
                return;
            case MotherBrainAscentPhase.WaitUntilUncrouched:
                MotherBrainAscent.WaitForMotherBrainToFinishUncrouching(state);
                return;
            case MotherBrainAscentPhase.TransitionFromGray:
                MotherBrainFakeDeath.TransitionMotherBrainFromGray(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain's rise has no behaviour for phase {state.AscentPhase}.");
        }
    }

    /// <summary>
    /// Runs the phase of her second phase's fight her body is in, from her stretch to her attacks, until her health first
    /// runs out and her thinking starts the rainbow beam.
    /// </summary>
    private void RunMotherBrainPhase2Fight(MotherBrainEnemyState state, SamusState samus)
    {
        switch (state.Phase2FightPhase)
        {
            case MotherBrainPhase2FightPhase.StretchingShakeHead:
                MotherBrainAscent.ShakeMotherBrainHeadMenacingly(state);
                return;
            case MotherBrainPhase2FightPhase.StretchingBringHeadUp:
                MotherBrainAscent.BringMotherBrainHeadBackUp(state);
                return;
            case MotherBrainPhase2FightPhase.StretchingFinish:
                MotherBrainAscent.FinishMotherBrainStretching(state);
                return;
            case MotherBrainPhase2FightPhase.Thinking:
                Attacks.RunMotherBrainSecondPhaseThinking(state);
                return;
            case MotherBrainPhase2FightPhase.TryAttack:
                Attacks.RunMotherBrainSecondPhaseTryAttack(state, samus);
                return;
            case MotherBrainPhase2FightPhase.BombDecideWalking:
                Attacks.DecideMotherBrainBombWalking(state);
                return;
            case MotherBrainPhase2FightPhase.BombWalkingBackwards:
                Attacks.ContinueMotherBrainBombWalk(state);
                return;
            case MotherBrainPhase2FightPhase.BombCrouch:
                MotherBrainAttacks.CrouchMotherBrainForBomb(state);
                return;
            case MotherBrainPhase2FightPhase.BombFired:
                MotherBrainAttacks.WaitAfterMotherBrainBomb(state);
                return;
            case MotherBrainPhase2FightPhase.BombStandUp:
                MotherBrainAttacks.StandMotherBrainAfterBomb(state);
                return;
            case MotherBrainPhase2FightPhase.LaserPositionHeadQuickly:
                MotherBrainAttacks.PositionMotherBrainHeadQuicklyForLaser(
                    state,
                    samus);
                return;
            case MotherBrainPhase2FightPhase.LaserPositionHeadSlowlyAndFire:
                MotherBrainAttacks.PositionMotherBrainHeadSlowlyAndFireLaser(state);
                return;
            case MotherBrainPhase2FightPhase.LaserFinishAttack:
                Attacks.FinishMotherBrainLaserAttack(state);
                return;
            case MotherBrainPhase2FightPhase.HandBeam:
                MotherBrainAttacks.RunMotherBrainHandBeamAttack(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain's second-phase fight has no behaviour for phase {state.Phase2FightPhase}.");
        }
    }
}
