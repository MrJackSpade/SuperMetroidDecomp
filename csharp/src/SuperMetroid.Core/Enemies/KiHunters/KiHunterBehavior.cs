using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The Ki-Hunter species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KiHunterBehavior : IEnemySpecies, IEnemySettingsFactory<KiHunterSpawnSettings>, IEnemySettingsFactory<KiHunterWingsSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [
        EnemyDefinitionId.KihunterGreen, EnemyDefinitionId.KihunterGreenWings, EnemyDefinitionId.KihunterRed,
        EnemyDefinitionId.KihunterRedWings, EnemyDefinitionId.KihunterYellow, EnemyDefinitionId.KihunterYellowWings,
    ];

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry)
    {
        Ensure.NotNull(registry).Add<KiHunterSpawnSettings>(this);
        registry.Add<KiHunterWingsSpawnSettings>(this);
    }

    /// <summary>A body of any colour, winged or not as its placement names (#2146).</summary>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, KiHunterSpawnSettings settings) =>
        new KiHunterBodyEntity(this, occupant, place, common, settings);

    /// <summary>A body's wings, detaching at the health their placement names (#2146).</summary>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, KiHunterWingsSpawnSettings settings) =>
        new KiHunterWingsEntity(this, occupant, place, common, settings);

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
    /// <summary>The body each body placed handed the wings its room lists right after it.</summary>
    private readonly EnemyPartLinks<KiHunterWingsBody> _bodyOfWings = new();
    /// <summary>
    /// The settings of the wings each body's wings were placed with, which name the health at which the body loses them;
    /// a body reads them after its wings were deleted as set up, as a wingless Ki-Hunter's are.
    /// </summary>
    private readonly EnemyPartLinks<KiHunterWingsSpawnSettings> _wingSettingsOfBody = new();

    /// <summary>What each body's placement told it, recorded as it is placed for the wings its room lists after it (#2554).</summary>
    private readonly EnemyPartLinks<KiHunterSpawnSettings> _bodySettings = new();

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
    private KiHunterSegment? ApplyKiHunterCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, KiHunterBodyState state, KiHunterCue cue)
    {
        if (cue is KiHunterCue.Spit spit)
        {
            SpawnKiHunterAcidFromCue(occupant, place, state, direction: spit.Direction);
            return null;
        }
        KiHunterCueKind kind = ((KiHunterCue.Signal)cue).Kind;
        switch (kind)
        {
            case KiHunterCueKind.ReturnToFlying:
                // The cue returns the body's flying animation while separately restarting the attached wings.
                return ReturnKiHunterToSteadyAnimation(place, state);
            case KiHunterCueKind.StartHop:
                StartKiHunterGroundJumpFromCue(state);
                return null;
            case KiHunterCueKind.StartWinglessThinking:
                StartKiHunterGroundWaitFromCue(state);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KiHunterCueKind)} {kind}.");
        }
    }

    /// <summary>Initializes the Ki-Hunter body.</summary>
    internal KiHunterBodyState InitializeKiHunter(RoomEnemyOccupant occupant, RoomEnemyPlace body, KiHunterSpawnSettings settings)
    {
        RoomEnemyPlace wings = _places.ListedAfter(body, 1);
        _wingsOfBody.Place(body, wings);
        _bodyOfWings.Place(wings, new KiHunterWingsBody(body, occupant));
        _bodySettings.Place(body, settings);
        var state = new KiHunterBodyState(KiHunterAction.FlyingPatrol);

        // Every Ki-Hunter animation is played by the shared enemy animation, which this property admits.
        body.Processing = body.Processing.With(EnemyProcessing.Animates);
        StartKiHunterAnimation(body, state, KiHunterSegment.FlyingLeft);
        state.Heading = KiHunterMotionDefinitions.PatrolStartHeading;
        state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
        state.VerticalVelocity = KiHunterMotionDefinitions.PatrolClimbSpeed;
        state.UpperPatrolY = body.Position.Y.Whole - KiHunterMotionDefinitions.PatrolRise;
        state.LowerPatrolY = state.UpperPatrolY + KiHunterMotionDefinitions.PatrolDepth;
        state.SpawnPosition = body.Position.Whole;

        // A wingless Ki-Hunter is the permanently ground-bound form. The wing listed after it
        // starts deleted and the body skips every later detachment side effect.
        if (settings.Wings == KiHunterWings.Wingless)
        {
            state.Action = KiHunterAction.FallingAfterWingLoss;
            state.VerticalVelocity = KiHunterMotionDefinitions.WinglessFallStartVelocity;
        }
        return state;
    }

    /// <summary>Initializes the Ki-Hunter's wings, which the body listed right before them handed itself as it was placed.</summary>
    internal KiHunterWingState InitializeKiHunterWings(RoomEnemyOccupant occupant, RoomEnemyPlace wings, KiHunterWingsSpawnSettings settings)
    {
        _bodyOfWings.Place(wings, _places.ListedBefore(wings, 1));
        RoomEnemyPlace body = GetKiHunterBody(wings);
        _wingSettingsOfBody.Place(body, settings);
        var state = new KiHunterWingState(KiHunterWingAction.FollowBody);

        wings.Processing = wings.Processing.With(EnemyProcessing.Animates);
        StartKiHunterAnimation(wings, state, KiHunterSegment.WingsLeft);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
        if (_bodySettings.Of(partner.Occupant).Wings == KiHunterWings.Wingless)
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
        return state;
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
                StartKiHunterSwipeOncePastItsAngle(actor, state);
                RunKiHunterSwoop(actor, state, level);
                return;
            case KiHunterAction.Swiping:
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
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixelsReversed();
        }
        else if (body.Position.Y.Whole < state.UpperPatrolY)
        {
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixelsOf(KiHunterMotionDefinitions.PatrolClimbSpeed);
        }
        else if (body.Position.Y.Whole >= state.LowerPatrolY)
        {
            state.VerticalVelocity = state.VerticalVelocity.WithWholePixelsOf(-KiHunterMotionDefinitions.PatrolClimbSpeed);
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.HorizontalVelocity))
        {
            state.Heading = state.Heading.Reversed();
            SetKiHunterPairFacing(body, state, state.Heading);
        }
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);

        int signedXDistance = samus.Position.X.Whole - body.Position.X.Whole;
        int absoluteXDistance = Math.Abs(signedXDistance);
        int samusDepth = samus.Position.Y.Whole - body.Position.Y.Whole;
        if (absoluteXDistance >= KiHunterMotionDefinitions.SwoopTriggerDistance || samusDepth < KiHunterMotionDefinitions.SwoopLeastDepth)
            return;

        state.AngularVelocity = AngularSpeed.Zero;
        state.SwoopRadius = new Extent16<RoomPixels>(absoluteXDistance, samusDepth);
        state.SwoopTargetX = samus.Position.X.Whole;
        state.SwoopOriginY = body.Position.Y.Whole;
        state.Action = KiHunterAction.Swooping;

        // The swoop heads toward Samus and arcs that way (#2338).
        state.Heading = signedXDistance < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
        state.SwoopAngle = state.Heading == HorizontalDirection.Left
            ? KiHunterMotionDefinitions.LeftSwoopStartAngle
            : KiHunterMotionDefinitions.RightSwoopStartAngle;
        SetKiHunterPairFacing(body, state, state.Heading);
    }

    /// <summary>
    /// <paramref name="speed"/> turning a swoop heading <paramref name="heading"/> (#2338): toward increasing angles for a
    /// rightward swoop, decreasing ones for a leftward one.
    /// </summary>
    private static AngularSpeed SwoopTurning(AngularSpeed speed, HorizontalDirection heading) =>
        speed.Toward(heading);

    /// <summary>
    /// Starts the swipe (#2473) once the swoop's angle, as the update finds it, has passed the swipe angle: below it for a
    /// leftward swoop, at or past it for a rightward one. The rest of the dive swipes.
    /// </summary>
    private static void StartKiHunterSwipeOncePastItsAngle(RoomEnemyPlace body, KiHunterBodyState state)
    {
        bool leftArc = state.Heading == HorizontalDirection.Left;
        bool pastSwipeAngle = leftArc
            ? state.SwoopAngle < state.SwipeAngle
            : state.SwoopAngle >= state.SwipeAngle;
        if (!pastSwipeAngle)
            return;

        state.Action = KiHunterAction.Swiping;
        StartKiHunterAnimation(body, state, leftArc ? KiHunterSegment.SwoopLeft : KiHunterSegment.SwoopRight);
    }

    /// <summary>Runs the elliptic diving arc, before its swipe and while it swipes.</summary>
    private void RunKiHunterSwoop(
        RoomEnemyPlace body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        // The cap holds only the whole steps; the fraction keeps accumulating beneath it.
        state.AngularVelocity = (state.AngularVelocity + SwoopTurning(KiHunterMotionDefinitions.SwoopAcceleration, state.Heading))
            .CappedInWholeSteps(SwoopTurning(KiHunterMotionDefinitions.SwoopMaximumAngularVelocity, state.Heading));

        // The swoop runs through the second half of the turn: a rightward swoop ends as its angle comes round past the
        // full turn onto the first half, a leftward one as it falls below the half turn. At its swoop's fastest turn
        // neither can cross the other end of that half.
        state.SwoopAngle = state.SwoopAngle.TurnedByWholeStepsOf(state.AngularVelocity);
        if (state.SwoopAngle < RotationAngle.HalfTurn)
        {
            state.Action = KiHunterAction.FlyingPatrol;
            return;
        }

        TrigonometryAngle swoopAngle = state.SwoopAngle.ToTrigonometryAngle();
        int targetX = state.SwoopTargetX + EnemyMath.CircleCosinePixels(swoopAngle, state.SwoopRadius.X);
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(targetX - body.Position.X.Whole);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            // Striking a wall, the body flies back the other way (#2338).
            state.Heading = state.Heading.Reversed();
            state.HorizontalSpeed = KiHunterMotionDefinitions.PatrolSpeed;
            StartKiHunterSwoopRecovery(state);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        int targetY = state.SwoopOriginY + EnemyMath.CircleNegativeSinePixels(swoopAngle, state.SwoopRadius.Y);
        SignedSixteenSixteen verticalDisplacement = SignedSixteenSixteen.FromWholePixels(targetY - body.Position.Y.Whole);
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
            body.Position.Y.Whole < state.SpawnPosition.Y)
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
        state.Heading = body.Position.X.Whole >= samus.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;
        state.HorizontalSpeed = KiHunterMotionDefinitions.GroundJumpHorizontalSpeed;
        state.VerticalVelocity = SignedSixteenSixteen.FromWholePixels(
            KiHunterMotionDefinitions.GroundJumpSpeedTakenOff.Of(random) - KiHunterMotionDefinitions.GroundJumpUpwardSpeed);
        StartKiHunterAnimation(
            body,
            state,
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
            if (state.VerticalVelocity.IsNegative)
            {
                state.VerticalVelocity = state.VerticalVelocity.WithWholePixelsOf(KiHunterMotionDefinitions.HopReboundSpeed);
            }
            else
            {
                state.VerticalVelocity = KiHunterMotionDefinitions.LandedVerticalVelocity;
                state.Action = KiHunterAction.AnimatingOnGround;
                state.WaitTimer = UpdateCountdown.For(KiHunterMotionDefinitions.LandingWait);
                StartKiHunterAnimation(
                    body,
                    state,
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
        if (!state.WaitTimer.StepEnds())
            return;

        int distance = Math.Abs(body.Position.X.Whole - samus.Position.X.Whole);
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
        bool samusRight = body.Position.X.Whole < samus.Position.X.Whole;
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

    /// <summary>
    /// Turns the detached wings back along their orbit by the coarse speed of their place on the speed curve
    /// (<see cref="DetachedWingTurn"/>).
    /// </summary>
    private static void RunDetachedKiHunterWingOrbit(RoomEnemyPlace wings, KiHunterWingState state)
    {
        state.WingArcAngle = state.WingArcAngle.TurnedBy(DetachedWingTurn(
            state.DetachedSpeedCurvePlace,
            RotationDirection.Clockwise));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + EnemyMath.CircleOffset(trigonometryAngle, radius) - KiHunterMotionDefinitions.DetachedOrbitOffset);

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
        state.WingArcAngle = state.WingArcAngle.TurnedBy(DetachedWingTurn(
            state.DetachedSpeedCurvePlace,
            RotationDirection.Counterclockwise));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        int desiredY = state.OrbitCenter.Y + EnemyMath.CircleNegativeSinePixels(trigonometryAngle, radius) - KiHunterMotionDefinitions.FallingArcOffset.Y;
        if (_terrain.MoveEnemyVertically(
                level,
                wings,
                SignedSixteenSixteen.FromWholePixels(desiredY - wings.Position.Y.Whole)))
        {
            // The landed wings are deleted where they detached, as the game puts them back there (#2474).
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
            wings.Position = wings.Position.WithWhole(state.DetachedAt);
            SlowDetachedKiHunterWing(state);
            return;
        }

        wings.Position = wings.Position.WithWholeX(
            state.OrbitCenter.X + EnemyMath.CircleCosinePixels(trigonometryAngle, radius) - KiHunterMotionDefinitions.FallingArcOffset.X);
        if (!LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingOrbit(wings, state);
            return;
        }
        SlowDetachedKiHunterWing(state);
    }

    /// <summary>
    /// Runs the Ki-Hunter's own response after the common shot and Power Bomb reactions. A surviving body freezes and
    /// flashes its wings with it until its health falls to the health at which the wings detach, which starts their fall.
    /// </summary>
    private void ResolveKiHunterShotAfterCommon(RoomEnemyPlace body, KiHunterBodyState bodyState)
    {
        RoomEnemyPlace wings = GetKiHunterWings(body);
        KiHunterWingState? wingsInPlace = WingsInPlace(wings);
        if (body.Health == 0)
        {
            if (wingsInPlace is null)
                return;
            // Killing the body removes its wings outright: they stop updating and keep none of their placement's properties.
            wings.Contact = EnemyContact.None;
            wings.Processing = EnemyProcessing.None;
            wings.Presence = EnemyPresence.Deleted;
            return;
        }

        if (body.Health <= _wingSettingsOfBody.Of(body).DetachHealth)
        {
            if (bodyState.Flies)
            {
                // A body that still flies still has its wings in place, and loses them now.
                bodyState.Action = KiHunterAction.FallingAfterWingLoss;
                bodyState.VerticalVelocity = KiHunterMotionDefinitions.WinglessFallStartVelocity;
                if (wingsInPlace is { Action: not KiHunterWingAction.Detached } wingState)
                    DetachKiHunterWings(wings, wingState);
            }
            return;
        }

        // Wings still in place freeze, flash and recover with their body; wings that are gone have nothing to keep in step.
        if (wingsInPlace is null)
            return;
        wings.AiStates = body.AiStates;
        wings.Freeze.Restart(body.Freeze.UpdatesLeft);
        wings.Invincibility.Restart(body.Invincibility.UpdatesLeft);
        wings.HurtFlash.Restart(body.HurtFlash.UpdatesLeft);
    }

    /// <summary>Detaches the wings: their first orbit is centred where they are, starting where their climb starts.</summary>
    private static void DetachKiHunterWings(RoomEnemyPlace wings, KiHunterWingState state)
    {
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.Action = KiHunterWingAction.Detached;
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.DetachedAt = wings.Position.Whole;
        state.OrbitCenter = state.DetachedAt;
        state.DetachedSpeedCurvePlace = KiHunterMotionDefinitions.DetachedWingClimb.StartPlace;
        StartKiHunterAnimation(
            wings,
            state,
            KiHunterSegment.DetachedWings);
        wings.Frame = EnemyFrameId.PointAtOrigin;
        wings.Processing = wings.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemyPlace wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.DetachedSpeedCurvePlace = KiHunterMotionDefinitions.DetachedWingClimb.StartPlace;
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemyPlace wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.FallingCollisionArc;
        state.DetachedSpeedCurvePlace = KiHunterMotionDefinitions.DetachedWingClimb.StartPlace;
        state.WingArcAngle = KiHunterMotionDefinitions.FallingArcStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>
    /// The detached wing's turn along its orbit each update (#2439): one step for each pixel of its curve mark's coarse
    /// speed toward <paramref name="direction"/>; clockwise turns the orbit back.
    /// </summary>
    private static AngularSpeed DetachedWingTurn(QuadraticSpeedCurvePlace place, RotationDirection direction) =>
        place.Speed.TurnOfAStepAPixelToward(direction);

    /// <summary>Slows a detached wing by one speed step, landing on the slowest place on the curve when the step would go below its start.</summary>
    private static void SlowDetachedKiHunterWing(KiHunterWingState state)
    {
        QuadraticSpeedCurvePlace next = state.DetachedSpeedCurvePlace - KiHunterMotionDefinitions.DetachedWingClimb.StepPerPass;
        state.DetachedSpeedCurvePlace = next.IsNegative ? KiHunterMotionDefinitions.SlowestDetachedWingPlace : next;
    }

    /// <summary>
    /// Whether a detached wing's arc angle lies from a quarter turn up to three quarters, the half of the turn in which
    /// the wing leaves its orbit for its falling arc and the other half in which it returns.
    /// </summary>
    private static bool LiesWithinMiddleHalfTurn(RotationAngle angle) =>
        angle + RotationAngle.QuarterTurn >= RotationAngle.HalfTurn;

    /// <summary>Animation cue: return to steady art and sync attached wings.</summary>
    private KiHunterSegment ReturnKiHunterToSteadyAnimation(RoomEnemyPlace body, KiHunterBodyState state)
    {
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

    /// <summary>Emits the acid projectile and sound, then starts the spit wait (<see cref="KiHunterMotionDefinitions.SpitWait"/>).</summary>
    private void SpawnKiHunterAcidFromCue(RoomEnemyOccupant occupant, RoomEnemyPlace body, KiHunterBodyState state, HorizontalDirection direction)
    {
        _audio.QueueEnemySound(EnemySound.AcidSpit, priority: SoundRequestPriority.Normal);
        SpawnKiHunterAcidSpit(occupant, body, direction);
        state.WaitTimer = UpdateCountdown.For(KiHunterMotionDefinitions.SpitWait);
    }

    /// <summary>Restarts matching body and still-attached wing animations for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemyPlace body, KiHunterBodyState state, HorizontalDirection facing)
    {
        bool movingRight = facing == HorizontalDirection.Right;
        StartKiHunterAnimation(
            body,
            state,
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
    private static KiHunterWingState? WingsInPlace(RoomEnemyPlace wings) => wings.Entity is KiHunterWingsEntity entity ? entity.State : null;

    /// <summary>
    /// The wings the body found when it was placed, which its room lists right after it. They may already be gone, as a
    /// wingless Ki-Hunter's are from the start, and the body's behaviour asks <see cref="WingsInPlace"/> before it
    /// touches them.
    /// </summary>
    private RoomEnemyPlace GetKiHunterWings(RoomEnemyPlace body) => _wingsOfBody.Of(body);

    /// <summary>The place of the body that handed itself to the wings, which their room lists right before them.</summary>
    private RoomEnemyPlace GetKiHunterBody(RoomEnemyPlace wings) => _bodyOfWings.Of(wings).Place;

    /// <summary>Restarts a body animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemyPlace body, KiHunterBodyState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(body, KiHunterAnimations.StartingWith(segment));

    /// <summary>Restarts a wing animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemyPlace wings, KiHunterWingState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(wings, KiHunterAnimations.StartingWith(segment));

    /// <summary>Runs wing-detachment logic after common bomb damage to a Ki-Hunter body.</summary>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal EnemyBombReaction KiHunterBombReactionOf(KiHunterBodyState state) => new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy, state),
    };

    /// <summary>
    /// Ki-Hunter acid: it holds still in the Ki-Hunter's mouth until its animation launches it in its direction, then flies
    /// under gravity until it strikes the room, where its splash stops it.
    /// </summary>
    /// <param name="acid">What the acid keeps: the way its Ki-Hunter spat it.</param>
    internal sealed class AcidBehavior(KiHunterAcidState acid) : EnemyProjectileMotionBehavior<KiHunterAcidMotion>, IEnemyProjectileCues<KiHunterAcidSpitCue>
    {
        /// <summary>The way each acid was spat, given as its Ki-Hunter spits it.</summary>
        internal EnemyProjectileStates<KiHunterAcidState> Spits { get; } = new();

        /// <summary>Carries out one cue of the acid's animations: its launch either way, its stop on the floor, or its deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, KiHunterAcidSpitCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case KiHunterAcidSpitCue.Launch launch:
                    // The launch cue names the way the acid's animation spits it, which must be the way its Ki-Hunter spat it (#2338).
                    HorizontalDirection spat = acid.Spat;
                    if (launch.Direction != spat)
                        throw new InvalidOperationException($"Acid spat {spat} played the animation that launches it {launch.Direction}.");
                    projectile.StartMotion(this, KiHunterAcidMotion.Launching);
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
        internal override void Move(EnemyProjectileMotionContext context, KiHunterAcidMotion motion)
        {
            switch (motion)
            {
                case KiHunterAcidMotion.Falling:
                    RunKiHunterAcidMovement(this, context.Projectile, context.Level, this);
                    break;
                case KiHunterAcidMotion.Launching:
                    StartKiHunterAcidMovement(this, context.Projectile, acid, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KiHunterAcidMotion)} {motion}.");
            }
        }
    }

    /// <summary>Runs a Ki-Hunter body's main AI.</summary>
    internal void RunKiHunterBodyMainAi(RoomEnemyPlace body, KiHunterBodyState state, IEnemyAiFrame frame) =>
        RunKiHunterMain(body, state, frame.Samus, frame.Level);

    /// <summary>Runs a Ki-Hunter's wings' main AI.</summary>
    internal void RunKiHunterWingsMainAi(RoomEnemyPlace wings, KiHunterWingState state, IEnemyAiFrame frame) =>
        RunKiHunterWingMain(wings, state, frame.Level);

    /// <summary>Advances a Ki-Hunter's wings' cue-free typed animation.</summary>
    internal static void AdvanceKiHunterWingsTypedAnimation(RoomEnemyPlace wings, KiHunterWingState kiHunterWingState) =>
        EnemyAnimationPlayback.Advance(wings, kiHunterWingState, KiHunterAnimations.Segments,
            static (KiHunterWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => throw new InvalidOperationException($"A Ki-Hunter wing animation raised {cue}."));

    /// <summary>Advances a Ki-Hunter body's typed animation, carrying out its cues.</summary>
    internal void AdvanceKiHunterBodyTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace body, KiHunterBodyState kiHunterBodyState) =>
        EnemyAnimationPlayback.Advance(body, kiHunterBodyState, KiHunterAnimations.Segments,
            static (KiHunterBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyKiHunterCue(occupant, body, kiHunterBodyState, cue) is { } target
                ? new AnimationContinuation<KiHunterSegment>(KiHunterAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal EnemyShotReaction KiHunterShotReactionOf(KiHunterBodyState state) => new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy, state),
    };

    /// <summary>
    /// Spawns an acid spit from its definition and setup.
    /// The acid draws from the Ki-Hunter's spawned-object sheet in the colours the firing body shows.
    /// </summary>
    private void SpawnKiHunterAcidSpit(RoomEnemyOccupant occupant, RoomEnemyPlace body, HorizontalDirection direction)
    {
        var acid = new AcidBehavior(new KiHunterAcidState(direction));
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.KiHunterAcidSpit, acid, SpriteGraphics.SpawnedBy(occupant, body.Shading));
        if (projectile is null)
            return;

        projectile.StartAnimation(new KiHunterAcidSpitAnimation(direction == HorizontalDirection.Right
            ? KiHunterAcidSpitSegment.Right
            : KiHunterAcidSpitSegment.Left, acid));

        projectile.Velocity = projectile.Velocity.WithX(KiHunterMotionDefinitions.AcidHorizontalSpeed.Toward(direction));
        projectile.Position = projectile.Position.WithWholeX(body.Position.X.Whole +
            (KiHunterMotionDefinitions.AcidSpitReach.Toward(direction)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
        projectile.Position = projectile.Position.WithWholeY(body.Position.Y.Whole - KiHunterMotionDefinitions.AcidSpitHeight).WithoutFractions();
    }

    /// <summary>
    /// Runs the acid's one-pass launch: its animation starts the launch after its opening frames, and the launch shifts the
    /// acid <see cref="KiHunterMotionDefinitions.AcidLaunchShift"/> pixels the way it was spat and sets it falling.
    /// </summary>
    /// <param name="projectile">The acid.</param>
    /// <param name="acid">What the acid keeps: the way its Ki-Hunter spat it.</param>
    /// <param name="motions">The acid's behaviour, which owns its motions.</param>
    internal static void StartKiHunterAcidMovement(AcidBehavior owner, EnemyProjectile projectile, KiHunterAcidState acid,
        EnemyProjectileMotionBehavior<KiHunterAcidMotion> motions)
    {
        HorizontalDirection direction = acid.Spat;
        projectile.StartMotion(motions, KiHunterAcidMotion.Falling);
        projectile.Position = projectile.Position.WithWholeX(
            projectile.Position.X.Whole + (KiHunterMotionDefinitions.AcidLaunchShift.Toward(direction)));
    }

    /// <summary>
    /// Moves the falling acid vertically, then horizontally, then speeds its fall up to its cap. Meeting the floor
    /// splashes it; meeting a wall only stops it moving sideways.
    /// </summary>
    /// <param name="projectile">The acid.</param>
    /// <param name="level">The room's blocks, which the acid meets.</param>
    /// <param name="cues">The acid's behaviour, which owns its animations' cues.</param>
    private static void RunKiHunterAcidMovement(
        AcidBehavior owner,
        EnemyProjectile projectile,
        RoomLevelData level,
        IEnemyProjectileCues<KiHunterAcidSpitCue> cues)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new KiHunterAcidSpitAnimation(owner, KiHunterAcidSpitSegment.HitFloor, cues));
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
