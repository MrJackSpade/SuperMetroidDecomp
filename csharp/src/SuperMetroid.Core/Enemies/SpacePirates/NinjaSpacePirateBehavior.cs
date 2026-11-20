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
internal sealed class NinjaSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldNinja, EnemyDefinitionId.PirateSilverNinja];

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The place holds no ninja definition.</exception>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.PirateGoldNinja)
            return EnemyOccupant.Of(new GoldNinjaSpacePirateEntity(this, place, common));
        if (definition == EnemyDefinitionId.PirateSilverNinja)
            return EnemyOccupant.Of(new NinjaSpacePirateEntity(this, place, common));
        throw new InvalidOperationException($"The ninja Space Pirate species creates no {place.DefinitionId}.");
    }

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
    internal static void InitializeNinjaSpacePirate(RoomEnemyPlace place)
    {
        var state = new NinjaSpacePirateEnemyState(NinjaSpacePirateAction.StandingStill);
        place.SetSpeciesState(state);

        NinjaSpacePirateSpawnSettings settings = place.SpawnSettings<NinjaSpacePirateSpawnSettings>();
        bool startsAtLeft = settings.StartPost switch
        {
            NinjaSpacePirateStartPost.Left => true,
            NinjaSpacePirateStartPost.Right => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(NinjaSpacePirateStartPost)} {settings.StartPost}."),
        };
        NinjaSpacePirateSegment initial = startsAtLeft
            ? NinjaSpacePirateSegment.AwaitingSamusFacingRight
            : NinjaSpacePirateSegment.AwaitingSamusFacingLeft;
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(initial), AnimationPlace.StartOfAnimation);

        // The post distance describes the requested distance between posts. The setup
        // first establishes those requested posts around the population X coordinate, then
        // deliberately replaces them with a symmetric pair derived from its acceleration
        // sum. That adjustment is why the final span can differ slightly from the post distance.
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

    /// <summary>Runs the Ninja Space Pirate's current behaviour.</summary>
    private void RunNinjaSpacePirateMain(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level,
        SamusProjectileSystem samusProjectiles)
    {
        switch (state.Action)
        {
            case NinjaSpacePirateAction.StandingStill:
                return;
            case NinjaSpacePirateAction.AwaitingSamus:
                RunNinjaPirateAwaitingSamus(place, state, samus, samusProjectiles);
                return;
            case NinjaSpacePirateAction.Active:
                RunNinjaPirateActive(place, state, samus, samusProjectiles);
                return;
            case NinjaSpacePirateAction.SpinJumpRising:
                StepNinjaPirateSpinJump(place, state, state.Heading, vertical: VerticalDirection.Up);
                return;
            case NinjaSpacePirateAction.SpinJumpFalling:
                StepNinjaPirateSpinJump(place, state, state.Heading, vertical: VerticalDirection.Down);
                return;
            case NinjaSpacePirateAction.ReadyToDivekick:
                RunNinjaPirateReadyToDivekick(
                    place,
                    state,
                    samus,
                    samusProjectiles);
                return;
            case NinjaSpacePirateAction.DivekickJump:
                StepNinjaPirateDiveJump(place, state, state.Heading);
                return;
            case NinjaSpacePirateAction.DivekickDive:
                StepNinjaPirateDive(place, state, level, state.Heading);
                return;
            case NinjaSpacePirateAction.DivekickWalkToPost:
                StepNinjaPirateWalkToPost(place, state, state.Heading);
                return;
        }
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

    /// <summary>Retries RNG until a nonzero dive choice after higher-priority reactions fail.</summary>
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

        // Every accepted draw starts the same divekick. The draws are kept because advancing the RNG until it
        // draws a number not divisible by four is observable.
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

    /// <summary>Rises until signed speed turns negative, then switches to the descent animation.</summary>
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
    private void StepNinjaPirateDive(
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
    private void SpawnNinjaPirateClaw(
        RoomEnemyPlace source,
        HorizontalDirection thrown,
        Offset16<RoomPixels> spawnOffset)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateClaw,
            Claw,
            SpriteGraphics.SpawnedBy(source));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(source.Position.Whole + spawnOffset);
        projectile.StartAnimation(new SpacePirateProjectileAnimation(Claw, thrown == HorizontalDirection.Right
            ? SpacePirateProjectileSegment.ClawRight
            : SpacePirateProjectileSegment.ClawLeft));
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
    private NinjaSpacePirateSegment? ApplyNinjaSpacePirateCue(RoomEnemyPlace place, NinjaSpacePirateCue cue, SamusState samus)
    {
        NinjaSpacePirateEnemyState state = place.RequireSpeciesState<NinjaSpacePirateEnemyState>();
        switch (cue)
        {
            case NinjaSpacePirateCue.SetShading setShading:
                place.Shading = setShading.Shading;
                return null;
            case NinjaSpacePirateCue.PlaySound playSound:
                Walking.RequestSpacePirateSound(playSound.Sound);
                return null;
            case NinjaSpacePirateCue.ThrowClaw throwClaw:
                SpawnNinjaPirateClaw(place, throwClaw.Throw, throwClaw.Offset);
                return null;
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartActive }:
            {
                NinjaSpacePirateSegment active = place.Position.X.Whole < samus.Position.X.Whole
                    ? NinjaSpacePirateSegment.ActiveFacingRight
                    : NinjaSpacePirateSegment.ActiveFacingLeft;
                place.AnimationClock.StepOnNextUpdate();
                // The cue returns the selected animation, not its next step.
                // Re-entering that animation runs its action cue and restores decision AI
                // after the idle animation set the action that waits for it.
                return active;
            }
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.ResetSpinSpeed }:
                state.SpinSpeed = SignedEightEight.Zero;
                return null;
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartDivekick }:
                InitializeNinjaPirateDive(state);
                return null;
            case NinjaSpacePirateCue.Begin begin:
                // The animation does not jump to the move it begins: main AI runs it from the next update, as the
                // animation runs on.
                state.Action = begin.Action;
                return null;
            case NinjaSpacePirateCue.BeginHeading begin:
                // As a plain begin, main AI runs the move from the next update, the cue's way (#2165).
                state.Action = begin.Action;
                state.Heading = begin.Heading;
                return null;
            default:
                throw new InvalidOperationException($"The ninja Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>Installs an animation and restarts its frame timer.</summary>
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
    private ClawBehavior Claw { get; } = new();

    /// <summary>A thrown claw: it flies out and back along its throw, once its animation throws it (#2278).</summary>
    internal sealed class ClawBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <summary>Each claw's throw and its speed along it, given as its pirate throws it.</summary>
        internal EnemyProjectileStates<NinjaPirateClawState> Claws { get; } = new();

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            FlyNinjaPirateClaw(this, context.Projectile, context.Camera);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// The cue is not a claw's throw, or throws the claw a way other than its thrower chose.
        /// </exception>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            if (cue.Kind != SpacePirateProjectileCueKind.ThrowClaw)
                throw new InvalidOperationException($"A claw's animation published {cue}.");
            HorizontalDirection thrown = Claws.Of(context.Projectile).Thrown;
            if (cue.Direction != thrown)
                throw new InvalidOperationException($"A claw its pirate threw {thrown} played the animation that throws it {cue.Direction}.");
            context.Projectile.StartMotion(this, SingleMotion.Moving);
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
    internal void AdvanceNinjaSpacePirateTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, NinjaSpacePirateAnimations.Segments,
            static (NinjaSpacePirateEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyNinjaSpacePirateCue(place, cue, context.Samus) is { } continuesAt
                ? new AnimationContinuation<NinjaSpacePirateSegment>(NinjaSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Accepts gold ninja extended-hitbox bomb overlaps without damage or generic death.</summary>
    internal static void ReactToGoldNinjaPirateBomb(EnemyBombContext c)
    {
        PirateHitboxShotActions.RequireGoldNinjaNormalBombReaction(c.SelectedShot);
        // Both of the Gold Ninja's body reactions answer only a Power Bomb. The bomb's overlap with the Ninja's hitbox has
        // already been counted, so the bomb touches it without damage, an impact, a reflection or the Gold Ninja's
        // special-drop death.
    }
}
