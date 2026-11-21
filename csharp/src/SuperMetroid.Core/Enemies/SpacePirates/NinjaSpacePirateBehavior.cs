using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The ninja space pirate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class NinjaSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    [field: NonSerialized]
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.PirateGoldNinja] = (occupant, place, common) => EnemyOccupant.Of(new GoldNinjaSpacePirateEntity(this, occupant, place, common)),
        [EnemyDefinitionId.PirateSilverNinja] = (occupant, place, common) => EnemyOccupant.Of(new NinjaSpacePirateEntity(this, occupant, place, common)),
    };

    /// <summary>Provides the shared RNG used by divekick selection.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves floor contact during divekick descent.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates thrown claw projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns paired landing dust sprites.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>The owner of the shared Pirate contact reactions, which every Pirate family takes.</summary>
    internal WalkingSpacePirateBehavior Walking { get; }

    /// <summary>Captures the room, collision, projectile, sprite, and sound owners for ninja AI.</summary>
    internal NinjaSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, RoomEffectActors effectActors, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        Walking = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _effectActors = effectActors;
    }

    /// <summary>Initializes the Ninja Space Pirate.</summary>
    internal static void InitializeNinjaSpacePirate(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new NinjaSpacePirateEnemyState(NinjaSpacePirateAction.StandingStill);
        place.SetSpeciesState(state);

        NinjaSpacePirateSpawnSettings settings = occupant.SpawnSettings<NinjaSpacePirateSpawnSettings>();
        bool startsAtLeft = settings.StartPost == NinjaSpacePirateStartPost.Left;
        NinjaSpacePirateSegment initial = startsAtLeft
            ? NinjaSpacePirateSegment.AwaitingSamusFacingRight
            : NinjaSpacePirateSegment.AwaitingSamusFacingLeft;
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(initial), AnimationPlace.StartOfAnimation);

        // The posts lie symmetrically about the placement's midpoint, at the distance the spin covers, which can differ
        // slightly from the placement's post distance.
        int requestedLeft = startsAtLeft
            ? place.Position.X.Whole
            : place.Position.X.Whole - settings.PostDistance;
        int requestedHalfSpan = settings.PostDistance / 2;
        state.PostsMidpointX = requestedLeft + requestedHalfSpan;

        // The spin's speed grows until the distance it covers reaches the half-span; it runs at least once, so posts no
        // distance apart still spin.
        SignedSixteenSixteen requestedDistance = SignedSixteenSixteen.FromWholePixels(requestedHalfSpan);
        SignedEightEight speed = SignedEightEight.Zero;
        SignedSixteenSixteen covered = SignedSixteenSixteen.Zero;
        do
        {
            speed += SpacePirateMotionDefinitions.NinjaSpinSpeedChange;
            covered += speed.ToSixteenSixteenDelta();
        }
        while (covered < requestedDistance);

        int adjustedHalfSpan = covered.WholePixels;
        state.RightPostX = state.PostsMidpointX + adjustedHalfSpan;
        state.LeftPostX = state.PostsMidpointX - adjustedHalfSpan;
        place.Position = place.Position.WithX(FixedCoordinate.FromWhole(startsAtLeft ? state.LeftPostX : state.RightPostX));
        state.SpawnPosition = place.Position.Whole;
    }

    /// <summary>What <see cref="RunNinjaSpacePirateMain"/> does for each <see cref="NinjaSpacePirateAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<NinjaSpacePirateAction, Action<RoomEnemyPlace, NinjaSpacePirateEnemyState, SamusState, RoomLevelData, SamusProjectileSystem>> RunNinjaSpacePirateMainForNinjaSpacePirateAction => field ??= new(new Dictionary<NinjaSpacePirateAction, Action<RoomEnemyPlace, NinjaSpacePirateEnemyState, SamusState, RoomLevelData, SamusProjectileSystem>>
    {
        [NinjaSpacePirateAction.StandingStill] = (place, state, samus, level, samusProjectiles) => { },
        [NinjaSpacePirateAction.AwaitingSamus] = (place, state, samus, level, samusProjectiles) => RunNinjaPirateAwaitingSamus(place, state, samus, samusProjectiles),
        [NinjaSpacePirateAction.Active] = (place, state, samus, level, samusProjectiles) => RunNinjaPirateActive(place, state, samus, samusProjectiles),
        [NinjaSpacePirateAction.SpinJumpRising] = (place, state, samus, level, samusProjectiles) => StepNinjaPirateSpinJump(place, state, state.Heading, vertical: VerticalDirection.Up),
        [NinjaSpacePirateAction.SpinJumpFalling] = (place, state, samus, level, samusProjectiles) => StepNinjaPirateSpinJump(place, state, state.Heading, vertical: VerticalDirection.Down),
        [NinjaSpacePirateAction.ReadyToDivekick] = (place, state, samus, level, samusProjectiles) =>
        {
            RunNinjaPirateReadyToDivekick(
                place,
                state,
                samus,
                samusProjectiles);
        },
        [NinjaSpacePirateAction.DivekickJump] = (place, state, samus, level, samusProjectiles) => StepNinjaPirateDiveJump(place, state, state.Heading),
        [NinjaSpacePirateAction.DivekickDive] = (place, state, samus, level, samusProjectiles) => StepNinjaPirateDive(place, state, level, state.Heading),
        [NinjaSpacePirateAction.DivekickWalkToPost] = (place, state, samus, level, samusProjectiles) => StepNinjaPirateWalkToPost(place, state, state.Heading),
    });

    /// <summary>Runs the Ninja Space Pirate's current behaviour.</summary>
    private void RunNinjaSpacePirateMain(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level,
        SamusProjectileSystem samusProjectiles)
    {
        RunNinjaSpacePirateMainForNinjaSpacePirateAction[state.Action](place, state, samus, level, samusProjectiles);
    }

    /// <summary>Waits for Samus to approach, then selects the active animation facing her.</summary>
    private static void RunNinjaPirateAwaitingSamus(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) >=
            SpacePirateMotionDefinitions.NinjaActivationDistance)
        {
            _ = TryNinjaPirateProjectileFlinch(place, state, samus, samusProjectiles);
            return;
        }

        NinjaSpacePirateSegment active = place.Position.X.Whole < samus.Position.X.Whole
            ? NinjaSpacePirateSegment.ActiveFacingRight
            : NinjaSpacePirateSegment.ActiveFacingLeft;
        StartNinjaPirateAnimation(place, state, active);
    }

    /// <summary>Checks flinch, standing kick, spin jump, then periodic claw attack in priority order.</summary>
    private static void RunNinjaPirateActive(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(place, state, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(place, state, samus) ||
            TryNinjaPirateSpinJump(place, state, samus))
        {
            return;
        }

        TryNinjaPirateClawAttack(place, state, samus);
    }

    /// <summary>Flinches when the one shot the flinch tests (<see cref="PirateFlinchFromHighestShot"/>) is close on both axes.</summary>
    private static bool TryNinjaPirateProjectileFlinch(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        // A ninja pirate reacts to a beam or missile by its kind, not its picture, so the invisible Murder Beam counts.
        SamusProjectile? projectile = PirateFlinchFromHighestShot.Shot(
            samusProjectiles.Shots, shot => shot.HasEnemyCollisionPayload);
        if (projectile is null ||
            Math.Abs(projectile.Position.X.Whole - place.Position.X.Whole) >=
                SpacePirateMotionDefinitions.NinjaFlinchDistance ||
            Math.Abs(projectile.Position.Y.Whole - place.Position.Y.Whole) >=
                SpacePirateMotionDefinitions.NinjaFlinchDistance)
        {
            return false;
        }

        StartNinjaPirateAnimation(
            place,
            state,
            place.Position.X.Whole < samus.Position.X.Whole
                ? NinjaSpacePirateSegment.FlinchFacingRight
                : NinjaSpacePirateSegment.FlinchFacingLeft);
        return true;
    }

    /// <summary>Starts a facing kick when Samus is within the strict contact range.</summary>
    private static bool TryNinjaPirateStandingKick(RoomEnemyPlace place, NinjaSpacePirateEnemyState state, SamusState samus)
    {
        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >=
                SpacePirateMotionDefinitions.NinjaKickDistance ||
            Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) >=
                SpacePirateMotionDefinitions.NinjaKickDistance)
        {
            return false;
        }

        StartNinjaPirateAnimation(
            place,
            state,
            place.Position.X.Whole < samus.Position.X.Whole
                ? NinjaSpacePirateSegment.KickFacingRight
                : NinjaSpacePirateSegment.KickFacingLeft);
        return true;
    }

    /// <summary>Starts a jump toward the opposite post when Samus nears the patrol midpoint.</summary>
    private static bool TryNinjaPirateSpinJump(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(state.PostsMidpointX - samus.Position.X.Whole) >=
            SpacePirateMotionDefinitions.NinjaMidpointTriggerDistance)
        {
            return false;
        }

        StartNinjaPirateAnimation(
            place,
            state,
            place.Position.X.Whole == state.LeftPostX
                ? NinjaSpacePirateSegment.SpinJumpRight
                : NinjaSpacePirateSegment.SpinJumpLeft);
        return true;
    }

    /// <summary>Starts a claw throw on the periodic frame when Samus is on the allowed side.</summary>
    private static void TryNinjaPirateClawAttack(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus)
    {
        if (!NinjaSpacePirateCadenceDefinitions.ClawAttack.IsDue(place.FrameCounter))
            return;

        if (place.Position.X.Whole == state.LeftPostX)
        {
            if (place.Position.X.Whole < samus.Position.X.Whole)
                return;
            StartNinjaPirateAnimation(
                place,
                state,
                NinjaSpacePirateSegment.ClawAttackLeft);
            return;
        }

        if (place.Position.X.Whole >= samus.Position.X.Whole)
            return;
        StartNinjaPirateAnimation(
            place,
            state,
            NinjaSpacePirateSegment.ClawAttackRight);
    }

    /// <summary>
    /// When no reaction comes first, draws random numbers until SpacePirateDecisions.DivekickDrawnAgain no longer takes
    /// one, then divekicks.
    /// </summary>
    private void RunNinjaPirateReadyToDivekick(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        if (TryNinjaPirateProjectileFlinch(place, state, samus, samusProjectiles) ||
            TryNinjaPirateStandingKick(place, state, samus))
        {
            return;
        }

        if (Math.Abs(state.PostsMidpointX - samus.Position.X.Whole) >=
            SpacePirateMotionDefinitions.NinjaMidpointTriggerDistance)
        {
            return;
        }

        // Every kept draw starts the same divekick; three draws in four are kept.
        bool drawsAgain;
        do
            drawsAgain = SpacePirateDecisions.DivekickDrawnAgain.IsTakenBy(_room.Random.Generate());
        while (drawsAgain);

        StartNinjaPirateAnimation(
            place,
            state,
            place.Position.X.Whole == state.LeftPostX
                ? NinjaSpacePirateSegment.DivekickRightJump
                : NinjaSpacePirateSegment.DivekickLeftJump);
    }

    /// <summary>Moves through a rising and falling arc between patrol posts, then emits landing dust.</summary>
    private void StepNinjaPirateSpinJump(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction,
        VerticalDirection vertical)
    {
        int xPixels = state.SpinSpeed.WholePixels;
        place.Position = place.Position.WithWholeX(place.Position.X.Whole + (xPixels.Toward(direction)));
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole +
            (SpacePirateMotionDefinitions.NinjaSpinClimbStep.Toward(vertical)));

        if (vertical == VerticalDirection.Up)
        {
            state.SpinSpeed += SpacePirateMotionDefinitions.NinjaSpinSpeedChange;
            bool crossedMidpoint = direction == HorizontalDirection.Right
                ? place.Position.X.Whole >= state.PostsMidpointX
                : place.Position.X.Whole < state.PostsMidpointX;
            if (crossedMidpoint)
            {
                state.Action = NinjaSpacePirateAction.SpinJumpFalling;
            }
            return;
        }

        state.SpinSpeed -= SpacePirateMotionDefinitions.NinjaSpinSpeedChange;
        if (!state.SpinSpeed.IsZero)
            return;

        state.Action = NinjaSpacePirateAction.StandingStill;
        place.Position = place.Position.WithWholeX(direction == HorizontalDirection.Right ? state.RightPostX : state.LeftPostX);
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.LandFacingRight
                : NinjaSpacePirateSegment.LandFacingLeft);
        SpawnNinjaPirateLandingDust(place);
    }

    /// <summary>
    /// Sets the divekick's initial rising speed.
    /// </summary>
    private static void InitializeNinjaPirateDive(NinjaSpacePirateEnemyState state) =>
        state.DiveSpeed = SpacePirateMotionDefinitions.NinjaDiveStartSpeed;

    /// <summary>Rises until its speed carries it downward, then switches to the descent animation.</summary>
    private static void StepNinjaPirateDiveJump(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - state.DiveSpeed.WholePixels);
        state.DiveSpeed -= SpacePirateMotionDefinitions.NinjaDiveSpeedChange;
        if (!state.DiveSpeed.IsNegative)
            return;

        state.Action = NinjaSpacePirateAction.DivekickDive;
        state.Heading = direction;
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.DivekickRightDive
                : NinjaSpacePirateSegment.DivekickLeftDive);
        state.DiveSpeed = SpacePirateMotionDefinitions.NinjaDiveStartSpeed;
    }

    /// <summary>
    /// Moves horizontally and falls by the divekick's step (<see cref="NinjaPirateDiveFallStep"/>) until the floor or the
    /// speed ends the dive.
    /// </summary>
    internal void StepNinjaPirateDive(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction)
    {
        place.Position = place.Position.WithWholeX(place.Position.X.Whole +
            (SpacePirateMotionDefinitions.NinjaDiveStep.Toward(direction)));

        bool landed = _terrain.MoveEnemyVertically(level, place, NinjaPirateDiveFallStep.Of(state.DiveSpeed));
        if (!landed)
        {
            state.DiveSpeed -= SpacePirateMotionDefinitions.NinjaDiveSpeedChange;
            landed = state.DiveSpeed.IsNegative || state.DiveSpeed.WholePixels == 0;
        }
        if (!landed)
            return;

        state.Action = NinjaSpacePirateAction.DivekickWalkToPost;
        state.Heading = direction;
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.WalkToRightPost
                : NinjaSpacePirateSegment.WalkToLeftPost);
        // Landing at either post sets it on its spawn row.
        place.Position = place.Position.WithWholeY(state.SpawnPosition.Y);
        SpawnNinjaPirateLandingDust(place);
    }

    /// <summary>Walks back to the selected post and restores its landing animation.</summary>
    private static void StepNinjaPirateWalkToPost(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        place.Position = place.Position.WithWholeX(place.Position.X.Whole +
            (SpacePirateMotionDefinitions.NinjaReturnStep.Toward(direction)));
        bool reachedPost = direction == HorizontalDirection.Right
            ? place.Position.X.Whole >= state.RightPostX
            : place.Position.X.Whole < state.LeftPostX;
        if (!reachedPost)
            return;

        place.Position = place.Position.WithWholeX(direction == HorizontalDirection.Right ? state.RightPostX : state.LeftPostX);
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.LandFacingRight
                : NinjaSpacePirateSegment.LandFacingLeft);
        state.Action = NinjaSpacePirateAction.StandingStill;
    }

    /// <summary>Spawns a dust cloud on each side of the landing position.</summary>
    private void SpawnNinjaPirateLandingDust(RoomEnemyPlace place)
    {
        SpriteGraphics graphics = SpriteGraphics.SharedStandard;
        _effectActors.SpawnRoomEffectActor(
            new(place.Position.X.Whole - SpacePirateMotionDefinitions.NinjaLandingDustSpread,
                place.Position.Y.Whole + SpacePirateMotionDefinitions.NinjaLandingDustDrop),
            RoomEffectActorKind.RisingDustPuff,
            graphics);
        _effectActors.SpawnRoomEffectActor(
            new(place.Position.X.Whole + SpacePirateMotionDefinitions.NinjaLandingDustSpread,
                place.Position.Y.Whole + SpacePirateMotionDefinitions.NinjaLandingDustDrop),
            RoomEffectActorKind.RisingDustPuff,
            graphics);
    }

    /// <summary>Allocates a claw with the selected throw direction, offset, graphics, and outbound speed.</summary>
    internal void SpawnNinjaPirateClaw(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        HorizontalDirection thrown,
        Offset16<RoomPixels> spawnOffset)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PirateClaw, Claw, SpriteGraphics.SpawnedBy(occupant, source.Shading), new SpacePirateProjectileAnimation(Claw, thrown == HorizontalDirection.Right
            ? SpacePirateProjectileSegment.ClawRight
            : SpacePirateProjectileSegment.ClawLeft));
        if (projectile is null)
            return;

        projectile.Position = FixedPosition<RoomPixels>.FromWhole(source.Position.Whole + spawnOffset);
        Claw.Claws.Start(projectile, new NinjaPirateClawState(SpacePirateMotionDefinitions.NinjaClawLaunchSpeed, thrown));
    }

    /// <summary>Flies a claw out along its throw and back, with exact camera culling.</summary>
    private static void FlyNinjaPirateClaw(
        ClawBehavior owner,
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        NinjaPirateClawState claw = owner.Claws.Of(projectile);
        HorizontalDirection thrown = claw.Thrown;
        int outbound = claw.StepAlongThrow();

        // Outbound motion follows the throw direction while decelerating to zero; inbound
        // motion reverses direction and accelerates indefinitely until the claw leaves the view, whose right and
        // bottom edges are already outside. Y advances one pixel on both halves of the flight. A claw flying left with the
        // view at the room's left edge leaves it past the room's origin, continuing from the space's far end as
        // OffRoomPlaces places it, before the view test deletes it; it falls a pixel an update, so it stays below the
        // room's top.
        projectile.Position = OffRoomPlaces.WithWholeX(projectile.Position, projectile.Position.X.Whole +
            (outbound.Toward(thrown)));
        projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole + 1);

        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }

    /// <summary>
    /// Carries out one cue of the ninja Space Pirates' animations. Becoming active continues at the active stance facing
    /// Samus, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private NinjaSpacePirateSegment? ApplyNinjaSpacePirateCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, NinjaSpacePirateCue cue, SamusState samus)
    {
        NinjaSpacePirateEnemyState state = place.RequireSpeciesState<NinjaSpacePirateEnemyState>();
        return cue.Match(ApplyNinjaSpacePirateCueCases, new ApplyNinjaSpacePirateCueContext(occupant, place, state, samus, this));
    }

    /// <summary>What <see cref="ApplyNinjaSpacePirateCue"/> does on each <see cref="NinjaSpacePirateCue"/>, built once (#2553).</summary>
    private static NinjaSpacePirateCueCases<ApplyNinjaSpacePirateCueContext, NinjaSpacePirateSegment?> ApplyNinjaSpacePirateCueCases { get; } = new(
        PlaySound: static (playSound, context) =>
        {
            context.Owner.Walking.RequestSpacePirateSound(playSound.Sound);
            return null;
        },
        SetShading: static (setShading, context) =>
        {
            context.Place.Shading = setShading.Shading;
            return null;
        },
        ThrowClaw: static (throwClaw, context) =>
        {
            context.Owner.SpawnNinjaPirateClaw(context.Occupant, context.Place, throwClaw.Throw, throwClaw.Offset);
            return null;
        },
        Begin: static (begin, context) =>
        {
            // The animation does not jump to the move it begins: main AI runs it from the next update, as the
            // animation runs on.
            context.State.Action = begin.Action;
            return null;
        },
        BeginHeading: static (begin, context) =>
        {
            // As a plain begin, main AI runs the move from the next update, the cue's way (#2165).
            context.State.Action = begin.Action;
            context.State.Heading = begin.Heading;
            return null;
        },
        Signal: static (signal, context) => context.Owner.NinjaSignals[signal.Kind](context.Place, context.State, context.Samus));

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The pirate's state <see cref="ApplyNinjaSpacePirateCue"/> works with.</param>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Owner">The <see cref="NinjaSpacePirateBehavior"/> reacting.</param>
    private readonly partial record struct ApplyNinjaSpacePirateCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, NinjaSpacePirateEnemyState State, SamusState Samus, NinjaSpacePirateBehavior Owner);

    /// <summary>What each of the ninja Space Pirates' signals does, and the animation it continues at, or null to run on.</summary>
    [field: NonSerialized]
    private IdentityTable<NinjaSpacePirateCueKind, Func<RoomEnemyPlace, NinjaSpacePirateEnemyState, SamusState, NinjaSpacePirateSegment?>> NinjaSignals => field ??=
        new(new Dictionary<NinjaSpacePirateCueKind, Func<RoomEnemyPlace, NinjaSpacePirateEnemyState, SamusState, NinjaSpacePirateSegment?>>
        {
            [NinjaSpacePirateCueKind.StartActive] = static (place, _, samus) =>
            {
                NinjaSpacePirateSegment active = place.Position.X.Whole < samus.Position.X.Whole
                    ? NinjaSpacePirateSegment.ActiveFacingRight
                    : NinjaSpacePirateSegment.ActiveFacingLeft;
                place.AnimationClock.StepOnNextUpdate();
                // The cue returns the selected animation, not its next step.
                // Re-entering that animation runs its action cue and restores decision AI
                // after the idle animation set the action that waits for it.
                return active;
            },
            [NinjaSpacePirateCueKind.ResetSpinSpeed] = static (_, state, _) =>
            {
                state.SpinSpeed = SignedEightEight.Zero;
                return null;
            },
            [NinjaSpacePirateCueKind.StartDivekick] = (_, state, _) =>
            {
                InitializeNinjaPirateDive(state);
                return null;
            },
        });

    /// <summary>Starts an animation and restarts its frame timer.</summary>
    private static void StartNinjaPirateAnimation(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        NinjaSpacePirateSegment segment)
    {
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// A Ninja Pirate's claw (#1760): its spawner starts its animation and chooses its throw's way, and the animation throws
    /// it.
    /// </summary>
    internal ClawBehavior Claw { get; } = new();

    /// <summary>A thrown claw: it flies out and back along its throw, once its animation throws it (#2278).</summary>
    internal sealed partial class ClawBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <summary>Each claw's throw and its speed along it, given as its pirate throws it.</summary>
        internal EnemyProjectileStates<NinjaPirateClawState> Claws { get; } = new();

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            FlyNinjaPirateClaw(this, context.Projectile, context.Camera);

        /// <inheritdoc/>
        /// <remarks>
        /// The cue starts the claw's flight the way its thrower chose, the one place the way is held (#2553); a firing cue also
        /// runs the flight's first update at once, as <see cref="SpacePirateProjectileCueKind.FireLaser"/> says.
        /// </remarks>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            context.Projectile.StartMotion(this, SingleMotion.Moving);
            if (cue.Kind == SpacePirateProjectileCueKind.FireLaser)
                FlyNinjaPirateClaw(this, context.Projectile, context.Camera);
        }
    }

    /// <summary>Runs the ninja Space Pirate's main AI.</summary>
    internal void RunNinjaSpacePirateMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunNinjaSpacePirateMain(
            place,
            place.RequireSpeciesState<NinjaSpacePirateEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the ninja Space Pirate's typed animation.</summary>
    internal void AdvanceNinjaSpacePirateTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, NinjaSpacePirateAnimations.Segments,
            static (NinjaSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyNinjaSpacePirateCue(occupant, place, cue, context.Samus) is { } continuesAt
                ? new AnimationContinuation<NinjaSpacePirateSegment>(NinjaSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

}
