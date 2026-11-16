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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. KiHunterPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KiHunterEntity(this, slot, common);

    /// <summary>Provides the active room state used by Ki-Hunter movement.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Checks the flying and wingless body's block collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the acid spit produced by the body animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the acid-spit sound under the enemy queue limit.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The room's enemy places, where a body and its wings find each other when they are placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The wings each body found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _wingsOfBody = new();
    /// <summary>The body each pair of wings found when it was placed, which its room lists right before them.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _bodyOfWings = new();
    /// <summary>
    /// The settings of the wings each body's wings were placed with, which name the health at which the body loses them;
    /// a body reads them after its wings were deleted as set up, as a wingless Ki-Hunter's are.
    /// </summary>
    private readonly EnemyPartLinks<KiHunterWingsSpawnSettings> _wingSettingsOfBody = new();

    /// <summary>Connects Ki-Hunter AI to room collision, projectile, audio and slot services.</summary>
    internal KiHunterBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemySlotTable slotTable)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _audio = audio;
        _slotTable = slotTable;
    }

    /// <summary>
    /// Carries out one cue of the Ki-Hunter body's animations. Returning to flight continues at the flying animation for
    /// the current direction, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private KiHunterSegment? ApplyKiHunterCue(RoomEnemySlot slot, KiHunterCue cue)
    {
        switch (cue.Kind)
        {
            case KiHunterCueKind.ReturnToFlying:
                // The cue returns the body's flying animation while separately restarting the attached wings.
                return ReturnKiHunterToSteadyAnimation(slot);
            case KiHunterCueKind.StartHop:
                StartKiHunterGroundJumpFromCue(slot.RequireSpeciesState<KiHunterBodyState>());
                return null;
            case KiHunterCueKind.StartWinglessThinking:
                StartKiHunterGroundWaitFromCue(slot.RequireSpeciesState<KiHunterBodyState>());
                return null;
            case KiHunterCueKind.SpitLeft:
                SpawnKiHunterAcidFromCue(slot, direction: HorizontalDirection.Left);
                return null;
            case KiHunterCueKind.SpitRight:
                SpawnKiHunterAcidFromCue(slot, direction: HorizontalDirection.Right);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KiHunterCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Grounded decision delay between jumps or spits.</summary>
    private const int KiHunterGroundWaitFrames = 12;
    /// <summary>Cooldown after an acid-spit cue.</summary>
    private const int KiHunterSpitWaitFrames = 24;
    /// <summary>Horizontal Samus range admitted by the grounded acid attack.</summary>
    private const int KiHunterGroundAttackDistance = 96;


    /// <summary>The detached wing's step along the speed curve: a mark and a half per update while it slows, and per step while its reset is found.</summary>
    private static readonly QuadraticSpeedCurvePlace DetachedSpeedStep = QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(384);
    /// <summary>The speed-curve place a slowing detached wing lands on when the next step would go below zero: mark one.</summary>
    private static readonly QuadraticSpeedCurvePlace SlowestDetachedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(1);

    /// <summary>Recognizes the green, yellow and red body definitions.</summary>
    internal static bool IsKiHunterBodyDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed;

    /// <summary>Recognizes the three corresponding wing definitions.</summary>
    internal static bool IsKiHunterWingDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings;

    /// <summary>Initializes the Ki-Hunter body.</summary>
    internal void InitializeKiHunter(RoomEnemySlot body)
    {
        _wingsOfBody.Place(body, _slotTable.ListedAfter(body, 1));
        var state = new KiHunterBodyState(KiHunterAction.FlyingPatrol);
        body.SetSpeciesState(state);

        // Every Ki-Hunter animation is played by the shared enemy animation, which this property admits.
        body.Processing = body.Processing.With(EnemyProcessing.Animates);
        state.HasLostWings = false;
        StartKiHunterAnimation(body, state, KiHunterSegment.FlyingLeft);
        state.SwoopAngle = RotationAngle.Zero;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(KiHunterMotionDefinitions.PatrolStartVelocity);
        state.UpperPatrolY = CoordinateAxis.Wrap(body.Position.Y.Whole - KiHunterMotionDefinitions.PatrolRise);
        state.LowerPatrolY = CoordinateAxis.Wrap(state.UpperPatrolY + KiHunterMotionDefinitions.PatrolDepth);
        state.SpawnPosition = body.Position.Whole;

        // A wingless Ki-Hunter is the permanently ground-bound form. The wing listed after it
        // starts deleted and the body skips every later detachment side effect.
        if (body.SpawnSettings<KiHunterSpawnSettings>().Wings == KiHunterWings.Wingless)
        {
            state.HasLostWings = true;
            state.Action = KiHunterAction.FallingAfterWingLoss;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(1, 0));
        }
    }

    /// <summary>Initializes the Ki-Hunter's wings, which find their body listed right before them.</summary>
    internal void InitializeKiHunterWings(RoomEnemySlot wings)
    {
        _bodyOfWings.Place(wings, _slotTable.ListedBefore(wings, 1));
        RoomEnemySlot body = GetKiHunterBody(wings);
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
        RoomEnemySlot actor,
        KiHunterBodyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case KiHunterAction.FlyingPatrol:
                RequireKiHunterBody(actor);
                RunKiHunterFlyingPatrol(actor, state, samus, level);
                return;
            case KiHunterAction.Swooping:
                RequireKiHunterBody(actor);
                RunKiHunterSwoop(actor, state, level);
                return;
            case KiHunterAction.RecoveringFromSwoop:
                RequireKiHunterBody(actor);
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
    private void RunKiHunterWingMain(RoomEnemySlot wings, KiHunterWingState state, RoomLevelData level)
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
        RoomEnemySlot body,
        KiHunterBodyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y))
        {
            state.Velocity = state.Velocity.WithWholeY(-state.Velocity.Y.WholePixels);
        }
        else if (CoordinateAxis.Precedes(body.Position.Y.Whole, state.UpperPatrolY))
        {
            state.Velocity = state.Velocity.WithWholeY(1);
        }
        else if (!CoordinateAxis.Precedes(body.Position.Y.Whole, state.LowerPatrolY))
        {
            state.Velocity = state.Velocity.WithWholeY(-1);
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X))
        {
            state.Velocity = state.Velocity.WithWholeX(-state.Velocity.X.WholePixels);
            SetKiHunterPairFacing(body, state.Velocity.X.WholePixels >= 0);
        }
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);

        int signedXDistance = CoordinateAxis.Difference(samus.Position.X.Whole, body.Position.X.Whole);
        int absoluteXDistance = Math.Abs(signedXDistance);
        int triggerDistance = KiHunterMotionDefinitions.SwoopTriggerDistance;
        if (CoordinateAxis.Difference(absoluteXDistance, triggerDistance) >= 0 ||
            CoordinateAxis.Difference(samus.Position.Y.Whole - body.Position.Y.Whole, 32) < 0)
        {
            return;
        }

        state.AngularVelocity = AngularSpeed.Zero;
        state.SwoopAnimationChanged = false;
        state.SwoopRadius = new Extent16<RoomPixels>(
            absoluteXDistance,
            CoordinateAxis.Wrap(samus.Position.Y.Whole - body.Position.Y.Whole));
        state.SwoopTargetX = samus.Position.X.Whole;
        state.SwoopOriginY = body.Position.Y.Whole;
        state.Action = KiHunterAction.Swooping;

        bool swoopLeft = signedXDistance < 0;
        if (swoopLeft)
        {
            state.MaximumAngularVelocity = KiHunterMotionDefinitions.LeftSwoopMaximumAngularVelocity;
            state.AngularAcceleration = KiHunterMotionDefinitions.LeftSwoopAcceleration;
            state.SwoopAngle = KiHunterMotionDefinitions.LeftSwoopStartAngle;
            state.AnimationChangeAngle = KiHunterMotionDefinitions.LeftSwoopAnimationChangeAngle;
            state.Velocity = state.Velocity.WithWholeX(-1);
        }
        else
        {
            state.MaximumAngularVelocity = KiHunterMotionDefinitions.RightSwoopMaximumAngularVelocity;
            state.AngularAcceleration = KiHunterMotionDefinitions.RightSwoopAcceleration;
            state.SwoopAngle = KiHunterMotionDefinitions.RightSwoopStartAngle;
            state.AnimationChangeAngle = KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle;
            state.Velocity = state.Velocity.WithWholeX(1);
        }
        SetKiHunterPairFacing(body, movingRight: !swoopLeft);
    }

    /// <summary>Runs the elliptic diving arc.</summary>
    private void RunKiHunterSwoop(
        RoomEnemySlot body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        bool leftArc = state.AngularAcceleration.IsNegative;
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
        state.AngularVelocity = (state.AngularVelocity + state.AngularAcceleration)
            .WithWholeStepsLimitedTo(state.MaximumAngularVelocity);

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
            state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(leftArc ? 1 : -1, 0));
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
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(-1, 0));
    }

    /// <summary>Runs the collision escape leg.</summary>
    private void RunKiHunterSwoopRecovery(
        RoomEnemySlot body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X))
        {
            state.Action = KiHunterAction.FlyingPatrol;
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y) ||
            CoordinateAxis.Difference(body.Position.Y.Whole, state.SpawnPosition.Y) < 0)
        {
            state.Action = KiHunterAction.FlyingPatrol;
        }
    }

    /// <summary>Falls under gravity after losing the wings.</summary>
    private void RunKiHunterFalling(
        RoomEnemySlot body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y))
        {
            state.Action = KiHunterAction.StartGroundJump;
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + KiHunterGravity);
    }

    /// <summary>Sets up a grounded jump.</summary>
    private void StartKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterBodyState state,
        SamusState samus)
    {
        state.Action = KiHunterAction.AnimatingOnGround;
        int random = _room.Random.Read();
        bool jumpLeft = CoordinateAxis.Difference(body.Position.X.Whole, samus.Position.X.Whole) >= 0;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(
            jumpLeft ? -KiHunterMotionDefinitions.GroundJumpHorizontalSpeed : KiHunterMotionDefinitions.GroundJumpHorizontalSpeed,
            KiHunterMotionDefinitions.ShallowerGroundJump.Of(random) - KiHunterMotionDefinitions.GroundJumpUpwardSpeed));
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterBodyState>(),
            jumpLeft
                ? KiHunterSegment.JumpLeft
                : KiHunterSegment.JumpRight);
    }

    /// <summary>Moves through an airborne hop.</summary>
    private void RunKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterBodyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y))
        {
            if (state.Velocity.Y.WholePixels < 0)
            {
                state.Velocity = state.Velocity.WithWholeY(1);
            }
            else
            {
                state.Velocity = state.Velocity.WithY(KiHunterMotionDefinitions.LandedVerticalVelocity);
                state.Action = KiHunterAction.AnimatingOnGround;
                state.WaitTimer = KiHunterGroundWaitFrames;
                StartKiHunterAnimation(
                    body,
                    body.RequireSpeciesState<KiHunterBodyState>(),
                    state.Velocity.X.WholePixels < 0
                        ? KiHunterSegment.LandLeft
                        : KiHunterSegment.LandRight);
            }
            return;
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X))
        {
            state.Velocity = state.Velocity.WithWholeX(-state.Velocity.X.WholePixels);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + KiHunterGravity);
    }

    /// <summary>Waits after landing and decides what to do next.</summary>
    private static void RunKiHunterGroundWait(
        RoomEnemySlot body,
        KiHunterBodyState state,
        SamusState samus)
    {
        if (--state.WaitTimer != 0)
            return;

        int distance = CoordinateAxis.Distance(body.Position.X.Whole, samus.Position.X.Whole);
        state.Action = distance < KiHunterGroundAttackDistance
            ? KiHunterAction.SelectAcidSpit
            : KiHunterAction.StartGroundJump;
    }

    /// <summary>Selects the left or right acid-spit animation.</summary>
    private static void SelectKiHunterAcidSpit(
        RoomEnemySlot body,
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
    private void FollowKiHunterBody(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
    }

    /// <summary>Runs the detached wings' behaviour.</summary>
    private void RunDetachedKiHunterWing(
        RoomEnemySlot wings,
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
    internal static void RunDetachedKiHunterWingOrbit(RoomEnemySlot wings, KiHunterWingState state)
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
        RoomEnemySlot wings,
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
    private void ResolveKiHunterShotAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot wings = GetKiHunterWings(body);
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
                bodyState.Velocity = bodyState.Velocity.WithY(SignedSixteenSixteen.FromParts(1, 0));
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
    private static void DetachKiHunterWings(RoomEnemySlot wings, KiHunterWingState state)
    {
        state.SavedWingPosition = wings.Position.Whole;
        CalculateKiHunterDetachedSpeedReset(state);

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

    /// <summary>
    /// Walks the quadratic speed curve until the detached wing's speed sum reaches its threshold. The reset cannot run
    /// past the curve: reading its speed rejects a place past the curve's last mark.
    /// </summary>
    private static void CalculateKiHunterDetachedSpeedReset(KiHunterWingState state)
    {
        state.DetachedSpeedReset = QuadraticSpeedCurvePlace.Start;
        state.DetachedSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        do
        {
            state.DetachedSpeedReset += DetachedSpeedStep;
            state.AccumulateDetachedSpeed(state.DetachedSpeedReset.Speed.CoarseToward(EnemySpeedSign.Positive));
        }
        while (WrappingCounter.IsBehind(state.DetachedSpeedAccumulator, KiHunterMotionDefinitions.DetachedSpeedThreshold));
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemySlot wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.DetachedSpeedCurvePlace = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterWingState state)
    {
        state.WingMovement = KiHunterWingMovement.FallingCollisionArc;
        state.DetachedSpeedCurvePlace = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.FallingArcStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>
    /// The detached wing's speed along its orbit each update: its curve mark's coarse speed in the direction it turns,
    /// in 256ths of a pixel, 0 through 65535. The orbit turns a 256th of a step for each 256th of a pixel of it, so its
    /// whole pixels turn whole steps; a speed toward decreasing coordinates is the speed's value continued around the
    /// turn, so it turns the orbit back.
    /// </summary>
    internal static int DetachedWingCoarseSpeed(QuadraticSpeedCurvePlace place, EnemySpeedSign sign)
    {
        // The orbit steps by the coarse speed; stepping by the full speed changes the orbit.
        return place.Speed.CoarseToward(sign);
    }

    /// <summary>Slows a detached wing by one speed step, landing on the slowest place on the curve when the step would go below its start.</summary>
    private static void SlowDetachedKiHunterWing(KiHunterWingState state)
    {
        QuadraticSpeedCurvePlace next = state.DetachedSpeedCurvePlace - DetachedSpeedStep;
        state.DetachedSpeedCurvePlace = next.IsNegative ? SlowestDetachedCurvePlace : next;
    }

    /// <summary>
    /// Whether a detached wing's arc angle lies from a quarter turn up to three quarters: the game's check that
    /// the angle plus a quarter turn lies in the second half-turn, which switches the wing from its orbit to its falling arc and back.
    /// </summary>
    private static bool LiesWithinMiddleHalfTurn(RotationAngle angle) =>
        angle + RotationAngle.QuarterTurn >= RotationAngle.HalfTurn;

    /// <summary>Animation cue: return to steady art and sync attached wings.</summary>
    private KiHunterSegment ReturnKiHunterToSteadyAnimation(RoomEnemySlot body)
    {
        KiHunterBodyState state = body.RequireSpeciesState<KiHunterBodyState>();
        bool movingRight = state.Velocity.X.WholePixels >= 0;
        KiHunterSegment wingSegment = movingRight
            ? KiHunterSegment.WingsRight
            : KiHunterSegment.WingsLeft;
        RoomEnemySlot wings = GetKiHunterWings(body);
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
    private void SpawnKiHunterAcidFromCue(RoomEnemySlot body, HorizontalDirection direction)
    {
        _audio.QueueEnemySound(EnemySound.AcidSpit, priority: SoundRequestPriority.Normal);
        SpawnKiHunterAcidSpit(body, direction == HorizontalDirection.Right);
        body.RequireSpeciesState<KiHunterBodyState>().WaitTimer = KiHunterSpitWaitFrames;
    }

    /// <summary>Restarts matching body and still-attached wing animations for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemySlot body, bool movingRight)
    {
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterBodyState>(),
            movingRight
                ? KiHunterSegment.FlyingRight
                : KiHunterSegment.FlyingLeft);
        RoomEnemySlot wings = GetKiHunterWings(body);
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
    private static KiHunterWingState? WingsInPlace(RoomEnemySlot wings) => wings.SpeciesState<KiHunterWingState>();

    /// <summary>
    /// The wings the body found when it was placed. While the pair is being set up the wings must be the body's colour;
    /// afterwards the wings may already be gone, as a wingless Ki-Hunter's are from the start, and the body's behaviour
    /// asks <see cref="WingsInPlace"/> before it touches them.
    /// </summary>
    private RoomEnemySlot GetKiHunterWings(RoomEnemySlot body)
    {
        // A fatal shot clears the body before its shot reaction deletes the wings, so a body that was placed stays one
        // until the reaction finishes.
        bool initializedBody = _wingsOfBody.IsPlaced(body);
        if (!IsKiHunterBodyDefinition(body.DefinitionId) && !initializedBody)
            throw new InvalidDataException($"Enemy slot {body.SlotIndex} is not a Ki-Hunter body.");

        RoomEnemySlot wings = _wingsOfBody.Of(body);
        if (body.IsFree && initializedBody)
            return wings;

        EnemyDefinitionId expected = KiHunterPartDefinitions.Parts[body.DefinitionId] switch
        {
            KiHunterPart.KihunterGreen => EnemyDefinitionId.KihunterGreenWings,
            KiHunterPart.KihunterYellow => EnemyDefinitionId.KihunterYellowWings,
            KiHunterPart.KihunterRed => EnemyDefinitionId.KihunterRedWings,
            KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterRedWings or KiHunterPart.KihunterYellowWings => throw new InvalidDataException("Unknown Ki-Hunter body variant."),
            _ => throw new InvalidDataException("Unknown Ki-Hunter body variant."),
        };
        // While the wings are being set up they must be the body's colour. Once the pair is established, wings that are
        // gone may leave no identity behind; anything else in their place is refused.
        bool established = _bodyOfWings.IsPlaced(wings);
        if (wings.DefinitionId != expected &&
            !(established && wings.IsFree))
        {
            throw new InvalidDataException(
                $"Ki-Hunter body slot {body.SlotIndex} is followed by {wings.DefinitionId}, " +
                $"not expected wing {expected} or its deleted place.");
        }
        return wings;
    }

    /// <summary>Resolves the body the wings found when they were placed and validates its colour pairing.</summary>
    private RoomEnemySlot GetKiHunterBody(RoomEnemySlot wings)
    {
        if (!IsKiHunterWingDefinition(wings.DefinitionId))
            throw new InvalidDataException($"Enemy slot {wings.SlotIndex} is not a linked Ki-Hunter wing.");
        RoomEnemySlot body = _bodyOfWings.Of(wings);
        if (!ReferenceEquals(GetKiHunterWings(body), wings)) // Performs the exact body/variant pairing validation.
            throw new InvalidDataException($"Ki-Hunter wings {wings.SlotIndex} are not the wings of body {body.SlotIndex}.");
        return body;
    }

    /// <summary>Restarts a body animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemySlot body, KiHunterBodyState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(body, KiHunterAnimations.StartingWith(segment));

    /// <summary>Restarts a wing animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemySlot wings, KiHunterWingState state, KiHunterSegment segment) =>
        state.Playback = EnemyAnimationPlayback.Start(wings, KiHunterAnimations.StartingWith(segment));

    /// <summary>The 0.875-pixel downward acceleration, in 1/65536 pixels, added to a falling or hopping body each frame.</summary>
    internal static SignedSixteenSixteen KiHunterGravity => KiHunterMotionDefinitions.Gravity;

    /// <summary>Requires a body definition before body-only movement AI.</summary>
    private static void RequireKiHunterBody(RoomEnemySlot actor)
    {
        if (!IsKiHunterBodyDefinition(actor.DefinitionId))
            throw new InvalidDataException("A Ki-Hunter wing entered body-only movement AI.");
    }

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
                case KiHunterAcidSpitCue.LaunchLeft:
                    projectile.StartMotion(KiHunterAcidMotion.LaunchingLeft);
                    break;
                case KiHunterAcidSpitCue.LaunchRight:
                    projectile.StartMotion(KiHunterAcidMotion.LaunchingRight);
                    break;
                case KiHunterAcidSpitCue.StopMoving:
                    projectile.HoldStill();
                    break;
                case KiHunterAcidSpitCue.Delete:
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
                case KiHunterAcidMotion.LaunchingLeft:
                    StartKiHunterAcidMovement(context.Projectile, direction: HorizontalDirection.Left);
                    break;
                case KiHunterAcidMotion.LaunchingRight:
                    StartKiHunterAcidMovement(context.Projectile, direction: HorizontalDirection.Right);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KiHunterAcidMotion)} {motion}.");
            }
        }
    }

    /// <summary>Runs the Ki-Hunter's main AI.</summary>
    internal void RunKiHunterMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        if (slot.SpeciesState<KiHunterWingState>() is { } wings)
            RunKiHunterWingMain(slot, wings, frame.Level);
        else
            RunKiHunterMain(slot, slot.RequireSpeciesState<KiHunterBodyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances the Ki-Hunter body's or wings' typed animation.</summary>
    internal void AdvanceKiHunterTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (slot.SpeciesState<KiHunterWingState>() is not null)
        {
            EnemyAnimationPlayback.Advance(slot, KiHunterAnimations.Segments,
                static (KiHunterWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
                (state, cue) => throw new InvalidOperationException($"A Ki-Hunter wing animation raised {cue}."));
            return;
        }
        EnemyAnimationPlayback.Advance(slot, KiHunterAnimations.Segments,
            static (KiHunterBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyKiHunterCue(slot, cue) is { } target
                ? new AnimationContinuation<KiHunterSegment>(KiHunterAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
    }

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    internal EnemyShotReaction KiHunterShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
    };
    /// <summary>Horizontal acid speed, three pixels per update, negated for leftward shots.</summary>
    private static readonly SignedEightEight KiHunterAcidHorizontalSpeed = SignedEightEight.FromParts(3, 0);
    /// <summary>Downward speed added to each active acid frame, a sixteenth of a pixel per update.</summary>
    private static readonly SignedEightEight KiHunterAcidGravity = SignedEightEight.FromParts(0, 16);
    /// <summary>Maximum downward speed for a falling acid projectile, two pixels per update.</summary>
    private static readonly SignedEightEight KiHunterAcidTerminalYSpeed = SignedEightEight.FromParts(2, 0);

    /// <summary>
    /// Spawns an acid spit from its definition and initializer.
    /// The acid draws from the Ki-Hunter's spawned-object sheet in the colours the firing body shows.
    /// </summary>
    internal void SpawnKiHunterAcidSpit(RoomEnemySlot body, bool movingRight)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = movingRight
            ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
            : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            Acid,
            SpriteGraphics.SpawnedBy(body));
        projectile.StartAnimation(new KiHunterAcidSpitAnimation(movingRight
            ? KiHunterAcidSpitSegment.Right
            : KiHunterAcidSpitSegment.Left));

        projectile.Velocity = projectile.Velocity.WithX(movingRight
            ? KiHunterAcidHorizontalSpeed
            : -KiHunterAcidHorizontalSpeed);
        projectile.Position = projectile.Position.WithWrappedWholeX(body.Position.X.Whole +
            (movingRight ? KiHunterMotionDefinitions.AcidSpitReach : -KiHunterMotionDefinitions.AcidSpitReach));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
        projectile.Position = projectile.Position.WithWrappedWholeY(body.Position.Y.Whole - KiHunterMotionDefinitions.AcidSpitHeight).WithFractionX(0).WithFractionY(0);
    }

    /// <summary>
    /// Runs the acid's one-pass launch: its animation starts the launch after its opening frames, and the launch shifts the
    /// acid <see cref="KiHunterMotionDefinitions.AcidLaunchShift"/> pixels in its direction and sets it falling.
    /// </summary>
    internal static void StartKiHunterAcidMovement(
        EnemyProjectile projectile,
        HorizontalDirection direction)
    {
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

        SignedEightEight accelerated = projectile.Velocity.Y + KiHunterAcidGravity;
        projectile.Velocity = projectile.Velocity.WithY(accelerated >= KiHunterAcidTerminalYSpeed
            ? KiHunterAcidTerminalYSpeed
            : accelerated);
    }
}
