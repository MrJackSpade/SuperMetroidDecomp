using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The Ki-Hunter species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KiHunterBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [
        EnemyDefinitionId.KihunterGreen, EnemyDefinitionId.KihunterGreenWings, EnemyDefinitionId.KihunterRed,
        EnemyDefinitionId.KihunterRedWings, EnemyDefinitionId.KihunterYellow, EnemyDefinitionId.KihunterYellowWings,
    ];

    /// <summary>A body of any colour, or its wings (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ki-Hunter definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterRed or EnemyDefinitionId.KihunterYellow)
            return new KiHunterBodyEntity(this, place, common);
        if (definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterRedWings or EnemyDefinitionId.KihunterYellowWings)
            return new KiHunterWingsEntity(this, place, common);
        throw new InvalidOperationException($"The Ki-Hunter species creates no {place.DefinitionId}.");
    }

    /// <summary>Provides the active room state used by Ki-Hunter movement.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Checks the flying and wingless body's block collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the acid spit produced by the body animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the acid-spit sound under the enemy queue limit.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The room's enemy places, where a body and its wings find each other when they are placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The wings each body found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _wingsOfBody = new();
    /// <summary>The body each pair of wings found when it was placed, which its room lists right before them.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _bodyOfWings = new();
    /// <summary>
    /// The settings of the wings each body's wings were placed with, which name the health at which the body loses them;
    /// a body reads them after its wings were deleted as set up, as a wingless Ki-Hunter's are.
    /// </summary>
    private readonly EnemyPartLinks<KiHunterWingsSpawnSettings> _wingSettingsOfBody = new();

    /// <summary>Connects Ki-Hunter AI to room collision, projectile, audio and place services.</summary>
    internal KiHunterBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyPlaces places)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _places = places;
    }

    /// <summary>
    /// Carries out one cue of the Ki-Hunter body's animations. Returning to flight continues at the flying animation for
    /// the current direction, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private KiHunterSegment? ApplyKiHunterCue(RoomEnemyPlace place, KiHunterCue cue)
    {
        if (cue is KiHunterCue.Spit spit)
        {
            SpawnKiHunterAcidFromCue(place, direction: spit.Direction);
            return null;
        }
        KiHunterCueKind kind = ((KiHunterCue.Signal)cue).Kind;
        switch (kind)
        {
            case KiHunterCueKind.ReturnToFlying:
                // The cue returns the body's flying animation while separately restarting the attached wings.
                return ReturnKiHunterToSteadyAnimation(place);
            case KiHunterCueKind.StartHop:
                StartKiHunterGroundJumpFromCue(place.RequireSpeciesState<KiHunterBodyState>());
                return null;
            case KiHunterCueKind.StartWinglessThinking:
                StartKiHunterGroundWaitFromCue(place.RequireSpeciesState<KiHunterBodyState>());
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KiHunterCueKind)} {kind}.");
        }
    }

    /// <summary>Initializes the Ki-Hunter body.</summary>
    internal void InitializeKiHunter(RoomEnemyPlace body)
    {
        _wingsOfBody.Place(body, _places.ListedAfter(body, 1));
        var state = new KiHunterBodyState(KiHunterAction.FlyingPatrol);
        body.SetSpeciesState(state);

        // Every Ki-Hunter animation is played by the shared enemy animation, which this property admits.
        body.Processing = body.Processing.With(EnemyProcessing.Animates);
        state.HasLostWings = false;
        StartKiHunterAnimation(body, state, KiHunterSegment.FlyingLeft);
        state.SwoopAngle = RotationAngle.Zero;
        state.Heading = KiHunterMotionDefinitions.PatrolStartHeading;
        state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
        state.VerticalVelocity = SignedSixteenSixteen.FromWholePixels(KiHunterMotionDefinitions.PatrolSpeed);
        state.UpperPatrolY = CoordinateAxis.Wrap(body.Position.Y.Whole - KiHunterMotionDefinitions.PatrolRise);
        state.LowerPatrolY = CoordinateAxis.Wrap(state.UpperPatrolY + KiHunterMotionDefinitions.PatrolDepth);
        state.SpawnPosition = body.Position.Whole;

        // A wingless Ki-Hunter is the permanently ground-bound form. The wing listed after it
        // starts deleted and the body skips every later detachment side effect.
        if (body.SpawnSettings<KiHunterSpawnSettings>().Wings == KiHunterWings.Wingless)
        {
            state.HasLostWings = true;
            state.Action = KiHunterAction.FallingAfterWingLoss;
            state.VerticalVelocity = KiHunterMotionDefinitions.WinglessFallStartVelocity;
        }
    }

    /// <summary>Initializes the Ki-Hunter's wings, which find their body listed right before them.</summary>
    internal void InitializeKiHunterWings(RoomEnemyPlace wings)
    {
        _bodyOfWings.Place(wings, _places.ListedBefore(wings, 1));
        RoomEnemyPlace body = GetKiHunterBody(wings);
        _wingSettingsOfBody.Place(body, wings.SpawnSettings<KiHunterWingsSpawnSettings>());
        var state = new KiHunterWingState(KiHunterWingAction.FollowBody);
        wings.SetSpeciesState(state);

        wings.Processing = wings.Processing.With(EnemyProcessing.Animates);
        StartKiHunterAnimation(wings, state, KiHunterSegment.WingsLeft);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
        if (body.SpawnSettings<KiHunterSpawnSettings>().Wings == KiHunterWings.Wingless)
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
    }

    /// <summary>Runs the body's main behaviour.</summary>
    private void RunKiHunterMain(
        RoomEnemyPlace actor,
        KiHunterBodyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case KiHunterAction.FlyingPatrol:
                RunKiHunterFlyingPatrol(actor, state, samus, level);
                return;
            case KiHunterAction.Swooping:
                RunKiHunterSwoop(actor, state, level);
                return;
            case KiHunterAction.RecoveringFromSwoop:
                RunKiHunterSwoopRecovery(actor, state, level);
                return;
            case KiHunterAction.FallingAfterWingLoss:
                RunKiHunterFalling(actor, state, level);
                return;
            case KiHunterAction.StartGroundJump:
                StartKiHunterGroundJump(actor, state, samus);
                return;
            case KiHunterAction.AnimatingOnGround:
                return;
            case KiHunterAction.GroundJump:
                RunKiHunterGroundJump(actor, state, level);
                return;
            case KiHunterAction.GroundWait:
                RunKiHunterGroundWait(actor, state, samus);
                return;
            case KiHunterAction.SelectAcidSpit:
                SelectKiHunterAcidSpit(actor, state, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Ki-Hunter body has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the wings' main behaviour: following the body, or flying off once detached.</summary>
    private void RunKiHunterWingMain(RoomEnemyPlace wings, KiHunterWingState state, RoomLevelData level)
    {
        switch (state.Action)
        {
            case KiHunterWingAction.FollowBody:
                FollowKiHunterBody(wings);
                return;
            case KiHunterWingAction.Detached:
                RunDetachedKiHunterWing(wings, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Ki-Hunter wings have no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Flies the patrol.</summary>
    private void RunKiHunterFlyingPatrol(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.VerticalVelocity))
        {
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixels(-state.VerticalVelocity.WholePixels);
        }
        else if (CoordinateAxis.Precedes(body.Position.Y.Whole, state.UpperPatrolY))
        {
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixels(KiHunterMotionDefinitions.PatrolSpeed);
        }
        else if (!CoordinateAxis.Precedes(body.Position.Y.Whole, state.LowerPatrolY))
        {
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixels(-KiHunterMotionDefinitions.PatrolSpeed);
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.HorizontalVelocity))
        {
            state.Heading = state.Heading.Reversed();
            SetKiHunterPairFacing(body, state.Heading);
        }
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);

        int signedXDistance = CoordinateAxis.Difference(samus.Position.X.Whole, body.Position.X.Whole);
        int absoluteXDistance = Math.Abs(signedXDistance);
        int samusDepth = CoordinateAxis.Difference(samus.Position.Y.Whole, body.Position.Y.Whole);
        if (absoluteXDistance >= KiHunterMotionDefinitions.SwoopTriggerDistance || samusDepth < KiHunterMotionDefinitions.SwoopLeastDepth)
            return;

        state.AngularVelocity = AngularSpeed.Zero;
        state.SwoopAnimationChanged = false;
        state.SwoopRadius = new Extent16<RoomPixels>(absoluteXDistance, samusDepth);
        state.SwoopTargetX = samus.Position.X.Whole;
        state.SwoopOriginY = body.Position.Y.Whole;
        state.Action = KiHunterAction.Swooping;

        // The swoop heads toward Samus and arcs that way (#2338).
        state.Heading = signedXDistance < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
        if (state.Heading == HorizontalDirection.Left)
        {
            state.SwoopAngle = KiHunterMotionDefinitions.LeftSwoopStartAngle;
            state.AnimationChangeAngle = KiHunterMotionDefinitions.LeftSwoopAnimationChangeAngle;
        }
        else
        {
            state.SwoopAngle = KiHunterMotionDefinitions.RightSwoopStartAngle;
            state.AnimationChangeAngle = KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle;
        }
        SetKiHunterPairFacing(body, state.Heading);
    }

    /// <summary>
    /// <paramref name="speed"/> turning a swoop heading <paramref name="heading"/> (#2338): toward increasing angles for a
    /// rightward swoop, decreasing ones for a leftward one.
    /// </summary>
    private static AngularSpeed SwoopTurning(AngularSpeed speed, HorizontalDirection heading) =>
        heading == HorizontalDirection.Left ? -speed : speed;

    /// <summary>Runs the elliptic diving arc.</summary>
    private void RunKiHunterSwoop(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        bool leftArc = state.Heading == HorizontalDirection.Left;
        bool beforeAnimationChange = leftArc
            ? state.SwoopAngle >= state.AnimationChangeAngle
            : state.SwoopAngle < state.AnimationChangeAngle;
        if (!beforeAnimationChange && !state.SwoopAnimationChanged)
        {
            state.SwoopAnimationChanged = true;
            StartKiHunterAnimation(
                body,
                body.RequireSpeciesState<KiHunterBodyState>(),
                leftArc
                    ? KiHunterSegment.SwoopLeft
                    : KiHunterSegment.SwoopRight);
        }

        // The cap holds only the whole steps; the fraction keeps accumulating beneath it.
        state.AngularVelocity = (state.AngularVelocity + SwoopTurning(KiHunterMotionDefinitions.SwoopAcceleration, state.Heading))
            .WithWholeStepsLimitedTo(SwoopTurning(KiHunterMotionDefinitions.SwoopMaximumAngularVelocity, state.Heading));

        // The swoop runs through the second half of the turn: a rightward swoop ends as its angle comes round past the
        // full turn onto the first half, a leftward one as it falls below the half turn. At two steps an update neither
        // can cross the other end of that half.
        state.SwoopAngle = state.SwoopAngle.TurnedBySteps(state.AngularVelocity.WholeSteps);
        if (state.SwoopAngle < RotationAngle.HalfTurn)
        {
            state.Action = KiHunterAction.FlyingPatrol;
            return;
        }

        TrigonometryAngle tableAngle = state.SwoopAngle.ToTrigonometryAngle();
        int targetX = CoordinateAxis.Wrap(
            state.SwoopTargetX +
            EnemyMath.CircleCosinePixels(tableAngle, state.SwoopRadius.X));
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(targetX, body.Position.X.Whole));
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            // Striking a wall, the body flies back the other way (#2338).
            state.Heading = state.Heading.Reversed();
            state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
            StartKiHunterSwoopRecovery(state);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        int targetY = CoordinateAxis.Wrap(
            state.SwoopOriginY +
            EnemyMath.CircleNegativeSinePixels(tableAngle, state.SwoopRadius.Y));
        SignedSixteenSixteen verticalDisplacement = SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(targetY, body.Position.Y.Whole));
        if (_terrain.MoveEnemyVertically(level, body, verticalDisplacement))
            StartKiHunterSwoopRecovery(state);
    }

    /// <summary>Switches a flying body from its dive arc to the collision escape leg.</summary>
    private static void StartKiHunterSwoopRecovery(KiHunterBodyState state)
    {
        state.Action = KiHunterAction.RecoveringFromSwoop;
        state.VerticalVelocity = KiHunterMotionDefinitions.SwoopRecoveryVerticalVelocity;
    }

    /// <summary>Runs the collision escape leg.</summary>
    private void RunKiHunterSwoopRecovery(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.HorizontalVelocity))
        {
            state.Action = KiHunterAction.FlyingPatrol;
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.VerticalVelocity) ||
            CoordinateAxis.Difference(body.Position.Y.Whole, state.SpawnPosition.Y) < 0)
        {
            state.Action = KiHunterAction.FlyingPatrol;
        }
    }

    /// <summary>Falls under gravity after losing the wings.</summary>
    private void RunKiHunterFalling(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.VerticalVelocity))
        {
            state.Action = KiHunterAction.StartGroundJump;
            return;
        }

        state.VerticalVelocity += KiHunterMotionDefinitions.Gravity;
    }

    /// <summary>Sets up a grounded jump.</summary>
    private void StartKiHunterGroundJump(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        SamusState samus)
    {
        state.Action = KiHunterAction.AnimatingOnGround;
        int random = _room.Random.Read();
        // The hop heads toward Samus, leftward when she is not to the body's right (#2338).
        state.Heading = CoordinateAxis.Difference(body.Position.X.Whole, samus.Position.X.Whole) >= 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        state.HorizontalSpeed = KiHunterMotionDefinitions.GroundJumpHorizontalSpeed;
        state.VerticalVelocity = SignedSixteenSixteen.FromWholePixels(
            KiHunterMotionDefinitions.ShallowerGroundJump.Of(random) - KiHunterMotionDefinitions.GroundJumpUpwardSpeed);
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterBodyState>(),
            state.Heading == HorizontalDirection.Left
                ? KiHunterSegment.JumpLeft
                : KiHunterSegment.JumpRight);
    }

    /// <summary>Moves through an airborne hop.</summary>
    private void RunKiHunterGroundJump(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.VerticalVelocity))
        {
            if (state.VerticalVelocity.WholePixels < 0)
            {
                state.VerticalVelocity = state.VerticalVelocity.WithWholePixels(KiHunterMotionDefinitions.HopReboundSpeed);
            }
            else
            {
                state.VerticalVelocity = KiHunterMotionDefinitions.LandedVerticalVelocity;
                state.Action = KiHunterAction.AnimatingOnGround;
                state.WaitTimer = KiHunterMotionDefinitions.LandingWait;
                StartKiHunterAnimation(
                    body,
                    body.RequireSpeciesState<KiHunterBodyState>(),
                    state.Heading == HorizontalDirection.Left
                        ? KiHunterSegment.LandLeft
                        : KiHunterSegment.LandRight);
            }
            return;
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.HorizontalVelocity))
        {
            state.Heading = state.Heading.Reversed();
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        state.VerticalVelocity += KiHunterMotionDefinitions.Gravity;
    }

    /// <summary>Waits after landing and decides what to do next.</summary>
    private static void RunKiHunterGroundWait(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        SamusState samus)
    {
        if (--state.WaitTimer != 0)
            return;

        int distance = CoordinateAxis.Distance(body.Position.X.Whole, samus.Position.X.Whole);
        state.Action = distance < KiHunterMotionDefinitions.SpitRange
            ? KiHunterAction.SelectAcidSpit
            : KiHunterAction.StartGroundJump;
    }

    /// <summary>Selects the left or right acid-spit animation.</summary>
    private static void SelectKiHunterAcidSpit(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        SamusState samus)
    {
        bool samusRight = CoordinateAxis.Difference(body.Position.X.Whole, samus.Position.X.Whole) < 0;
        StartKiHunterAnimation(
            body,
            state,
            samusRight
                ? KiHunterSegment.SpitRight
                : KiHunterSegment.SpitLeft);
        state.Action = KiHunterAction.AnimatingOnGround;
    }

    /// <summary>Moves attached wings with the body.</summary>
    private void FollowKiHunterBody(RoomEnemyPlace wings)
    {
        RoomEnemyPlace body = GetKiHunterBody(wings);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
    }

    /// <summary>Runs the detached wings' behaviour.</summary>
    private void RunDetachedKiHunterWing(
        RoomEnemyPlace wings,
        KiHunterWingState state,
        RoomLevelData level)
    {
        switch (state.WingMovement)
        {
            case KiHunterWingMovement.Orbit:
                RunDetachedKiHunterWingOrbit(wings, state);
                return;
            case KiHunterWingMovement.FallingCollisionArc:
                RunDetachedKiHunterWingCollisionArc(wings, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Detached Ki-Hunter has no behaviour for wing movement {state.WingMovement}.");
        }
    }

    /// <summary>Advances the detached wings' orbit, including its odd authored speed read.</summary>
    internal static void RunDetachedKiHunterWingOrbit(RoomEnemyPlace wings, KiHunterWingState state)
    {
        state.WingArcAngle = state.WingArcAngle.TurnedByFractionsOfStep(DetachedWingCoarseSpeed(
            state.DetachedSpeedCurvePlace,
            sign: EnemySpeedSign.Negative));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + EnemyMath.CircleOffset(trigonometryAngle, radius) - state.OrbitOffset);

        if (LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingCollisionArc(wings, state);
            return;
        }
        SlowDetachedKiHunterWing(state);
    }

    /// <summary>Advances the detached wings' collision-bearing arc.</summary>
    private void RunDetachedKiHunterWingCollisionArc(
        RoomEnemyPlace wings,
        KiHunterWingState state,
        RoomLevelData level)
    {
        state.WingArcAngle = state.WingArcAngle.TurnedByFractionsOfStep(DetachedWingCoarseSpeed(
            state.DetachedSpeedCurvePlace,
            sign: EnemySpeedSign.Positive));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        int desiredY = CoordinateAxis.Wrap(
            state.OrbitCenter.Y +
            EnemyMath.CircleNegativeSinePixels(trigonometryAngle, radius) -
            state.FallingArcOffset.Y);
        if (_terrain.MoveEnemyVertically(
                level,
                wings,
                SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(desiredY, wings.Position.Y.Whole))))
        {
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
            wings.Position = wings.Position.WithWhole(state.SavedWingPosition);
            SlowDetachedKiHunterWing(state);
            return;
        }

        wings.Position = wings.Position.WithWrappedWholeX(
            state.OrbitCenter.X + EnemyMath.CircleCosinePixels(trigonometryAngle, radius) - state.FallingArcOffset.X);
        if (!LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingOrbit(wings, state);
            return;
        }
        SlowDetachedKiHunterWing(state);
    }

    /// <summary>
    /// Runs the private tail after common shot/power-bomb AI. A surviving
    /// body mirrors freeze/hurt timing into its wing until health reaches the wing's
    /// detach health; that exact threshold—not an invented constant—starts detachment.
    /// </summary>
    private void ResolveKiHunterShotAfterCommon(RoomEnemyPlace body)
    {
        RoomEnemyPlace wings = GetKiHunterWings(body);
        bool wingsInPlace = WingsInPlace(wings) is not null;
        if (body.Health == 0)
        {
            if (!wingsInPlace)
                return;
            // Killing the body deletes its wings outright, stripping processing and every other population flag rather
            // than merely adding the deleted flag.
            wings.Contact = EnemyContact.None;
            wings.Processing = EnemyProcessing.None;
            wings.Presence = EnemyPresence.Deleted;
            return;
        }

        KiHunterBodyState bodyState = body.RequireSpeciesState<KiHunterBodyState>();
        if (body.Health <= _wingSettingsOfBody.Of(body).DetachHealth)
        {
            if (!bodyState.HasLostWings)
            {
                // A body that has not lost its wings still has them in place.
                KiHunterWingState wingState = wings.RequireSpeciesState<KiHunterWingState>();
                bodyState.HasLostWings = true;
                bodyState.Action = KiHunterAction.FallingAfterWingLoss;
                bodyState.VerticalVelocity = KiHunterMotionDefinitions.WinglessFallStartVelocity;
                if (wingState.Action != KiHunterWingAction.Detached)
                    DetachKiHunterWings(wings, wingState);
            }
            return;
        }

        // Wings still in place freeze, flash and recover with their body; wings that are gone have nothing to keep in step.
        if (!wingsInPlace)
            return;
        wings.AiStates = body.AiStates;
        wings.FrozenTimer = body.FrozenTimer;
        wings.InvincibilityTimer = body.InvincibilityTimer;
        wings.FlashTimer = body.FlashTimer;
    }

    /// <summary>Initializes all fields the wing detachment writes.</summary>
    private static void DetachKiHunterWings(RoomEnemyPlace wings, KiHunterWingState state)
    {
        state.SavedWingPosition = wings.Position.Whole;
        state.DetachedSpeedReset = KiHunterMotionDefinitions.DetachedWingClimb.StartPlace;

        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        state.OrbitOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.DetachedOrbitOffsetAngle, radius);
        state.FallingArcOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.FallingArcOffsetAngle, radius);
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.Action = KiHunterWingAction.Detached;
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.OrbitCenter = state.SavedWingPosition;
        state.DetachedSpeedCurvePlace = state.DetachedSpeedReset;
        StartKiHunterAnimation(
            wings,
            state,
            KiHunterSegment.DetachedWings);
        wings.Frame = EnemyFrameId.Empty;
        wings.Processing = wings.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemyPlace wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.DetachedSpeedCurvePlace = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemyPlace wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.FallingCollisionArc;
        state.DetachedSpeedCurvePlace = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.FallingArcStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>
    /// The detached wing's speed along its orbit each update: its curve mark's coarse speed in the direction it turns,
    /// in 256ths of a pixel. The orbit turns a 256th of a step for each 256th of a pixel of it, so its whole pixels turn
    /// whole steps; a speed toward decreasing coordinates is negative and turns the orbit back.
    /// </summary>
    internal static int DetachedWingCoarseSpeed(QuadraticSpeedCurvePlace place, EnemySpeedSign sign)
    {
        // The orbit steps by the coarse speed; stepping by the full speed changes the orbit.
        return place.Speed.CoarseToward(sign);
    }

    /// <summary>Slows a detached wing by one speed step, landing on the slowest place on the curve when the step would go below its start.</summary>
    private static void SlowDetachedKiHunterWing(KiHunterWingState state)
    {
        QuadraticSpeedCurvePlace next = state.DetachedSpeedCurvePlace - KiHunterMotionDefinitions.DetachedWingClimb.StepPerPass;
        state.DetachedSpeedCurvePlace = next.IsNegative ? KiHunterMotionDefinitions.SlowestDetachedWingPlace : next;
    }

    /// <summary>
    /// Whether a detached wing's arc angle lies from a quarter turn up to three quarters: the game's check that
    /// the angle plus a quarter turn lies in the second half-turn, which switches the wing from its orbit to its falling arc and back.
    /// </summary>
    private static bool LiesWithinMiddleHalfTurn(RotationAngle angle) =>
        angle + RotationAngle.QuarterTurn >= RotationAngle.HalfTurn;

    /// <summary>Animation cue: return to steady art and sync attached wings.</summary>
    private KiHunterSegment ReturnKiHunterToSteadyAnimation(RoomEnemyPlace body)
    {
        KiHunterBodyState state = body.RequireSpeciesState<KiHunterBodyState>();
        bool movingRight = state.Heading == HorizontalDirection.Right;
        KiHunterSegment wingSegment = movingRight
            ? KiHunterSegment.WingsRight
            : KiHunterSegment.WingsLeft;
        RoomEnemyPlace wings = GetKiHunterWings(body);
        if (WingsInPlace(wings) is { Action: KiHunterWingAction.FollowBody } wingState)
            StartKiHunterAnimation(wings, wingState, wingSegment);
        return movingRight
            ? KiHunterSegment.FlyingRight
            : KiHunterSegment.FlyingLeft;
    }

    /// <summary>Lets the body's animation cue enter its wingless hop action.</summary>
    private static void StartKiHunterGroundJumpFromCue(KiHunterBodyState state) =>
        state.Action = KiHunterAction.GroundJump;

    /// <summary>Returns the body to its wingless decision action after an animation cue.</summary>
    private static void StartKiHunterGroundWaitFromCue(KiHunterBodyState state) =>
        state.Action = KiHunterAction.GroundWait;

    /// <summary>Emits the acid projectile and sound, then starts the 24-frame spit wait.</summary>
    private void SpawnKiHunterAcidFromCue(RoomEnemyPlace body, HorizontalDirection direction)
    {
        _audio.QueueEnemySound(EnemySound.AcidSpit, priority: SoundRequestPriority.Normal);
        SpawnKiHunterAcidSpit(body, direction);
        body.RequireSpeciesState<KiHunterBodyState>().WaitTimer = KiHunterMotionDefinitions.SpitWait;
    }

    /// <summary>Restarts matching body and still-attached wing animations for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemyPlace body, HorizontalDirection facing)
    {
        bool movingRight = facing == HorizontalDirection.Right;
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterBodyState>(),
            movingRight
                ? KiHunterSegment.FlyingRight
                : KiHunterSegment.FlyingLeft);
        RoomEnemyPlace wings = GetKiHunterWings(body);
        if (WingsInPlace(wings) is { Action: KiHunterWingAction.FollowBody } wingState)
        {
            StartKiHunterAnimation(
                wings,
                wingState,
                movingRight
                    ? KiHunterSegment.WingsRight
                    : KiHunterSegment.WingsLeft);
        }
    }

    /// <summary>
    /// The state of a body's wings while they hold their place, or null once they are gone: a wingless Ki-Hunter's
    /// wings are deleted as they are set up, and detached wings are deleted where they land. A body whose wings are
    /// gone has none to keep in step with its flight.
    /// </summary>
    private static KiHunterWingState? WingsInPlace(RoomEnemyPlace wings) => wings.SpeciesState<KiHunterWingState>();

    /// <summary>
    /// The wings the body found when it was placed, which its room lists right after it. They may already be gone, as a
    /// wingless Ki-Hunter's are from the start, and the body's behaviour asks <see cref="WingsInPlace"/> before it
    /// touches them.
    /// </summary>
    private RoomEnemyPlace GetKiHunterWings(RoomEnemyPlace body) => _wingsOfBody.Of(body);

    /// <summary>The body the wings found when they were placed, which their room lists right before them.</summary>
    private RoomEnemyPlace GetKiHunterBody(RoomEnemyPlace wings) => _bodyOfWings.Of(wings);

    /// <summary>Restarts a body animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemyPlace body, KiHunterBodyState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(body, KiHunterAnimations.StartingWith(segment));

    /// <summary>Restarts a wing animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemyPlace wings, KiHunterWingState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(wings, KiHunterAnimations.StartingWith(segment));

    /// <summary>Runs wing-detachment logic after common bomb damage to a Ki-Hunter body.</summary>
    internal EnemyBombReaction KiHunterBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
    };

    /// <summary>Ki-Hunter acid, spat either way (#1760).</summary>
    internal static readonly EnemyProjectileBehavior Acid = new AcidBehavior();

    /// <summary>
    /// Ki-Hunter acid: it holds still in the Ki-Hunter's mouth until its animation launches it in its direction, then flies
    /// under gravity until it strikes the room, where its splash stops it.
    /// </summary>
    private sealed class AcidBehavior : EnemyProjectileMotionBehavior<KiHunterAcidMotion>, IEnemyProjectileCues<KiHunterAcidSpitCue>
    {
        /// <summary>Carries out one cue of the acid's animations: its launch either way, its stop on the floor, or its deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, KiHunterAcidSpitCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case KiHunterAcidSpitCue.Launch launch:
                    // The launch cue names the way the acid's animation spits it, which must be the way its Ki-Hunter spat it (#2338).
                    HorizontalDirection spat = projectile.StateOf<KiHunterAcidState>().Spat;
                    if (launch.Direction != spat)
                        throw new InvalidOperationException($"Acid spat {spat} played the animation that launches it {launch.Direction}.");
                    projectile.StartMotion(KiHunterAcidMotion.Launching);
                    break;
                case KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.StopMoving }:
                    projectile.HoldStill();
                    break;
                case KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.Delete }:
                    projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KiHunterAcidSpitCue)} {cue}.");
            }
        }

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, KiHunterAcidMotion motion)
        {
            switch (motion)
            {
                case KiHunterAcidMotion.Falling:
                    RunKiHunterAcidMovement(context.Projectile, context.Level);
                    break;
                case KiHunterAcidMotion.Launching:
                    StartKiHunterAcidMovement(context.Projectile);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KiHunterAcidMotion)} {motion}.");
            }
        }
    }

    /// <summary>Runs a Ki-Hunter body's main AI.</summary>
    internal void RunKiHunterBodyMainAi(RoomEnemyPlace body, IEnemyAiFrame frame) =>
        RunKiHunterMain(body, body.RequireSpeciesState<KiHunterBodyState>(), frame.Samus, frame.Level);

    /// <summary>Runs a Ki-Hunter's wings' main AI.</summary>
    internal void RunKiHunterWingsMainAi(RoomEnemyPlace wings, IEnemyAiFrame frame) =>
        RunKiHunterWingMain(wings, wings.RequireSpeciesState<KiHunterWingState>(), frame.Level);

    /// <summary>Advances a Ki-Hunter's wings' cue-free typed animation.</summary>
    internal static void AdvanceKiHunterWingsTypedAnimation(RoomEnemyPlace wings) =>
        EnemyAnimationPlayback.Advance(wings, KiHunterAnimations.Segments,
            static (KiHunterWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => throw new InvalidOperationException($"A Ki-Hunter wing animation raised {cue}."));

    /// <summary>Advances a Ki-Hunter body's typed animation, carrying out its cues.</summary>
    internal void AdvanceKiHunterBodyTypedAnimation(RoomEnemyPlace body) =>
        EnemyAnimationPlayback.Advance(body, KiHunterAnimations.Segments,
            static (KiHunterBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyKiHunterCue(body, cue) is { } target
                ? new AnimationContinuation<KiHunterSegment>(KiHunterAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    internal EnemyShotReaction KiHunterShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
    };

    /// <summary>
    /// Spawns an acid spit from its definition and initializer.
    /// The acid draws from the Ki-Hunter's spawned-object sheet in the colours the firing body shows.
    /// </summary>
    internal void SpawnKiHunterAcidSpit(RoomEnemyPlace body, HorizontalDirection direction)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.KiHunterAcidSpit,
            Acid,
            SpriteGraphics.SpawnedBy(body));
        projectile.StartAnimation(new KiHunterAcidSpitAnimation(direction == HorizontalDirection.Right
            ? KiHunterAcidSpitSegment.Right
            : KiHunterAcidSpitSegment.Left));
        projectile.State = new KiHunterAcidState(direction);

        projectile.Velocity = projectile.Velocity.WithX(direction == HorizontalDirection.Right
            ? KiHunterMotionDefinitions.AcidHorizontalSpeed
            : -KiHunterMotionDefinitions.AcidHorizontalSpeed);
        projectile.Position = projectile.Position.WithWrappedWholeX(body.Position.X.Whole +
            (direction == HorizontalDirection.Right ? KiHunterMotionDefinitions.AcidSpitReach : -KiHunterMotionDefinitions.AcidSpitReach));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
        projectile.Position = projectile.Position.WithWrappedWholeY(body.Position.Y.Whole - KiHunterMotionDefinitions.AcidSpitHeight).WithFractionX(0).WithFractionY(0);
    }

    /// <summary>
    /// Runs the acid's one-pass launch: its animation starts the launch after its opening frames, and the launch shifts the
    /// acid <see cref="KiHunterMotionDefinitions.AcidLaunchShift"/> pixels the way it was spat and sets it falling.
    /// </summary>
    internal static void StartKiHunterAcidMovement(EnemyProjectile projectile)
    {
        HorizontalDirection direction = projectile.StateOf<KiHunterAcidState>().Spat;
        projectile.StartMotion(KiHunterAcidMotion.Falling);
        projectile.Position = projectile.Position.WithWrappedWholeX(
            projectile.Position.X.Whole + (direction == HorizontalDirection.Right
                ? KiHunterMotionDefinitions.AcidLaunchShift
                : -KiHunterMotionDefinitions.AcidLaunchShift));
    }

    /// <summary>
    /// Moves the falling acid exactly: move/test Y, then X, then add capped 1/256-pixel gravity. A floor
    /// collision swaps to the splash animation; a wall collision only zeros X speed.
    /// </summary>
    internal static void RunKiHunterAcidMovement(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new KiHunterAcidSpitAnimation(KiHunterAcidSpitSegment.HitFloor));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
            return;
        }

        SignedEightEight accelerated = projectile.Velocity.Y + KiHunterMotionDefinitions.AcidGravity;
        projectile.Velocity = projectile.Velocity.WithY(accelerated >= KiHunterMotionDefinitions.AcidFastestFall
            ? KiHunterMotionDefinitions.AcidFastestFall
            : accelerated);
    }
}
