using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Mother Brain species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed partial class MotherBrainBehavior : IEnemySpecies, IEnemyJoiningFactory<MotherBrainEnemyState, MotherBrainTubeSpawnSettings>
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;

    /// <summary>A tube of her fake death, which joins the encounter whose fake death drops it (#2146, #2218).</summary>
    [field: NonSerialized]
    public EnemyOccupant CreateJoining(RoomEnemyOccupant occupant, MotherBrainTubeSpawnSettings settings, RoomEnemyPlace place,
        EnemyCommonParticipation common, MotherBrainEnemyState encounter) =>
        EnemyOccupant.Running(new MotherBrainTubesEntity(this, occupant, place, common, settings, encounter));

    /// <summary>
    /// Mother Brain's body (#2146), placed in a room whose tileset has the warning picture sheet: her death types Zebes's
    /// warning on <paramref name="zebesWarning"/>, the room's background warning (#2554).
    /// </summary>
    internal EnemyOccupant CreateBody(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        RoomWarning zebesWarning) =>
        EnemyOccupant.Running(new MotherBrainBodyEntity(this, occupant, place, common, _bossArtwork, zebesWarning));
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Draws encounter artwork and applies installed colours.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Where her body offers her head the creation that builds it joining her encounter.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>
    /// Sets her body up, beginning the encounter, and offers her head, which the room lists after her body, the creation
    /// that builds it joining that encounter (#2218, #2554).
    /// </summary>
    /// <param name="body">Her body.</param>
    /// <param name="zebesWarning">The room's background warning, which her death types Zebes's warning on.</param>
    /// <returns>The encounter, which the body holds from its setup.</returns>
    internal MotherBrainEnemyState BeginEncounter(RoomEnemyOccupant body, RoomWarning zebesWarning)
    {
        MotherBrainEnemyState state = Body.InitializeMotherBrainBody(body, zebesWarning);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.MotherBrainHead, new HeadJoining(this, state).CreateHead);
        return state;
    }

    /// <summary>The creation her body offers her head (#2554): the head joining the encounter her body began.</summary>
    /// <param name="species">Mother Brain's species owner.</param>
    /// <param name="encounter">The encounter her body began.</param>
    private sealed partial class HeadJoining(MotherBrainBehavior species, MotherBrainEnemyState encounter)
    {
        /// <summary>Her head, joining her encounter.</summary>
        internal EnemyOccupant CreateHead(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(MotherBrainHeadEntity.SetUp(species, occupant, place, common, encounter));
    }
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
        _passEvents = dependencies.PassEvents;
        _room = dependencies.Room;
        _presentation = dependencies.Presentation.Surfaces;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _partOffers = dependencies.PartOffers;
        Projectiles = new MotherBrainProjectiles(_room, dependencies.Projectiles, dependencies.Earthquake, dependencies.DeathsAndDrops, _motherBrainEncounter, dependencies.RoomActors);
        Access = new MotherBrainEncounterAccess(_passEvents);
        Head = new MotherBrainHead(new MotherBrainHeadDependencies { Room = _room, Places = dependencies.Places, Projectiles = dependencies.Projectiles, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles });
        Body = new MotherBrainBody(new MotherBrainBodyDependencies { PassEvents = _passEvents, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , Presentation = _presentation, Places = dependencies.Places });
        FakeDeath = new MotherBrainFakeDeath(new MotherBrainFakeDeathDependencies { PassEvents = _passEvents, Lifecycle = dependencies.Lifecycle, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, MotherBrainEncounter = _motherBrainEncounter, Access = Access, ProjectileAttacks = Projectiles, Tubes = this });
        Ascent = new MotherBrainAscent(_room, dependencies.Projectiles, dependencies.Earthquake, _presentation, dependencies.FrameContext, _motherBrainEncounter);
        RainbowAndDeath = new MotherBrainRainbowAndDeath(new MotherBrainRainbowAndDeathDependencies { PassEvents = _passEvents, Room = _room, Projectiles = dependencies.Projectiles, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Presentation = _presentation, FrameContext = dependencies.FrameContext, MotherBrainEncounter = _motherBrainEncounter, RoomActors = dependencies.RoomActors, ProjectileAttacks = Projectiles, Access = Access , BossArtwork = _bossArtwork });
        Attacks = new MotherBrainAttacks(_room);
        Combat = new MotherBrainCombat(_room, dependencies.Projectiles, dependencies.Audio, dependencies.Collision);
    }

    /// <summary>Runs her room flash, her body's stage and her contact with Samus.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemyPlace body,
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem samusBombs)
    {
        _ = Ensure.NotNull(body);
        // The body main advances the independent room-colour animation before it
        // runs the body phase. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomFlash(state);
        RunMotherBrainBodyPhase(state, samus, samusBombs);
        // Every body phase ends with her contact with Samus; her first phase also checks contact within its own phase,
        // so it checks twice.
        MotherBrainBody.ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>
    /// Runs the stage Mother Brain's body is in (#2208), keeping its phase's own timer and attack order: her late fight,
    /// once begun, runs every stage from the rainbow beam on (#2471).
    /// </summary>
    private void RunMotherBrainBodyPhase(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem samusBombs)
    {
        if (state.RainbowBeamSequence is { } lateFight)
        {
            RainbowAndDeath.RunLiveMotherBrainRainbowBeam(lateFight, samus, state.Body.FrameCounter, samusBombs);
            return;
        }
        // Before her late fight her body runs one of its own stages.
        if (state.Stage == MotherBrainBodyStage.Glass)
            Body.RunMotherBrainFirstPhase(state, samus);
        else if (state.Stage == MotherBrainBodyStage.FakeDeathDescent)
            RunMotherBrainFakeDeath(state, samus);
        else if (state.Stage == MotherBrainBodyStage.FakeDeathAscent)
            RunMotherBrainAscent(state);
        else
        {
            // The remaining member, MotherBrainBodyStage.Phase2Fight.
            RunMotherBrainPhase2Fight(state, samus);
        }
    }

    /// <summary>What <see cref="RunMotherBrainFakeDeath"/> does for each <see cref="MotherBrainDescentPhase"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainDescentPhase, Action<MotherBrainEnemyState, SamusState>> RunMotherBrainFakeDeathForMotherBrainDescentPhase => field ??= new(new Dictionary<MotherBrainDescentPhase, Action<MotherBrainEnemyState, SamusState>>
    {
        [MotherBrainDescentPhase.InitialPause] = (state, samus) =>
        {
            // The fake death's start begins the pause before the lock, whose first update is this same update.
            state.DescentPhase = MotherBrainDescentPhase.PauseBeforeLock;
            state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeLockUpdates);
            RunMotherBrainPauseBeforeLock(state, samus);
        },
        [MotherBrainDescentPhase.PauseBeforeLock] = (state, samus) => RunMotherBrainPauseBeforeLock(state, samus),
        [MotherBrainDescentPhase.PauseBeforeMusic] = (state, samus) =>
        {
            if (!state.PhaseTimer.StepRunsOut())
                return;
            _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
            _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.MotherBrain), MusicQueueDelay.Shortest);
            state.DescentPhase = MotherBrainDescentPhase.PauseBeforeUnlock;
            state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeUnlockUpdates);
            RunMotherBrainFakeDeath(state, samus);
        },
        [MotherBrainDescentPhase.PauseBeforeUnlock] = (state, samus) =>
        {
            if (!state.PhaseTimer.StepRunsOut())
                return;
            // Samus's control returns to normal.
            samus.Control.Unlock();
            state.DescentPhase = MotherBrainDescentPhase.PauseBeforeFlash;
            state.PhaseTimer.LastFor(MotherBrainWaitDefinitions.BeforeFlashUpdates);
            RunMotherBrainFakeDeath(state, samus);
        },
        [MotherBrainDescentPhase.PauseBeforeFlash] = (state, samus) =>
        {
            if (!state.PhaseTimer.StepRunsOut())
                return;

            // The fake-death flash starts the looping room flash, drains the room's acid,
            // arms the head's independent tube sequence, and clears both fade timers.
            state.RoomFlash = MotherBrainRoomFlashAnimations.Starting();
            _passEvents.Publish(new MotherBrainAcidChange(MotherBrainRoomFxDefinitions.AcidDrains));
            state.TubeCollapse = MotherBrainTubeCollapseProgress.Start;
            state.TubeCollapseTimer.LastFor(1);
            state.DescentPhase = MotherBrainDescentPhase.FadeToGray;
            state.FakeDeathGreying.StartAfter(1);
            Access.RequestMotherBrainObject(MotherBrainTubeCollapseDefinitions.CeilingBlock.Block, MotherBrainTubeCollapseDefinitions.CeilingBlock.RoomObject);
        },
        [MotherBrainDescentPhase.FadeToGray] = (state, samus) => FakeDeath.RunMotherBrainFadeToGray(state),
        [MotherBrainDescentPhase.CollapseTubes] = (state, samus) =>
        {
            FakeDeath.RunMotherBrainTubeCollapse(state);
            FakeDeath.RunMotherBrainFakeDeathExplosion(state);
        },
    });

    /// <summary>Runs the phase of her fake death's descent her body is in, after her first phase ends.</summary>
    internal void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState samus)
    {
        RunMotherBrainFakeDeathForMotherBrainDescentPhase[state.DescentPhase](state, samus);
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
    /// Runs one update of Mother Brain's room flash, while it runs: it plays on through its images, looping
    /// (<see cref="MotherBrainRoomFlashAnimations"/>), and her room shows the image it stands on.
    /// </summary>
    internal static void RunMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        if (state.RoomFlash is not { } flash)
            return;

        flash.Step();
        state.Colours.ShowRoomFlash(flash.Showing);
    }

    /// <summary>What <see cref="RunMotherBrainAscent"/> does for each <see cref="MotherBrainAscentPhase"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainAscentPhase, Action<MotherBrainEnemyState>> RunMotherBrainAscentForMotherBrainAscentPhase => field ??= new(new Dictionary<MotherBrainAscentPhase, Action<MotherBrainEnemyState>>
    {
        [MotherBrainAscentPhase.DrawMural] = state => FakeDeath.DrawNextMuralRows(state),
        [MotherBrainAscentPhase.SetupPhase2Graphics] = MotherBrainAscent.SetupMotherBrainPhaseTwoGraphics,
        [MotherBrainAscentPhase.SetupPhase2Brain] = state => MotherBrainAscent.SetupMotherBrainPhaseTwoBrain(state),
        [MotherBrainAscentPhase.PauseForSuspense] = state => MotherBrainAscent.PauseBeforeMotherBrainRises(state),
        [MotherBrainAscentPhase.PrepareForRising] = state => MotherBrainAscent.PrepareMotherBrainForRising(state),
        [MotherBrainAscentPhase.WaitBeforePause] = state => MotherBrainAscent.WaitBeforeMotherBrainAscentPause(state),
        [MotherBrainAscentPhase.ContinuePausing] = state => MotherBrainAscent.ContinueMotherBrainAscentPause(state),
        [MotherBrainAscentPhase.StartMusicAndEarthquake] = state => Ascent.StartMotherBrainAscent(state),
        [MotherBrainAscentPhase.RaiseMotherBrain] = state => Ascent.RaiseMotherBrain(state),
        [MotherBrainAscentPhase.WaitUntilUncrouched] = state => MotherBrainAscent.WaitForMotherBrainToFinishUncrouching(state),
        [MotherBrainAscentPhase.TransitionFromGray] = state => MotherBrainFakeDeath.TransitionMotherBrainFromGray(state),
    });

    /// <summary>Runs the phase of her rise out of the fake death her body is in, from the mural to her colours' return.</summary>
    private void RunMotherBrainAscent(MotherBrainEnemyState state)
    {
        RunMotherBrainAscentForMotherBrainAscentPhase[state.AscentPhase](state);
    }

    /// <summary>What <see cref="RunMotherBrainPhase2Fight"/> does for each <see cref="MotherBrainPhase2FightPhase"/> member, each member's own arm.</summary>
    private IdentityTable<MotherBrainPhase2FightPhase, Action<MotherBrainEnemyState, SamusState>> RunMotherBrainPhase2FightForMotherBrainPhase2FightPhase => field ??= new(new Dictionary<MotherBrainPhase2FightPhase, Action<MotherBrainEnemyState, SamusState>>
    {
        [MotherBrainPhase2FightPhase.StretchingShakeHead] = (state, samus) => MotherBrainAscent.ShakeMotherBrainHeadMenacingly(state),
        [MotherBrainPhase2FightPhase.StretchingBringHeadUp] = (state, samus) => MotherBrainAscent.BringMotherBrainHeadBackUp(state),
        [MotherBrainPhase2FightPhase.StretchingFinish] = (state, samus) => MotherBrainAscent.FinishMotherBrainStretching(state),
        [MotherBrainPhase2FightPhase.Thinking] = (state, samus) => Attacks.RunMotherBrainSecondPhaseThinking(state),
        [MotherBrainPhase2FightPhase.TryAttack] = (state, samus) => Attacks.RunMotherBrainSecondPhaseTryAttack(state, samus),
        [MotherBrainPhase2FightPhase.BombDecideWalking] = (state, samus) => Attacks.DecideMotherBrainBombWalking(state),
        [MotherBrainPhase2FightPhase.BombWalkingBackwards] = (state, samus) => Attacks.ContinueMotherBrainBombWalk(state),
        [MotherBrainPhase2FightPhase.BombCrouch] = (state, samus) => MotherBrainAttacks.CrouchMotherBrainForBomb(state),
        [MotherBrainPhase2FightPhase.BombFired] = (state, samus) => MotherBrainAttacks.WaitAfterMotherBrainBomb(state),
        [MotherBrainPhase2FightPhase.BombStandUp] = (state, samus) => MotherBrainAttacks.StandMotherBrainAfterBomb(state),
        [MotherBrainPhase2FightPhase.LaserPositionHeadQuickly] = (state, samus) =>
        {
            MotherBrainAttacks.PositionMotherBrainHeadQuicklyForLaser(
                state,
                samus);
        },
        [MotherBrainPhase2FightPhase.LaserPositionHeadSlowlyAndFire] = (state, samus) => MotherBrainAttacks.PositionMotherBrainHeadSlowlyAndFireLaser(state),
        [MotherBrainPhase2FightPhase.LaserFinishAttack] = (state, samus) => Attacks.FinishMotherBrainLaserAttack(state),
        [MotherBrainPhase2FightPhase.HandBeam] = (state, samus) => MotherBrainAttacks.RunMotherBrainHandBeamAttack(state),
    });

    /// <summary>
    /// Runs the phase of her second phase's fight her body is in, from her stretch to her attacks, until her health first
    /// runs out and her thinking starts the rainbow beam.
    /// </summary>
    private void RunMotherBrainPhase2Fight(MotherBrainEnemyState state, SamusState samus)
    {
        RunMotherBrainPhase2FightForMotherBrainPhase2FightPhase[state.Phase2FightPhase](state, samus);
    }
}
