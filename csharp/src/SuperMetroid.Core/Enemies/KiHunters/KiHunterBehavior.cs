using SuperMetroid.Core.Animation;
using System.Diagnostics.CodeAnalysis;
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
                StartKiHunterGroundJumpFromCue(slot.RequireSpeciesState<KiHunterEnemyState>());
                return null;
            case KiHunterCueKind.StartWinglessThinking:
                StartKiHunterGroundWaitFromCue(slot.RequireSpeciesState<KiHunterEnemyState>());
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


    /// <summary>The detached wing's speed-index step: a row and a half per update while it slows, and per step while its reset is found.</summary>
    private static readonly EnemyQuadraticSpeedIndex DetachedSpeedStep = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(384);
    /// <summary>The speed index a slowing detached wing lands on when the next step would go below zero: row one.</summary>
    private static readonly EnemyQuadraticSpeedIndex SlowestDetachedSpeedIndex = EnemyQuadraticSpeedIndex.FromRows(1);

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
        KiHunterEnemyState state = CreateKiHunterState(body);

        // Every Ki-Hunter animation is played by the shared enemy animation. The
        // game's name calls this property "disable Samus collision", but the live engine
        // proves it is the animating flag.
        body.Processing = body.Processing.With(EnemyProcessing.Animates);
        state.HasLostWings = false;
        StartKiHunterAnimation(body, state, KiHunterSegment.FlyingLeft);
        state.SwoopAngle = 0;
        state.Action = KiHunterAction.FlyingPatrol;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(-1, 1));
        state.UpperPatrol = new(0, CoordinateAxis.Wrap(body.Position.Y.Whole - 16));
        state.LowerPatrol = state.UpperPatrol + new Offset16<RoomPixels>(0, 32);
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
        KiHunterEnemyState state = CreateKiHunterState(wings);

        wings.Processing = wings.Processing.With(EnemyProcessing.Animates);
        StartKiHunterAnimation(wings, state, KiHunterSegment.WingsLeft);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
        state.Action = KiHunterAction.FollowBody;
        if (body.SpawnSettings<KiHunterSpawnSettings>().Wings == KiHunterWings.Wingless)
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
    }

    /// <summary>Dispatches the body's and wings' main behaviours.</summary>
    private void RunKiHunterMain(
        RoomEnemySlot actor,
        KiHunterEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Action)
        {
            case KiHunterAction.FlyingPatrol:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterFlyingPatrol(actor, state, samus, level);
                return;
            case KiHunterAction.Swooping:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoop(actor, state, level);
                return;
            case KiHunterAction.RecoveringFromSwoop:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoopRecovery(actor, state, level);
                return;
            case KiHunterAction.FallingAfterWingLoss:
                RequireKiHunterLevel(level);
                RunKiHunterFalling(actor, state, level);
                return;
            case KiHunterAction.StartGroundJump:
                RequireKiHunterSamus(samus);
                StartKiHunterGroundJump(actor, state, samus);
                return;
            case KiHunterAction.NoOp:
                return;
            case KiHunterAction.GroundJump:
                RequireKiHunterLevel(level);
                RunKiHunterGroundJump(actor, state, level);
                return;
            case KiHunterAction.GroundWait:
                RequireKiHunterSamus(samus);
                RunKiHunterGroundWait(actor, state, samus);
                return;
            case KiHunterAction.SelectAcidSpit:
                RequireKiHunterSamus(samus);
                SelectKiHunterAcidSpit(actor, state, samus);
                return;
            case KiHunterAction.FollowBody:
                FollowKiHunterBody(actor);
                return;
            case KiHunterAction.DetachedWing:
                RequireKiHunterLevel(level);
                RunDetachedKiHunterWing(actor, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Ki-Hunter has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Flies the patrol.</summary>
    private void RunKiHunterFlyingPatrol(
        RoomEnemySlot body,
        KiHunterEnemyState state,
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
        else if ((body.Position.Whole - state.UpperPatrol).Y < 0)
        {
            state.Velocity = state.Velocity.WithWholeY(1);
        }
        else if ((body.Position.Whole - state.LowerPatrol).Y >= 0)
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

        state.AngularVelocity = SignedSixteenSixteen.Zero;
        state.SwoopAnimationChanged = false;
        state.SwoopRadius = new Extent16<RoomPixels>(
            absoluteXDistance,
            CoordinateAxis.Wrap(samus.Position.Y.Whole - body.Position.Y.Whole));
        state.SwoopTarget = new(samus.Position.X.Whole, 0);
        state.SwoopOrigin = new(0, body.Position.Y.Whole);
        state.Action = KiHunterAction.Swooping;

        bool swoopLeft = signedXDistance < 0;
        if (swoopLeft)
        {
            state.MaximumAngularVelocity = -2;
            state.AngularAcceleration = KiHunterMotionDefinitions.LeftSwoopAcceleration;
            // A leftward swoop starts one step short of the whole turn, as the game's does, and steps down to the half turn.
            state.SwoopAngle = RotationAngle.StepsPerTurn - 1;
            state.AnimationChangeAngle = 240;
            state.Velocity = state.Velocity.WithWholeX(-1);
        }
        else
        {
            state.MaximumAngularVelocity = 2;
            state.AngularAcceleration = KiHunterMotionDefinitions.RightSwoopAcceleration;
            state.SwoopAngle = RotationAngle.StepsPerTurn / 2;
            state.AnimationChangeAngle = 144;
            state.Velocity = state.Velocity.WithWholeX(1);
        }
        SetKiHunterPairFacing(body, movingRight: !swoopLeft);
    }

    /// <summary>Runs the elliptic diving arc.</summary>
    private void RunKiHunterSwoop(
        RoomEnemySlot body,
        KiHunterEnemyState state,
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
                body.RequireSpeciesState<KiHunterEnemyState>(),
                leftArc
                    ? KiHunterSegment.SwoopLeft
                    : KiHunterSegment.SwoopRight);
        }

        // The cap replaces only the whole part; the fraction keeps accumulating beneath it.
        state.AngularVelocity += state.AngularAcceleration;
        bool pastCap = leftArc
            ? state.AngularVelocity.WholePixels < state.MaximumAngularVelocity
            : state.AngularVelocity.WholePixels >= state.MaximumAngularVelocity;
        if (pastCap)
            state.AngularVelocity = state.AngularVelocity.WithWholePixels(state.MaximumAngularVelocity);

        state.SwoopAngle += state.AngularVelocity.WholePixels;
        if ((!leftArc && state.SwoopAngle >= RotationAngle.StepsPerTurn) ||
            (leftArc && state.SwoopAngle < RotationAngle.StepsPerTurn / 2))
        {
            state.Action = KiHunterAction.FlyingPatrol;
            return;
        }

        TrigonometryAngle tableAngle = RotationAngle.WrappedSteps(state.SwoopAngle).ToTrigonometryAngle();
        int targetX = CoordinateAxis.Wrap(
            state.SwoopTarget.X +
            EnemyMath.ReadEightBitCosineProduct(tableAngle, state.SwoopRadius.X));
        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(targetX, body.Position.X.Whole));
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(leftArc ? 1 : -1, 0));
            StartKiHunterSwoopRecovery(state);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        int targetY = CoordinateAxis.Wrap(
            state.SwoopOrigin.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(tableAngle, state.SwoopRadius.Y));
        SignedSixteenSixteen verticalDisplacement = SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(targetY, body.Position.Y.Whole));
        if (_terrain.MoveEnemyVertically(level, body, verticalDisplacement))
            StartKiHunterSwoopRecovery(state);
    }

    /// <summary>Switches a flying body from its dive arc to the collision escape leg.</summary>
    private static void StartKiHunterSwoopRecovery(KiHunterEnemyState state)
    {
        state.Action = KiHunterAction.RecoveringFromSwoop;
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(-1, 0));
    }

    /// <summary>Runs the collision escape leg.</summary>
    private void RunKiHunterSwoopRecovery(
        RoomEnemySlot body,
        KiHunterEnemyState state,
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
        KiHunterEnemyState state,
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
        KiHunterEnemyState state,
        SamusState samus)
    {
        state.Action = KiHunterAction.NoOp;
        int random = _room.Random.Read();
        bool jumpLeft = CoordinateAxis.Difference(body.Position.X.Whole, samus.Position.X.Whole) >= 0;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(
            jumpLeft ? -2 : 2,
            KiHunterMotionDefinitions.ShallowerGroundJump.Of(random) - 8));
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterEnemyState>(),
            jumpLeft
                ? KiHunterSegment.JumpLeft
                : KiHunterSegment.JumpRight);
    }

    /// <summary>Moves through an airborne hop.</summary>
    private void RunKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterEnemyState state,
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
                state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(-4, 0));
                state.Action = KiHunterAction.NoOp;
                state.WaitTimer = KiHunterGroundWaitFrames;
                StartKiHunterAnimation(
                    body,
                    body.RequireSpeciesState<KiHunterEnemyState>(),
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
        KiHunterEnemyState state,
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
        KiHunterEnemyState state,
        SamusState samus)
    {
        bool samusRight = CoordinateAxis.Difference(body.Position.X.Whole, samus.Position.X.Whole) < 0;
        StartKiHunterAnimation(
            body,
            state,
            samusRight
                ? KiHunterSegment.SpitRight
                : KiHunterSegment.SpitLeft);
        state.Action = KiHunterAction.NoOp;
    }

    /// <summary>Moves attached wings with the body.</summary>
    private void FollowKiHunterBody(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
    }

    /// <summary>Dispatches the detached wings' behaviour.</summary>
    private void RunDetachedKiHunterWing(
        RoomEnemySlot wings,
        KiHunterEnemyState state,
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
    internal static void RunDetachedKiHunterWingOrbit(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.WingArcAngle = state.WingArcAngle.TurnedByFractionsOfStep(DetachedWingCoarseSpeed(
            state.DetachedSpeedIndex,
            sign: EnemySpeedSign.Negative));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + EnemyMath.CircleOffset(trigonometryAngle, radius) - state.OrbitOffset);

        if (LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingCollisionArc(wings, state);
            return;
        }
        DecrementKiHunterDetachedSpeedIndex(state);
    }

    /// <summary>Advances the detached wings' collision-bearing arc.</summary>
    private void RunDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        state.WingArcAngle = state.WingArcAngle.TurnedByFractionsOfStep(DetachedWingCoarseSpeed(
            state.DetachedSpeedIndex,
            sign: EnemySpeedSign.Positive));
        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        TrigonometryAngle trigonometryAngle = state.WingArcAngle.ToTrigonometryAngle();
        int desiredY = CoordinateAxis.Wrap(
            state.OrbitCenter.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(trigonometryAngle, radius) -
            state.FallingArcOffset.Y);
        if (_terrain.MoveEnemyVertically(
                level,
                wings,
                SignedSixteenSixteen.FromWholePixels(CoordinateAxis.Difference(desiredY, wings.Position.Y.Whole))))
        {
            wings.Presence = wings.Presence.With(EnemyPresence.Deleted);
            wings.Position = wings.Position.WithWhole(state.SavedWingPosition);
            DecrementKiHunterDetachedSpeedIndex(state);
            return;
        }

        wings.Position = wings.Position.WithWrappedWholeX(
            state.OrbitCenter.X + EnemyMath.ReadEightBitCosineProduct(trigonometryAngle, radius) - state.FallingArcOffset.X);
        if (!LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingOrbit(wings, state);
            return;
        }
        DecrementKiHunterDetachedSpeedIndex(state);
    }

    /// <summary>
    /// Runs the private tail after common shot/power-bomb AI. A surviving
    /// body mirrors freeze/hurt timing into its wing until health reaches the wing's
    /// detach health; that exact threshold—not an invented constant—starts detachment.
    /// </summary>
    private void ResolveKiHunterShotAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot wings = GetKiHunterWings(body);
        if (body.Health == 0)
        {
            // The game assigns exactly the deletion property, stripping processing and any other
            // population flags rather than merely adding the deleted flag.
            wings.Contact = EnemyContact.None;
            wings.Processing = EnemyProcessing.None;
            wings.Presence = EnemyPresence.Deleted;
            return;
        }

        KiHunterEnemyState bodyState = body.RequireSpeciesState<KiHunterEnemyState>();
        if (body.Health <= _wingSettingsOfBody.Of(body).DetachHealth)
        {
            if (!bodyState.HasLostWings)
            {
                // A body that has not lost its wings still has them in place.
                KiHunterEnemyState wingState = wings.RequireSpeciesState<KiHunterEnemyState>();
                bodyState.HasLostWings = true;
                bodyState.Action = KiHunterAction.FallingAfterWingLoss;
                bodyState.Velocity = bodyState.Velocity.WithY(SignedSixteenSixteen.FromParts(1, 0));
                if (wingState.Action != KiHunterAction.DetachedWing)
                    DetachKiHunterWings(wings, wingState);
            }
            return;
        }

        wings.AiStates = body.AiStates;
        wings.FrozenTimer = body.FrozenTimer;
        wings.InvincibilityTimer = body.InvincibilityTimer;
        wings.FlashTimer = body.FlashTimer;
    }

    /// <summary>Initializes all fields the wing detachment writes.</summary>
    private static void DetachKiHunterWings(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.SavedWingPosition = wings.Position.Whole;
        CalculateKiHunterDetachedSpeedReset(state);

        int radius = KiHunterMotionDefinitions.DetachedWingRadius;
        state.OrbitOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.DetachedOrbitOffsetAngle, radius);
        state.FallingArcOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.FallingArcOffsetAngle, radius);
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.Action = KiHunterAction.DetachedWing;
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.OrbitCenter = new Position16<RoomPixels>(
            wings.Position.X.Whole,
            CoordinateAxis.Wrap((state.SavedWingPosition - state.LowerPatrol).Y));
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        StartKiHunterAnimation(
            wings,
            state,
            KiHunterSegment.DetachedWings);
        wings.Frame = EnemyFrameId.Empty;
        wings.Processing = wings.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>
    /// Walks the quadratic speed table until the detached wing's speed sum reaches its threshold. The reset cannot run
    /// past the table: the table read rejects an index past its last row.
    /// </summary>
    private static void CalculateKiHunterDetachedSpeedReset(KiHunterEnemyState state)
    {
        state.DetachedSpeedReset = EnemyQuadraticSpeedIndex.Start;
        state.DetachedSpeedIndex = EnemyQuadraticSpeedIndex.Start;
        do
        {
            state.DetachedSpeedReset += DetachedSpeedStep;
            state.AccumulateDetachedSpeed(state.DetachedSpeedReset.Row.CoarseToward(EnemySpeedSign.Positive));
        }
        while (WrappingCounter.IsBehind(state.DetachedSpeedAccumulator, KiHunterMotionDefinitions.DetachedSpeedThreshold));
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingMovement = KiHunterWingMovement.Orbit;
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingMovement = KiHunterWingMovement.FallingCollisionArc;
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.FallingArcStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>
    /// The detached wing's speed along its orbit each update: its quadratic row's coarse speed in the direction it turns,
    /// in 256ths of a pixel, 0 through 65535. The orbit turns a 256th of a step for each 256th of a pixel of it, so its
    /// whole pixels turn whole steps; a speed toward decreasing coordinates is the speed's value continued around the
    /// turn, so it turns the orbit back.
    /// </summary>
    internal static int DetachedWingCoarseSpeed(EnemyQuadraticSpeedIndex speedIndex, EnemySpeedSign sign)
    {
        // The orbit steps by the coarse speed; stepping by the full speed changes the orbit.
        return speedIndex.Row.CoarseToward(sign);
    }

    /// <summary>Slows a detached wing by one speed step, landing on the slowest index when the step would go below zero.</summary>
    private static void DecrementKiHunterDetachedSpeedIndex(KiHunterEnemyState state)
    {
        EnemyQuadraticSpeedIndex next = state.DetachedSpeedIndex - DetachedSpeedStep;
        state.DetachedSpeedIndex = next.IsNegative ? SlowestDetachedSpeedIndex : next;
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
        KiHunterEnemyState state = body.RequireSpeciesState<KiHunterEnemyState>();
        bool movingRight = state.Velocity.X.WholePixels >= 0;
        KiHunterSegment wingSegment = movingRight
            ? KiHunterSegment.WingsRight
            : KiHunterSegment.WingsLeft;
        RoomEnemySlot wings = GetKiHunterWings(body);
        if (WingsInPlace(wings) is { Action: KiHunterAction.FollowBody } wingState)
            StartKiHunterAnimation(wings, wingState, wingSegment);
        return movingRight
            ? KiHunterSegment.FlyingRight
            : KiHunterSegment.FlyingLeft;
    }

    /// <summary>Lets the body's animation cue enter its wingless hop action.</summary>
    private static void StartKiHunterGroundJumpFromCue(KiHunterEnemyState state) =>
        state.Action = KiHunterAction.GroundJump;

    /// <summary>Returns the body to its wingless decision action after an animation cue.</summary>
    private static void StartKiHunterGroundWaitFromCue(KiHunterEnemyState state) =>
        state.Action = KiHunterAction.GroundWait;

    /// <summary>Emits the acid projectile and sound, then starts the 24-frame spit wait.</summary>
    private void SpawnKiHunterAcidFromCue(RoomEnemySlot body, HorizontalDirection direction)
    {
        _audio.QueueEnemySound(EnemySound.AcidSpit, priority: SoundRequestPriority.Normal);
        SpawnKiHunterAcidSpit(body, direction == HorizontalDirection.Right);
        body.RequireSpeciesState<KiHunterEnemyState>().WaitTimer = KiHunterSpitWaitFrames;
    }

    /// <summary>Restarts matching body and still-attached wing animations for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemySlot body, bool movingRight)
    {
        StartKiHunterAnimation(
            body,
            body.RequireSpeciesState<KiHunterEnemyState>(),
            movingRight
                ? KiHunterSegment.FlyingRight
                : KiHunterSegment.FlyingLeft);
        RoomEnemySlot wings = GetKiHunterWings(body);
        if (WingsInPlace(wings) is { Action: KiHunterAction.FollowBody } wingState)
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
    private static KiHunterEnemyState? WingsInPlace(RoomEnemySlot wings) => wings.SpeciesState<KiHunterEnemyState>();

    /// <summary>
    /// Returns the enemy listed after the body, which every body-to-wing access in
    /// the Ki-Hunter's behaviour reads. The matching definitions are authoritative while the pair is being
    /// initialized. Afterward, however, the processing pass clears only a deleted wing's definition;
    /// the rest of its state stays, and the game deliberately continues reading or writing it through that place.
    /// A permanently ground-bound red Ki-Hunter therefore has a live body followed by enemy
    /// definition zero on its second frame. Requiring the definition to remain the wing's changed
    /// valid game behavior into a thrown error whenever that body was shot.
    /// </summary>
    private RoomEnemySlot GetKiHunterWings(RoomEnemySlot body)
    {
        // The normal enemy shot AI clears the whole body before returning to Ki-Hunter's
        // private shot tail on a fatal hit. The game nevertheless keeps the
        // dead body's place in the room's listing and reaches the enemy listed after it to delete its wings. Once the
        // typed state has established this slot as a body, a cleared definition is
        // therefore valid until the shot reaction finishes; revalidating only the mutable definition
        // converts the ordinary fatal-shot path into a thrown error. A place the room set up as a body stays one.
        bool initializedBody = _wingsOfBody.IsPlaced(body);
        if (!IsKiHunterBodyDefinition(body.DefinitionId) && !initializedBody)
            throw new InvalidDataException($"Enemy slot {body.SlotIndex} is not a Ki-Hunter body.");

        RoomEnemySlot wings = _wingsOfBody.Of(body);
        if (body.DefinitionId == EnemyDefinitionId.None && initializedBody)
            return wings;

        EnemyDefinitionId expected = KiHunterPartDefinitions.Parts[body.DefinitionId] switch
        {
            KiHunterPart.KihunterGreen => EnemyDefinitionId.KihunterGreenWings,
            KiHunterPart.KihunterYellow => EnemyDefinitionId.KihunterYellowWings,
            KiHunterPart.KihunterRed => EnemyDefinitionId.KihunterRedWings,
            KiHunterPart.KihunterGreenWings or KiHunterPart.KihunterRedWings or KiHunterPart.KihunterYellowWings => throw new InvalidDataException("Unknown Ki-Hunter body variant."),
            _ => throw new InvalidDataException("Unknown Ki-Hunter body variant."),
        };
        // During wing initialization its typed state does not exist yet, so insist on the
        // authored matching definition. Once initialization has established the pair, the slot
        // stays the wing's after deletion clears its identity. Every retail Ki-Hunter room places
        // the wing right after its body and nothing in those rooms places another enemy in a freed
        // slot, so a slot that now holds a different enemy is refused rather than written as the wing.
        bool established = _bodyOfWings.IsPlaced(wings);
        if (wings.DefinitionId != expected &&
            !(established && wings.DefinitionId == EnemyDefinitionId.None))
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

    /// <summary>Installs typed body or wing state on the listed enemy.</summary>
    private static KiHunterEnemyState CreateKiHunterState(RoomEnemySlot slot)
    {
        var state = new KiHunterEnemyState();
        slot.SetSpeciesState(state);
        return state;
    }

    /// <summary>Restarts a body or wing animation from its first step.</summary>
    private static void StartKiHunterAnimation(RoomEnemySlot slot, KiHunterEnemyState state, KiHunterSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, KiHunterAnimations.StartingWith(segment));
    }

    /// <summary>The 0.875-pixel downward acceleration, in 1/65536 pixels, added to a falling or hopping body each frame.</summary>
    internal static SignedSixteenSixteen KiHunterGravity => KiHunterMotionDefinitions.Gravity;

    /// <summary>Requires a live Samus, room collision data and a body definition before body-only movement AI.</summary>
    private static void RequireKiHunterWorld(
        RoomEnemySlot actor,
        [NotNull] SamusState? samus,
        [NotNull] RoomLevelData? level)
    {
        RequireKiHunterSamus(samus);
        RequireKiHunterLevel(level);
        if (!IsKiHunterBodyDefinition(actor.DefinitionId))
            throw new InvalidDataException("A Ki-Hunter wing entered body-only movement AI.");
    }

    /// <summary>Rejects a missing Samus actor for target-dependent Ki-Hunter movement.</summary>
    private static void RequireKiHunterSamus([NotNull] SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ki-Hunter AI requires the active Samus actor.");
    }

    /// <summary>Rejects missing room collision data for flight and ground movement.</summary>
    private static void RequireKiHunterLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Ki-Hunter movement requires room collision data.");
    }

    /// <summary>Runs wing-detachment logic after common bomb damage to a Ki-Hunter body.</summary>
    internal EnemyBombReaction KiHunterBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
    };

    /// <summary>Ki-Hunter acid, spat either way (#1760).</summary>
    internal static readonly EnemyProjectileBehavior Acid = new AcidBehavior();

    /// <summary>Ki-Hunter acid: its animation launches it in its direction, and it then flies under gravity until it strikes the room.</summary>
    private sealed class AcidBehavior : EnemyProjectileBehavior
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileFrameBehaviorContext context)
        {
            EnemyProjectileFrameBehavior motion = context.Projectile.FrameBehavior;
            if (motion == EnemyProjectileFrameBehavior.KiHunterAcidFlight)
            {
                RunKiHunterAcidMovement(context.Projectile, context.Level);
            }
            else if (motion == EnemyProjectileFrameBehavior.KiHunterAcidLaunchLeft)
            {
                StartKiHunterAcidMovement(context.Projectile, direction: HorizontalDirection.Left);
            }
            else if (motion == EnemyProjectileFrameBehavior.KiHunterAcidLaunchRight)
            {
                StartKiHunterAcidMovement(context.Projectile, direction: HorizontalDirection.Right);
            }
            else
            {
                base.Move(context);
            }
        }
    }

    /// <summary>Runs the Ki-Hunter's main AI.</summary>
    internal void RunKiHunterMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKiHunterMain(slot, slot.RequireSpeciesState<KiHunterEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances the Ki-Hunter's typed animation.</summary>
    internal void AdvanceKiHunterTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, KiHunterAnimations.Segments,
            static (KiHunterEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyKiHunterCue(slot, cue) is { } target
                ? new AnimationContinuation<KiHunterSegment>(KiHunterAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    internal EnemyShotReaction KiHunterShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
    };
    /// <summary>Acid mover installed by the startup frame behaviour after the projectile's idle frames.</summary>
    private const EnemyProjectileFrameBehavior KiHunterAcidMovingFrameBehavior =
        EnemyProjectileFrameBehavior.KiHunterAcidFlight;
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
        projectile.Position = projectile.Position.WithWrappedWholeX(body.Position.X.Whole + (movingRight ? 22 : -22));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
        projectile.Position = projectile.Position.WithWrappedWholeY(body.Position.Y.Whole - 16).WithFractionX(0).WithFractionY(0);
    }

    /// <summary>
    /// Runs the one-shot spit frame behaviours. The animation installs one through the
    /// common frame-behaviour cue after its opening map; the frame behaviour shifts nineteen pixels and
    /// then installs the falling mover.
    /// </summary>
    internal static void StartKiHunterAcidMovement(
        EnemyProjectile projectile,
        HorizontalDirection direction)
    {
        projectile.FrameBehavior = KiHunterAcidMovingFrameBehavior;
        projectile.Position = projectile.Position.WithWrappedWholeX(
            projectile.Position.X.Whole + (direction == HorizontalDirection.Right ? 19 : -19));
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
