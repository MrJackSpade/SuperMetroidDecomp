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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.DraygonArms, EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail];

    /// <summary>Draygon's body, eye, tail or arms (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Draygon definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.DraygonBody)
            return new DraygonBodyEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.DraygonEye)
            return new DraygonEyeEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.DraygonTail)
            return new DraygonTailEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.DraygonArms)
            return new DraygonArmsEntity(this, place, common, _bossArtwork);
        throw new InvalidOperationException($"The Draygon species creates no {place.DefinitionId}.");
    }

    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies frame timing for the body and linked-part AI behaviours.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    internal DraygonParts Parts { get; }

    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    internal DraygonTurrets Turrets { get; }

    /// <summary>Draygon's entrance: the opening delay and its dance.</summary>
    internal DraygonIntro Intro { get; }

    /// <summary>Draygon's swoops across the room and its climb straight up.</summary>
    internal DraygonSwoops Swoops { get; }

    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    internal DraygonMovement Movement { get; }

    /// <summary>Draygon's goop passes.</summary>
    internal DraygonGoop Goop { get; }

    /// <summary>The goop and turret shots as projectiles.</summary>
    internal DraygonShots Shots { get; }

    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    internal DraygonGrab Grab { get; }

    /// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
    internal DraygonDeath Death { get; }

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
        Grab = new DraygonGrab(dependencies.FrameResults, _room, dependencies.EffectActors, dependencies.ScrollCap, Turrets);
        Death = new DraygonDeath(_room, dependencies.EffectActors, dependencies.DeathsAndDrops, Parts);
        Combat = new DraygonCombat(dependencies.Projectiles, dependencies.Collision, Grab);
    }

    /// <summary>Runs Draygon's body for one update.</summary>
    private void RunDraygonBodyMain(RoomEnemyPlace body, SamusState samus, int frameCounter)
    {
        DraygonEnemyState state = body.RequireSpeciesState<DraygonEnemyState>();
        switch (state.Phase)
        {
            case DraygonPhase.IntroInitialDelay:
                Intro.RunDraygonIntroInitialDelay(state, samus, frameCounter);
                break;
            case DraygonPhase.IntroDance:
                Intro.RunDraygonIntroDance(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopSetup when state.PassDirection == HorizontalDirection.Right:
                Swoops.SetupDraygonRightSwoop(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopSetup:
                Swoops.SetupDraygonLeftSwoop(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopDescending:
                Swoops.DescendDraygonSwoop(state, samus, frameCounter, SwoopSideOf(state));
                break;
            case DraygonPhase.SwoopApex when state.PassDirection == HorizontalDirection.Right:
                Swoops.CrossDraygonRightSwoopApex(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopApex:
                Swoops.CrossDraygonLeftSwoopApex(state, samus, frameCounter);
                break;
            case DraygonPhase.SwoopAscending:
                Swoops.AscendDraygonSwoop(state, samus, frameCounter, SwoopSideOf(state));
                break;
            case DraygonPhase.GoopSetup:
                Goop.SetupDraygonGoopPass(state, state.PassDirection, samus, frameCounter);
                break;
            case DraygonPhase.GoopApproach:
                Goop.ApproachSamusForDraygonGoop(state, state.PassDirection, samus, frameCounter);
                break;
            case DraygonPhase.GoopTail:
                Goop.FireDraygonGoop(state, state.PassDirection, samus, frameCounter);
                break;
            case DraygonPhase.GoopRecovery:
                Goop.ExitDraygonGoopPass(state, state.PassDirection, samus);
                break;
            case DraygonPhase.TryGrabSamus:
                Grab.ChaseAndTryToGrabSamus(state, samus, frameCounter);
                break;
            case DraygonPhase.RepelGrapple:
                DraygonGrab.RepelDraygonWithGrapple(state, samus);
                break;
            case DraygonPhase.CarrySamus:
                Grab.CarrySamusToDraygonSpiral(state, samus);
                break;
            case DraygonPhase.FlailWithSamus:
                Grab.CarrySamusInDraygonSpiral(state, samus);
                break;
            case DraygonPhase.TailWhipWithSamus:
                Grab.RunDraygonTailWhip(state, samus);
                break;
            case DraygonPhase.FinalTailWhips:
                Grab.BeginDraygonFinalTailWhips(state, samus);
                break;
            case DraygonPhase.FinalTailWhipsWait:
                Grab.WaitForDraygonFinalTailWhips(state, samus);
                break;
            case DraygonPhase.ReleaseSamus:
                Grab.ReleaseSamusFromDraygon(state, samus);
                break;
            case DraygonPhase.FlyStraightUp:
                Swoops.FlyDraygonStraightUp(state, samus, frameCounter);
                break;
            case DraygonPhase.Dying:
                Death.DriftDyingDraygonToBurialPoint(state);
                break;
            case DraygonPhase.DyingWaitForEvirs:
                Death.WaitForDraygonBurialEvirs(state, frameCounter);
                break;
            case DraygonPhase.DyingFinish:
                Death.SinkDraygonBelowTheRoom(state, frameCounter, samus);
                break;
            default:
                throw new InvalidDataException(
                    $"Draygon body has no behaviour for phase {state.Phase}.");
        }

        // These are four separate enemies, but Draygon's main behaviour makes the body coordinate
        // authoritative after its phase runs. Copying here preserves that ordering for
        // the eye AI and each part's later animation step in the same enemy frame.
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

    /// <summary>Advances the typed animation of <paramref name="place"/>, one of the parts of <paramref name="draygon"/>.</summary>
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
