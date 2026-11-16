using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new NinjaSpacePirateEntity(this, place, common);

    /// <summary>Provides the shared RNG used by divekick selection.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves floor contact during divekick descent.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates thrown claw projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns paired landing dust sprites.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>Receives sound requests from shared Space Pirate animation steps.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;

    /// <summary>The owner of the shared Pirate contact reactions, which every Pirate family takes.</summary>
    internal WalkingSpacePirateBehavior Walking => _walkingSpacePirateBehavior;

    /// <summary>Captures the room, collision, projectile, sprite, and sound owners for ninja AI.</summary>
    internal NinjaSpacePirateBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, RoomEffectActors effectActors, WalkingSpacePirateBehavior walkingSpacePirateBehavior)
    {
        _walkingSpacePirateBehavior = walkingSpacePirateBehavior;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _effectActors = effectActors;
    }

    /// <summary>Speed assigned at the start of a divekick jump and descent, six pixels per update.</summary>
    private static readonly SignedEightEight NinjaPirateInitialDiveSpeed = SignedEightEight.FromParts(6, 0);
    /// <summary>How much a spin jump's speed changes each update, an eighth of a pixel per update.</summary>
    private static readonly SignedEightEight NinjaPirateSpinSpeedChange = SignedEightEight.FromParts(0, 32);
    /// <summary>How much a divekick's speed falls each update, a quarter pixel per update.</summary>
    private static readonly SignedEightEight NinjaPirateDiveSpeedChange = SignedEightEight.FromParts(0, 64);
    /// <summary>
    /// The widest half-span between a ninja's posts, in pixels: the spin's distance then stays below half the counting
    /// range as it accumulates. Every retail ninja's posts lie 0 or 192 pixels apart (#1620).
    /// </summary>
    private const int LongestPostHalfSpan = 127;
    /// <summary>Maximum horizontal distance at which an idle ninja activates.</summary>
    private const int NinjaPirateActivationDistance = 128;
    /// <summary>Strict distance on both axes for a reaction to the highest occupied projectile.</summary>
    private const int NinjaPirateFlinchDistance = 32;
    /// <summary>Strict midpoint distance that permits a spin jump or divekick choice.</summary>
    private const int NinjaPirateMidpointTriggerDistance = 32;
    /// <summary>Strict X and Y distance for a standing kick.</summary>
    private const int NinjaPirateKickDistance = 40;
    /// <summary>Whole-pixel horizontal step during divekick descent.</summary>
    private const int NinjaPirateDiveHorizontalPixels = 5;
    /// <summary>Whole-pixel step while returning to a patrol post after a dive.</summary>
    private const int NinjaPirateReturnWalkPixels = 2;
    /// <summary>Whole-pixel vertical step used by each spin-jump update.</summary>
    private const int NinjaPirateSpinVerticalPixels = 2;

    /// <summary>Identifies the gold and silver ninja Space Pirate enemy definitions, which share this AI.</summary>
    internal static bool IsNinjaSpacePirateDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PirateGoldNinja or
        EnemyDefinitionId.PirateSilverNinja;

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
            ? NinjaSpacePirateSegment.InitialFacingRight
            : NinjaSpacePirateSegment.InitialFacingLeft;
        state.Playback = new(NinjaSpacePirateAnimations.StartingWith(initial), AnimationPlace.StartOfAnimation);
        state.ActiveAnimation = initial;

        // The post distance describes the requested distance between posts. The setup
        // first establishes those requested posts around the population X coordinate, then
        // deliberately replaces them with a symmetric pair derived from its acceleration
        // sum. That adjustment is why the final span can differ slightly from the post distance.
        int requestedLeft = startsAtLeft
            ? place.Position.X.Whole
            : CoordinateAxis.Wrap(place.Position.X.Whole - settings.PostDistance);
        int requestedHalfSpan = Ensure.BetweenInclusive(0, LongestPostHalfSpan, settings.PostDistance / 2);
        state.PostsMidpointX = CoordinateAxis.Wrap(requestedLeft + requestedHalfSpan);

        // The spin's speed grows until the distance it covers reaches the half-span; it runs at least once, so posts no
        // distance apart still spin.
        SignedSixteenSixteen requestedDistance = SignedSixteenSixteen.FromWholePixels(requestedHalfSpan);
        SignedEightEight speed = SignedEightEight.Zero;
        SignedSixteenSixteen covered = SignedSixteenSixteen.Zero;
        do
        {
            speed += NinjaPirateSpinSpeedChange;
            covered += speed.ToSixteenSixteenDelta();
        }
        while (covered < requestedDistance);

        int adjustedHalfSpan = covered.WholePixels;
        state.RightPostX = CoordinateAxis.Wrap(state.PostsMidpointX + adjustedHalfSpan);
        state.LeftPostX = CoordinateAxis.Wrap(state.PostsMidpointX - adjustedHalfSpan);
        place.Position = place.Position.WithX(new(startsAtLeft ? state.LeftPostX : state.RightPostX, 0));
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
            case NinjaSpacePirateAction.Initial:
                RunNinjaPirateInitial(place, state, samus, samusProjectiles);
                return;
            case NinjaSpacePirateAction.Active:
                RunNinjaPirateActive(place, state, samus, samusProjectiles);
                return;
            case NinjaSpacePirateAction.SpinJumpLeftRising:
                StepNinjaPirateSpinJump(place, state, direction: HorizontalDirection.Left, vertical: VerticalDirection.Up);
                return;
            case NinjaSpacePirateAction.SpinJumpLeftFalling:
                StepNinjaPirateSpinJump(place, state, direction: HorizontalDirection.Left, vertical: VerticalDirection.Down);
                return;
            case NinjaSpacePirateAction.SpinJumpRightRising:
                StepNinjaPirateSpinJump(place, state, direction: HorizontalDirection.Right, vertical: VerticalDirection.Up);
                return;
            case NinjaSpacePirateAction.SpinJumpRightFalling:
                StepNinjaPirateSpinJump(place, state, direction: HorizontalDirection.Right, vertical: VerticalDirection.Down);
                return;
            case NinjaSpacePirateAction.ReadyToDivekick:
                RunNinjaPirateReadyToDivekick(
                    place,
                    state,
                    samus,
                    samusProjectiles);
                return;
            case NinjaSpacePirateAction.DivekickLeftJump:
                StepNinjaPirateDiveJump(place, state, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateAction.DivekickRightJump:
                StepNinjaPirateDiveJump(place, state, direction: HorizontalDirection.Right);
                return;
            case NinjaSpacePirateAction.DivekickLeftDive:
                StepNinjaPirateDive(place, state, level, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateAction.DivekickRightDive:
                StepNinjaPirateDive(place, state, level, direction: HorizontalDirection.Right);
                return;
            case NinjaSpacePirateAction.DivekickLeftWalkToPost:
                StepNinjaPirateWalkToPost(place, state, direction: HorizontalDirection.Left);
                return;
            case NinjaSpacePirateAction.DivekickRightWalkToPost:
                StepNinjaPirateWalkToPost(place, state, direction: HorizontalDirection.Right);
                return;
            default:
                throw new InvalidDataException(
                    $"Ninja Space Pirate has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Waits for Samus to approach, then selects the active animation facing her.</summary>
    private static void RunNinjaPirateInitial(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        if (CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) >=
            NinjaPirateActivationDistance)
        {
            _ = TryNinjaPirateProjectileFlinch(place, state, samus, samusProjectiles);
            return;
        }

        NinjaSpacePirateSegment active = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
            ? NinjaSpacePirateSegment.ActiveFacingRight
            : NinjaSpacePirateSegment.ActiveFacingLeft;
        state.ActiveAnimation = active;
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
            CoordinateAxis.Distance(projectile.Position.X.Whole, place.Position.X.Whole) >=
                NinjaPirateFlinchDistance ||
            CoordinateAxis.Distance(projectile.Position.Y.Whole, place.Position.Y.Whole) >=
                NinjaPirateFlinchDistance)
        {
            return false;
        }

        StartNinjaPirateAnimation(
            place,
            state,
            CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
                ? NinjaSpacePirateSegment.FlinchFacingRight
                : NinjaSpacePirateSegment.FlinchFacingLeft);
        return true;
    }

    /// <summary>Starts a facing kick when Samus is within the strict contact range.</summary>
    private static bool TryNinjaPirateStandingKick(RoomEnemyPlace place, NinjaSpacePirateEnemyState state, SamusState samus)
    {
        if (CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) >=
                NinjaPirateKickDistance ||
            CoordinateAxis.Distance(samus.Position.Y.Whole, place.Position.Y.Whole) >=
                NinjaPirateKickDistance)
        {
            return false;
        }

        StartNinjaPirateAnimation(
            place,
            state,
            CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
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
        if (CoordinateAxis.Distance(state.PostsMidpointX, samus.Position.X.Whole) >=
            NinjaPirateMidpointTriggerDistance)
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
        if (!NinjaSpacePirateCadenceDefinitions.ClawAttackEverySixtyFourthUpdate.IsDue(place.FrameCounter))
            return;

        if (place.Position.X.Whole == state.LeftPostX)
        {
            if (CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0)
                return;
            StartNinjaPirateAnimation(
                place,
                state,
                NinjaSpacePirateSegment.ClawAttackLeft);
            return;
        }

        if (CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) >= 0)
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

        if (CoordinateAxis.Distance(state.PostsMidpointX, samus.Position.X.Whole) >=
            NinjaPirateMidpointTriggerDistance)
        {
            return;
        }

        // The table contains three identical entries per side. The random retry is retained
        // because advancing the RNG until its low two bits are nonzero is observable
        // even though all three accepted values select the same animation.
        int choice;
        do
            choice = _room.Random.Next(SpacePirateDecisions.DivekickChoice);
        while (choice == 0);

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
        int xPixels = state.Speed.UnsignedWholePixels;
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + (direction == HorizontalDirection.Right ? xPixels : -xPixels));
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole +
            (vertical == VerticalDirection.Up ? -NinjaPirateSpinVerticalPixels : NinjaPirateSpinVerticalPixels));

        if (vertical == VerticalDirection.Up)
        {
            state.ChangeSpeed(NinjaPirateSpinSpeedChange);
            bool crossedMidpoint = direction == HorizontalDirection.Right
                ? CoordinateAxis.Difference(place.Position.X.Whole, state.PostsMidpointX) >= 0
                : CoordinateAxis.Precedes(place.Position.X.Whole, state.PostsMidpointX);
            if (crossedMidpoint)
            {
                state.Action = direction == HorizontalDirection.Right
                    ? NinjaSpacePirateAction.SpinJumpRightFalling
                    : NinjaSpacePirateAction.SpinJumpLeftFalling;
            }
            return;
        }

        state.ChangeSpeed(-NinjaPirateSpinSpeedChange);
        if (!state.Speed.IsZero)
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
    /// Sets the divekick's initial rising speed. The game also stores a target midway between a post and the
    /// patrol centre, which nothing reads; it is not kept.
    /// </summary>
    private static void InitializeNinjaPirateDive(NinjaSpacePirateEnemyState state) =>
        state.Speed = NinjaPirateInitialDiveSpeed;

    /// <summary>Rises until signed speed turns negative, then switches to the descent animation.</summary>
    private static void StepNinjaPirateDiveJump(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole - state.Speed.UnsignedWholePixels);
        state.ChangeSpeed(-NinjaPirateDiveSpeedChange);
        if (!state.Speed.IsNegative)
            return;

        state.Action = direction == HorizontalDirection.Right
            ? NinjaSpacePirateAction.DivekickRightDive
            : NinjaSpacePirateAction.DivekickLeftDive;
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.DivekickRightDive
                : NinjaSpacePirateSegment.DivekickLeftDive);
        state.Speed = NinjaPirateInitialDiveSpeed;
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
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole +
            (direction == HorizontalDirection.Right ? NinjaPirateDiveHorizontalPixels : -NinjaPirateDiveHorizontalPixels));

        bool landed = _terrain.MoveEnemyVertically(level, place, NinjaPirateDiveFallStep.Of(state.Speed));
        if (!landed)
        {
            state.ChangeSpeed(-NinjaPirateDiveSpeedChange);
            landed = state.Speed.IsNegative || state.Speed.UnsignedWholePixels == 0;
        }
        if (!landed)
            return;

        state.Action = direction == HorizontalDirection.Right
            ? NinjaSpacePirateAction.DivekickRightWalkToPost
            : NinjaSpacePirateAction.DivekickLeftWalkToPost;
        StartNinjaPirateAnimation(
            place,
            state,
            direction == HorizontalDirection.Right
                ? NinjaSpacePirateSegment.WalkToRightPost
                : NinjaSpacePirateSegment.WalkToLeftPost);
        // Landing at either post stores the spawn row's whole Y only; the dive's Y fraction stays.
        place.Position = place.Position.WithWholeY(state.SpawnPosition.Y);
        SpawnNinjaPirateLandingDust(place);
    }

    /// <summary>Walks back to the selected post and restores its landing animation.</summary>
    private static void StepNinjaPirateWalkToPost(
        RoomEnemyPlace place,
        NinjaSpacePirateEnemyState state,
        HorizontalDirection direction)
    {
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole +
            (direction == HorizontalDirection.Right ? NinjaPirateReturnWalkPixels : -NinjaPirateReturnWalkPixels));
        bool reachedPost = direction == HorizontalDirection.Right
            ? CoordinateAxis.Difference(place.Position.X.Whole, state.RightPostX) >= 0
            : CoordinateAxis.Precedes(place.Position.X.Whole, state.LeftPostX);
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
            new(CoordinateAxis.Wrap(place.Position.X.Whole - 8), CoordinateAxis.Wrap(place.Position.Y.Whole + 28)),
            RoomEffectActorKind.NinjaPirateLandingDust,
            graphics);
        _effectActors.SpawnRoomEffectActor(
            new(CoordinateAxis.Wrap(place.Position.X.Whole + 8), CoordinateAxis.Wrap(place.Position.Y.Whole + 28)),
            RoomEffectActorKind.NinjaPirateLandingDust,
            graphics);
    }

    /// <summary>A thrown claw's launch speed: eight pixels an update.</summary>
    private static readonly SignedEightEight NinjaPirateClawLaunchSpeed = SignedEightEight.FromPixels(8);

    /// <summary>Allocates a claw with the selected throw direction, offset, graphics, and outbound speed.</summary>
    internal void SpawnNinjaPirateClaw(
        RoomEnemyPlace source,
        bool throwsRight,
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
        projectile.StartAnimation(new SpacePirateProjectileAnimation(throwsRight
            ? SpacePirateProjectileSegment.ClawRight
            : SpacePirateProjectileSegment.ClawLeft));
        projectile.State = new NinjaPirateClawState(NinjaPirateClawLaunchSpeed);
    }

    /// <summary>Flies a claw thrown toward <paramref name="thrown"/> out and back, with exact camera culling.</summary>
    private static void FlyNinjaPirateClaw(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera,
        HorizontalDirection thrown)
    {
        NinjaPirateClawState claw = projectile.StateOf<NinjaPirateClawState>();
        int direction = thrown == HorizontalDirection.Right ? 1 : -1;

        // Outbound motion follows the throw direction while decelerating to zero; inbound
        // motion reverses direction and accelerates indefinitely until the claw leaves the view, whose right and
        // bottom edges are already outside. Y advances one pixel on both halves of the flight.
        projectile.Position = projectile.Position.WithWrappedWholeX(projectile.Position.X.Whole +
            direction * claw.StepAlongThrow());
        projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 1);

        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }

    /// <summary>The action main AI runs for the move a cue of <paramref name="kind"/> begins.</summary>
    internal static NinjaSpacePirateAction ActionBegunBy(NinjaSpacePirateCueKind kind) => kind switch
    {
        NinjaSpacePirateCueKind.StandStill => NinjaSpacePirateAction.StandingStill,
        NinjaSpacePirateCueKind.WaitForSamus => NinjaSpacePirateAction.Initial,
        NinjaSpacePirateCueKind.ChooseAttack => NinjaSpacePirateAction.Active,
        NinjaSpacePirateCueKind.SpinJumpLeft => NinjaSpacePirateAction.SpinJumpLeftRising,
        NinjaSpacePirateCueKind.SpinJumpRight => NinjaSpacePirateAction.SpinJumpRightRising,
        NinjaSpacePirateCueKind.ReadyDivekick => NinjaSpacePirateAction.ReadyToDivekick,
        NinjaSpacePirateCueKind.JumpToDivekickLeft => NinjaSpacePirateAction.DivekickLeftJump,
        NinjaSpacePirateCueKind.JumpToDivekickRight => NinjaSpacePirateAction.DivekickRightJump,
        NinjaSpacePirateCueKind.DiveLeft => NinjaSpacePirateAction.DivekickLeftDive,
        NinjaSpacePirateCueKind.DiveRight => NinjaSpacePirateAction.DivekickRightDive,
        NinjaSpacePirateCueKind.WalkBackToLeftPost => NinjaSpacePirateAction.DivekickLeftWalkToPost,
        NinjaSpacePirateCueKind.WalkBackToRightPost => NinjaSpacePirateAction.DivekickRightWalkToPost,
        NinjaSpacePirateCueKind.ResetSpeed or NinjaSpacePirateCueKind.StartActive or NinjaSpacePirateCueKind.StartDivekick or _ =>
            throw new InvalidOperationException($"A ninja Space Pirate {kind} cue begins no move."),
    };

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
                _walkingSpacePirateBehavior.RequestSpacePirateSound(playSound.Sound ?? throw new InvalidOperationException(
                    "A ninja Space Pirate sound cue carries no sound."));
                return null;
            case NinjaSpacePirateCue.ThrowClaw throwClaw:
                SpawnNinjaPirateClaw(place, throwClaw.Throw == HorizontalDirection.Right, throwClaw.Offset);
                return null;
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartActive }:
                state.ActiveAnimation = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
                    ? NinjaSpacePirateSegment.ActiveFacingRight
                    : NinjaSpacePirateSegment.ActiveFacingLeft;
                place.AnimationClock.StepOnNextUpdate();
                // The cue returns the selected animation, not its next step.
                // Re-entering that animation runs its action cue and restores decision AI
                // after the idle animation explicitly set the no-op action.
                return state.ActiveAnimation;
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.ResetSpeed }:
                state.Speed = default;
                return null;
            case NinjaSpacePirateCue.Signal { Kind: NinjaSpacePirateCueKind.StartDivekick }:
                InitializeNinjaPirateDive(state);
                return null;
            case NinjaSpacePirateCue.Signal begun:
                // The animation does not jump to the move it begins: main AI runs it from the next update, as the
                // animation runs on.
                state.Action = ActionBegunBy(begun.Kind);
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

    /// <summary>A Ninja Pirate's claw (#1760): its spawner starts its animation, which installs its throw left or right.</summary>
    internal static readonly EnemyProjectileBehavior Claw = new ClawBehavior();

    /// <summary>A thrown claw: it flies out and back in its throw's direction, once its animation throws it.</summary>
    private sealed class ClawBehavior : EnemyProjectileMotionBehavior<PirateClawMotion>, IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, PirateClawMotion motion)
        {
            switch (motion)
            {
                case PirateClawMotion.ThrownLeft:
                    FlyNinjaPirateClaw(context.Projectile, context.Camera, HorizontalDirection.Left);
                    break;
                case PirateClawMotion.ThrownRight:
                    FlyNinjaPirateClaw(context.Projectile, context.Camera, HorizontalDirection.Right);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PirateClawMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            if (cue.Kind != SpacePirateProjectileCueKind.ThrowClaw)
                throw new InvalidOperationException($"A claw's animation published {cue}.");
            context.Projectile.StartMotion(cue.Direction == HorizontalDirection.Right ? PirateClawMotion.ThrownRight : PirateClawMotion.ThrownLeft);
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
        PirateHitboxShotActions.PirateHitboxShotAction pirateAction = PirateHitboxShotActions.SelectPirateNormalBombHitboxShotAction(c.Enemy, c.SelectedShot);
        if (pirateAction != PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
            throw new InvalidDataException("A Gold Ninja Space Pirate's normal-bomb reaction selected an action other than ignoring the bomb.");
        // Both gold-Ninja component reactions compare the normal-bomb family against the Power Bomb family and return
        // immediately. The extended collision walker has already marked the bomb, so this is an
        // accepted physical overlap without damage, impact replacement, reflection, or the Lower-Norfair
        // special-drop death tail.
    }
}
