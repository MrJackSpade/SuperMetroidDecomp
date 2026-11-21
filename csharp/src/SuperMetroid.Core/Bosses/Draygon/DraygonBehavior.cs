using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The Draygon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DraygonBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.DraygonBody];

    /// <summary>
    /// Draygon's body, built the same way wherever it is set up; his eye, tail and arms join the encounter his body offers
    /// them as it sets up (<see cref="BeginDraygon"/>).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EnemySpawnSettings>(Definitions, Create);
        registrar.Joins([EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail, EnemyDefinitionId.DraygonArms]);
    }

    /// <summary>Draygon's body (#2146).</summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new DraygonBodyEntity(this, occupant, place, common, _bossArtwork));

    /// <summary>Where his body offers his other parts the creation that builds each of them joining his encounter.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>
    /// Sets his body up, beginning the encounter, and offers his eye, tail and arms, which his room lists right after his
    /// body (#2298), the creation that builds each of them joining that encounter (#2554).
    /// </summary>
    /// <returns>The encounter, which the body holds from its setup.</returns>
    internal DraygonEnemyState BeginDraygon(RoomEnemyOccupant body)
    {
        DraygonEnemyState state = Parts.InitializeDraygonBody(body);
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.DraygonEye, (occupant, _, place, common) =>
            EnemyOccupant.Running(new DraygonEyeEntity(this, occupant, place, common, _bossArtwork, state)));
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.DraygonTail, (occupant, _, place, common) =>
            EnemyOccupant.Running(new DraygonTailEntity(this, occupant, place, common, _bossArtwork, state)));
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.DraygonArms, (occupant, _, place, common) =>
            EnemyOccupant.Running(new DraygonArmsEntity(this, occupant, place, common, _bossArtwork, state)));
        return state;
    }

    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies frame timing for the body and linked-part AI behaviours.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    internal DraygonParts Parts { get; }

    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private DraygonTurrets Turrets { get; }

    /// <summary>Draygon's entrance: the opening delay and its dance.</summary>
    internal DraygonIntro Intro { get; }

    /// <summary>Draygon's swoops across the room and its climb straight up.</summary>
    internal DraygonSwoops Swoops { get; }

    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    internal DraygonMovement Movement { get; }

    /// <summary>Draygon's goop passes.</summary>
    internal DraygonGoop Goop { get; }

    /// <summary>The goop and turret shots as projectiles.</summary>
    private DraygonShots Shots { get; }

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
        _partOffers = dependencies.PartOffers;
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

    /// <summary>Runs Draygon's body for one update.</summary>
    private void RunDraygonBodyMain(RoomEnemyPlace body, DraygonEnemyState state, SamusState samus, int frameCounter)
    {
        state.Step.Run(this, state, samus, frameCounter);

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
    internal static DraygonSwoopSide SwoopSideOf(DraygonEnemyState state) =>
        state.PassDirection == HorizontalDirection.Right ? RightSwoop : LeftSwoop;

    /// <summary>Advances the typed animation of <paramref name="part"/>, one of Draygon's parts, in <paramref name="context"/>.</summary>
    internal void AdvanceDraygonTypedAnimation(DraygonPart part, EnemyAnimationCueContext context)
    {
        Parts.AdvanceDraygonPartAnimation(part, context.Samus);
    }

    /// <summary>Runs DraygonBody's main AI.</summary>
    internal void RunDraygonBodyMainAi(RoomEnemyPlace place, DraygonEnemyState draygonEnemyState, IEnemyAiFrame frame)
    {
        // Draygon times his turrets and smoke by the frame counter's place in a short repeating cycle.
        RunDraygonBodyMain(place, draygonEnemyState, frame.Samus, _frameContext.FrameCounter);
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
