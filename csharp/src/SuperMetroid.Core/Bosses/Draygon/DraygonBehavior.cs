using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The Draygon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DraygonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.DraygonArms] = (occupant, place, common) => EnemyOccupant.Of(new DraygonArmsEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.DraygonBody] = (occupant, place, common) => EnemyOccupant.Of(new DraygonBodyEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.DraygonEye] = (occupant, place, common) => EnemyOccupant.Of(new DraygonEyeEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.DraygonTail] = (occupant, place, common) => EnemyOccupant.Of(new DraygonTailEntity(this, occupant, place, common, _bossArtwork)),
    };

    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies frame timing for the body and linked-part AI behaviours.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    internal DraygonParts Parts { get; }

    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private DraygonTurrets Turrets { get; }

    /// <summary>Draygon's entrance: the opening delay and its dance.</summary>
    private DraygonIntro Intro { get; }

    /// <summary>Draygon's swoops across the room and its climb straight up.</summary>
    internal DraygonSwoops Swoops { get; }

    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    internal DraygonMovement Movement { get; }

    /// <summary>Draygon's goop passes.</summary>
    private DraygonGoop Goop { get; }

    /// <summary>The goop and turret shots as projectiles.</summary>
    private DraygonShots Shots { get; }

    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    private DraygonGrab Grab { get; }

    /// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
    private DraygonDeath Death { get; }

    /// <summary>Draygon's hitboxes, touch and hurt, and what Samus's weapons do to it.</summary>
    internal DraygonCombat Combat { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>Binds the encounter's room, projectile, presentation, and interaction owners.</summary>
    internal DraygonBehavior(DraygonBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _frameContext = dependencies.FrameContext;
        Shots = new DraygonShots(dependencies.DeathsAndDrops);
        Parts = new DraygonParts(new DraygonPartsDependencies
        {
            Room = dependencies.Room, Projectiles = dependencies.Projectiles,
            Earthquake = dependencies.Earthquake, EffectActors = dependencies.EffectActors,
            Presentation = dependencies.Presentation, FrameContext = dependencies.FrameContext, Audio = dependencies.Audio,
            Shots = Shots,
            Places = dependencies.Places,
        });
        Turrets = new DraygonTurrets(_room, dependencies.Projectiles, Shots);
        Intro = new DraygonIntro(dependencies.EffectActors, Turrets);
        Movement = new DraygonMovement(dependencies.EffectActors);
        Swoops = new DraygonSwoops(_room, Turrets, Movement);
        Goop = new DraygonGoop(_room, Turrets, Movement);
        Grab = new DraygonGrab(dependencies.PassEvents, _room, dependencies.EffectActors, dependencies.ScrollCap, Turrets);
        Death = new DraygonDeath(_room, dependencies.EffectActors, dependencies.DeathsAndDrops, Parts);
        Combat = new DraygonCombat(dependencies.Projectiles, dependencies.Collision, Grab);
    }

    /// <summary>What <see cref="RunDraygonBodyMain"/> does for each <see cref="DraygonPhase"/> member, each member's own arm.</summary>
    private IdentityTable<DraygonPhase, Action<SamusState, int, DraygonEnemyState>> RunDraygonBodyMainForDraygonPhase => field ??= new(new Dictionary<DraygonPhase, Action<SamusState, int, DraygonEnemyState>>
    {
        [DraygonPhase.IntroInitialDelay] = (samus, frameCounter, state) => Intro.RunDraygonIntroInitialDelay(state, samus, frameCounter),
        [DraygonPhase.IntroDance] = (samus, frameCounter, state) => Intro.RunDraygonIntroDance(state, samus, frameCounter),
        [DraygonPhase.SwoopSetup] = (samus, frameCounter, state) =>
        {
            if (state.PassDirection == HorizontalDirection.Right)
            {
                Swoops.SetupDraygonRightSwoop(state, samus, frameCounter);
            }
            else
            {
                Swoops.SetupDraygonLeftSwoop(state, samus, frameCounter);
            }
        },
        [DraygonPhase.SwoopDescending] = (samus, frameCounter, state) => Swoops.DescendDraygonSwoop(state, samus, frameCounter, SwoopSideOf(state)),
        [DraygonPhase.SwoopApex] = (samus, frameCounter, state) =>
        {
            if (state.PassDirection == HorizontalDirection.Right)
            {
                Swoops.CrossDraygonRightSwoopApex(state, samus, frameCounter);
            }
            else
            {
                Swoops.CrossDraygonLeftSwoopApex(state, samus, frameCounter);
            }
        },
        [DraygonPhase.SwoopAscending] = (samus, frameCounter, state) => Swoops.AscendDraygonSwoop(state, samus, frameCounter, SwoopSideOf(state)),
        [DraygonPhase.GoopSetup] = (samus, frameCounter, state) => Goop.SetupDraygonGoopPass(state, state.PassDirection, samus, frameCounter),
        [DraygonPhase.GoopApproach] = (samus, frameCounter, state) => Goop.ApproachSamusForDraygonGoop(state, samus, frameCounter),
        [DraygonPhase.GoopTail] = (samus, frameCounter, state) => Goop.FireDraygonGoop(state, state.PassDirection, samus, frameCounter),
        [DraygonPhase.GoopRecovery] = (samus, frameCounter, state) => Goop.ExitDraygonGoopPass(state, state.PassDirection, samus),
        [DraygonPhase.TryGrabSamus] = (samus, frameCounter, state) => Grab.ChaseAndTryToGrabSamus(state, samus, frameCounter),
        [DraygonPhase.RepelGrapple] = (samus, frameCounter, state) => DraygonGrab.RepelDraygonWithGrapple(state, samus),
        [DraygonPhase.CarrySamus] = (samus, frameCounter, state) => Grab.CarrySamusToDraygonSpiral(state, samus),
        [DraygonPhase.FlailWithSamus] = (samus, frameCounter, state) => Grab.CarrySamusInDraygonSpiral(state, samus),
        [DraygonPhase.BeginTailWhipWithSamus] = (samus, frameCounter, state) => Grab.BeginDraygonTailWhip(state, samus),
        [DraygonPhase.TailWhipWithSamus] = (samus, frameCounter, state) => Grab.RunDraygonTailWhip(state, samus),
        [DraygonPhase.FinalTailWhips] = (samus, frameCounter, state) => Grab.BeginDraygonFinalTailWhips(state, samus),
        [DraygonPhase.FinalTailWhipsWait] = (samus, frameCounter, state) => Grab.WaitForDraygonFinalTailWhips(state, samus),
        [DraygonPhase.ReleaseSamus] = (samus, frameCounter, state) => Grab.ReleaseSamusFromDraygon(state, samus),
        [DraygonPhase.FlyStraightUp] = (samus, frameCounter, state) => Swoops.FlyDraygonStraightUp(state, samus, frameCounter),
        [DraygonPhase.Dying] = (samus, frameCounter, state) => Death.DriftDyingDraygonToBurialPoint(state),
        [DraygonPhase.DyingWaitForEvirs] = (samus, frameCounter, state) => Death.WaitForDraygonBurialEvirs(state, frameCounter),
        [DraygonPhase.DyingFinish] = (samus, frameCounter, state) => Death.SinkDraygonBelowTheRoom(state, frameCounter, samus),
    });

    /// <summary>Runs Draygon's body for one update.</summary>
    private void RunDraygonBodyMain(RoomEnemyPlace body, SamusState samus, int frameCounter)
    {
        DraygonEnemyState state = body.RequireSpeciesState<DraygonEnemyState>();
        RunDraygonBodyMainForDraygonPhase[state.Phase](samus, frameCounter, state);

        // His arms, tail and eye are placed at his body once his phase has moved it, before the eye and each part's
        // animation run this update.
        state.Arms.Position = state.Arms.Position.WithWholeX(body.Position.X.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Arms.Position = state.Arms.Position.WithWholeY(body.Position.Y.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }

    /// <summary>One side of Draygon's swoop: the direction it travels and the arm animations that side uses.</summary>
    /// <param name="Direction">The way the swoop travels; rightward adds its velocity to X.</param>
    /// <param name="ArmsNearApex">The arms animation started as the path nears its lowest point.</param>
    /// <param name="ArmsIdle">The arms animation restored as the ascent passes the same point.</param>
    internal sealed record DraygonSwoopSide(HorizontalDirection Direction, DraygonSegment ArmsNearApex, DraygonSegment ArmsIdle);

    /// <summary>The swoop from the left edge to the right.</summary>
    internal static readonly DraygonSwoopSide RightSwoop = new(HorizontalDirection.Right, DraygonSegment.ArmsFacingRightNearSwoopApex,
        DraygonSegment.ArmsFacingRightIdle);

    /// <summary>The mirrored swoop from the right edge to the left.</summary>
    internal static readonly DraygonSwoopSide LeftSwoop = new(HorizontalDirection.Left, DraygonSegment.ArmsFacingLeftNearSwoopApex,
        DraygonSegment.ArmsFacingLeftIdle);

    /// <summary>The side of the swoop Draygon's pass flies (#2165).</summary>
    private static DraygonSwoopSide SwoopSideOf(DraygonEnemyState state) =>
        state.PassDirection == HorizontalDirection.Right ? RightSwoop : LeftSwoop;

    /// <summary>Advances the typed animation of <paramref name="part"/>, one of Draygon's parts, in <paramref name="context"/>.</summary>
    internal void AdvanceDraygonTypedAnimation(DraygonPart part, EnemyAnimationCueContext context)
    {
        Parts.AdvanceDraygonPartAnimation(part, context.Samus);
    }

    /// <summary>Runs DraygonBody's main AI.</summary>
    internal void RunDraygonBodyMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        // Draygon times his turrets and smoke by the frame counter's place in a short repeating cycle.
        RunDraygonBodyMain(place, frame.Samus, _frameContext.FrameCounter);
    }

    /// <summary>
    /// Disables a wall cannon for the room object that names it. Cannon setup names one of
    /// Draygon's wall cannons, then the cannon's damage step disables it here.
    /// </summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon)
    {
        // The defeated room still has cannon room objects even though its enemy population no
        // longer includes Draygon; only a live Draygon's turrets read the disabled cannons.
        Turrets.DisableCannon(cannon);
    }
}
