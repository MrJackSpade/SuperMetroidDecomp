using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Fx;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The Ki-Hunter species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KiHunterBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.KihunterGreen, EnemyDefinitionId.KihunterGreenWings, EnemyDefinitionId.KihunterRed, EnemyDefinitionId.KihunterRedWings, EnemyDefinitionId.KihunterYellow, EnemyDefinitionId.KihunterYellowWings];

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
    /// <summary>Resolves each body's physically adjacent wing record.</summary>
    private readonly EnemySlotTable _slotTable;

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
    /// the current direction, as the native callback returns that list; every other cue runs on.
    /// </summary>
    private KiHunterProgram? ApplyKiHunterCue(RoomEnemySlot slot, KiHunterCue cue)
    {
        switch (cue.Kind)
        {
            case KiHunterCueKind.ReturnToFlying:
                // The callback returns the body's flying animation while separately restarting the attached wings.
                return ReturnKiHunterToSteadyInstruction(slot);
            case KiHunterCueKind.StartHop:
                StartKiHunterGroundJumpFromInstruction(RequireKiHunterState(slot));
                return null;
            case KiHunterCueKind.StartWinglessThinking:
                StartKiHunterGroundWaitFromInstruction(RequireKiHunterState(slot));
                return null;
            case KiHunterCueKind.SpitLeft:
                SpawnKiHunterAcidFromInstruction(slot, movingRight: false);
                return null;
            case KiHunterCueKind.SpitRight:
                SpawnKiHunterAcidFromInstruction(slot, movingRight: true);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KiHunterCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Native grounded decision delay between jumps or spits.</summary>
    private const ushort KiHunterGroundWaitFrames = 12;
    /// <summary>Native cooldown after an acid-spit instruction.</summary>
    private const ushort KiHunterSpitWaitFrames = 24;
    /// <summary>Horizontal Samus range admitted by the grounded acid attack.</summary>
    private const ushort KiHunterGroundAttackDistance = 96;


    /// <summary>Last library-two sound queued by an acid-spit instruction this frame.</summary>
    public SoundEffectId? LastKiHunterSoundEffect { get; private set; }

    /// <summary>The detached wing's speed-index step: 384 per update while it slows, and per step while its reset is found.</summary>
    private const int DetachedSpeedStep = 384;
    /// <summary>The speed index a slowing detached wing lands on when the next step would go below zero.</summary>
    private const int SlowestDetachedSpeedIndex = 256;

    /// <summary>Recognizes the green, yellow and red body definition records.</summary>
    internal static bool IsKiHunterBodyDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed;

    /// <summary>Recognizes the three corresponding wing definition records.</summary>
    internal static bool IsKiHunterWingDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings;

    /// <summary>Discards per-slot typed state and prior sound publication on room load.</summary>
    internal void ResetKiHunterRoomState()
    {
        foreach (RoomEnemySlot cleared in _slotTable.Slots)
            cleared.SetSpeciesState<KiHunterEnemyState>(null);
        LastKiHunterSoundEffect = null;
    }

    /// <summary>Ports body initializer <c>$A8:F188</c>.</summary>
    internal static void InitializeKiHunter(RoomEnemySlot body)
    {
        KiHunterEnemyState state = CreateKiHunterState(body);

        // Every Ki-Hunter list is interpreted by the common enemy bytecode engine. The
        // disassembly calls this property "disable Samus collision", but the live engine
        // proves bit $2000 is the process-instructions flag.
        body.Properties = body.Properties.With(EnemyProperties.ProcessInstructions);
        state.HasLostWings = false;
        InstallKiHunterInstruction(body, state, KiHunterProgram.FlyingLeft);
        state.SwoopAngle = 0;
        state.Function = KiHunterEnemyFunction.FlyingPatrol;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(-1, 1));
        state.UpperPatrol = new(0, unchecked((ushort)(body.Position.Y.Whole - 16)));
        state.LowerPatrol = state.UpperPatrol + new Offset16<RoomPixels>(0, 32);
        state.SpawnPosition = body.Position.Whole;

        // Parameter bit 15 selects the permanently ground-bound form used by two red
        // Ki-Hunters. Its following wing record starts deleted and the body skips every
        // later detachment side effect.
        if ((body.Spawn.Population.Parameter1 & KiHunterMotionDefinitions.WinglessParameterBit) != 0)
        {
            state.HasLostWings = true;
            state.Function = KiHunterEnemyFunction.FallingAfterWingLoss;
            state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(1, 0));
        }
    }

    /// <summary>Ports wing initializer <c>$A8:F214</c>, including its preceding-slot aliases.</summary>
    internal void InitializeKiHunterWings(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        KiHunterEnemyState state = CreateKiHunterState(wings);

        wings.Properties = wings.Properties.With(EnemyProperties.ProcessInstructions);
        InstallKiHunterInstruction(wings, state, KiHunterProgram.WingsLeft);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
        state.Function = KiHunterEnemyFunction.FollowBody;
        wings.PaletteIndex = body.PaletteIndex;
        wings.VramTilesIndex = body.VramTilesIndex;
        if ((body.Spawn.Population.Parameter1 & KiHunterMotionDefinitions.WinglessParameterBit) != 0)
            wings.Properties = wings.Properties.With(EnemyProperties.Deleted);
    }

    /// <summary>Dispatches body main <c>$A8:F25C</c> and wing main <c>$A8:F262</c>.</summary>
    private void RunKiHunterMain(
        RoomEnemySlot actor,
        KiHunterEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case KiHunterEnemyFunction.FlyingPatrol:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterFlyingPatrol(actor, state, samus, level);
                return;
            case KiHunterEnemyFunction.Swooping:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoop(actor, state, level);
                return;
            case KiHunterEnemyFunction.RecoveringFromSwoop:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoopRecovery(actor, state, level);
                return;
            case KiHunterEnemyFunction.FallingAfterWingLoss:
                RequireKiHunterLevel(level);
                RunKiHunterFalling(actor, state, level);
                return;
            case KiHunterEnemyFunction.StartGroundJump:
                RequireKiHunterSamus(samus);
                StartKiHunterGroundJump(actor, state, samus);
                return;
            case KiHunterEnemyFunction.NoOp:
                return;
            case KiHunterEnemyFunction.GroundJump:
                RequireKiHunterLevel(level);
                RunKiHunterGroundJump(actor, state, level);
                return;
            case KiHunterEnemyFunction.GroundWait:
                RequireKiHunterSamus(samus);
                RunKiHunterGroundWait(actor, state, samus);
                return;
            case KiHunterEnemyFunction.SelectAcidSpit:
                RequireKiHunterSamus(samus);
                SelectKiHunterAcidSpit(actor, state, samus);
                return;
            case KiHunterEnemyFunction.FollowBody:
                FollowKiHunterBody(actor);
                return;
            case KiHunterEnemyFunction.DetachedWing:
                RequireKiHunterLevel(level);
                RunDetachedKiHunterWing(actor, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Ki-Hunter function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports flying patrol function <c>$A8:F268</c>.</summary>
    private void RunKiHunterFlyingPatrol(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y.RawValue))
        {
            state.Velocity = state.Velocity.WithWholeY(unchecked((short)-state.Velocity.Y.WholePixels));
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
                state.Velocity.X.RawValue))
        {
            state.Velocity = state.Velocity.WithWholeX(unchecked((short)-state.Velocity.X.WholePixels));
            SetKiHunterPairFacing(body, state.Velocity.X.WholePixels >= 0);
        }
        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - body.Position.X.Whole));
        ushort absoluteXDistance = EnemyMath.WrappedMagnitude(signedXDistance);
        ushort triggerDistance = KiHunterMotionDefinitions.SwoopTriggerDistance;
        if (unchecked((short)(absoluteXDistance - triggerDistance)) >= 0 ||
            unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole - 32)) < 0)
        {
            return;
        }

        state.AngularVelocity = SnesSignedSixteenSixteen.Zero;
        state.SwoopAnimationChanged = false;
        state.SwoopRadius = new Extent16<RoomPixels>(
            absoluteXDistance,
            unchecked((ushort)(samus.Position.Y.Whole - body.Position.Y.Whole)));
        state.SwoopTarget = new(samus.Position.X.Whole, 0);
        state.SwoopOrigin = new(0, body.Position.Y.Whole);
        state.Function = KiHunterEnemyFunction.Swooping;

        bool swoopLeft = unchecked((short)signedXDistance) < 0;
        if (swoopLeft)
        {
            state.MaximumAngularVelocity = -2;
            state.AngularAcceleration = SnesSignedSixteenSixteen.FromParts(
                unchecked((short)KiHunterMotionDefinitions.LeftSwoopAccelerationWhole),
                KiHunterMotionDefinitions.LeftSwoopAccelerationFraction);
            state.SwoopAngle = 255;
            state.AnimationChangeAngle = 240;
            state.Velocity = state.Velocity.WithWholeX(-1);
        }
        else
        {
            state.MaximumAngularVelocity = 2;
            state.AngularAcceleration = SnesSignedSixteenSixteen.FromParts(
                0, KiHunterMotionDefinitions.RightSwoopAccelerationFraction);
            state.SwoopAngle = SnesAngle.HalfTurn.TableIndex;
            state.AnimationChangeAngle = 144;
            state.Velocity = state.Velocity.WithWholeX(1);
        }
        SetKiHunterPairFacing(body, movingRight: !swoopLeft);
    }

    /// <summary>Ports the elliptic diving arc at <c>$A8:F3B8</c>.</summary>
    private void RunKiHunterSwoop(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        bool leftArc = state.AngularAcceleration.RawValue < 0;
        bool beforeAnimationChange = leftArc
            ? state.SwoopAngle >= state.AnimationChangeAngle
            : state.SwoopAngle < state.AnimationChangeAngle;
        if (!beforeAnimationChange && !state.SwoopAnimationChanged)
        {
            state.SwoopAnimationChanged = true;
            InstallKiHunterInstruction(
                body,
                RequireKiHunterState(body),
                leftArc
                    ? KiHunterProgram.SwoopLeft
                    : KiHunterProgram.SwoopRight);
        }

        // The cap replaces only the whole part; the fraction keeps accumulating beneath it.
        state.AngularVelocity += state.AngularAcceleration;
        bool pastCap = leftArc
            ? state.AngularVelocity.WholePixels < state.MaximumAngularVelocity
            : state.AngularVelocity.WholePixels >= state.MaximumAngularVelocity;
        if (pastCap)
            state.AngularVelocity = state.AngularVelocity.WithWholePixels(checked((short)state.MaximumAngularVelocity));

        state.SwoopAngle += state.AngularVelocity.WholePixels;
        if ((!leftArc && state.SwoopAngle >= SnesAngle.TableUnitsPerTurn) ||
            (leftArc && state.SwoopAngle < SnesAngle.HalfTurn.TableIndex))
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
            return;
        }

        SnesByteAngle tableAngle = SnesAngle.NormalizeTableIndex(state.SwoopAngle).ToByteAngle();
        ushort targetX = unchecked((ushort)(
            state.SwoopTarget.X +
            EnemyMath.ReadEightBitCosineProduct(tableAngle, state.SwoopRadius.X)));
        int horizontalDisplacement = SnesSixteenSixteen.FromWhole((short)(targetX - body.Position.X.Whole));
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(leftArc ? (short)1 : (short)-1, 0));
            StartKiHunterSwoopRecovery(state);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        ushort targetY = unchecked((ushort)(
            state.SwoopOrigin.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(tableAngle, state.SwoopRadius.Y)));
        int verticalDisplacement = SnesSixteenSixteen.FromWhole((short)(targetY - body.Position.Y.Whole));
        if (_terrain.MoveEnemyVertically(level, body, verticalDisplacement))
            StartKiHunterSwoopRecovery(state);
    }

    /// <summary>Switches a flying body from its dive arc to the collision escape leg.</summary>
    private static void StartKiHunterSwoopRecovery(KiHunterEnemyState state)
    {
        state.Function = KiHunterEnemyFunction.RecoveringFromSwoop;
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(-1, 0));
    }

    /// <summary>Ports the collision escape leg at <c>$A8:F4ED</c>.</summary>
    private void RunKiHunterSwoopRecovery(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X.RawValue))
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y.RawValue) ||
            unchecked((short)(body.Position.Y.Whole - state.SpawnPosition.Y)) < 0)
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
        }
    }

    /// <summary>Ports wing-loss fall/gravity function <c>$A8:F55A</c>.</summary>
    private void RunKiHunterFalling(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y.RawValue))
        {
            state.Function = KiHunterEnemyFunction.StartGroundJump;
            return;
        }

        state.Velocity = state.Velocity.WithY(state.Velocity.Y + KiHunterGravity);
    }

    /// <summary>Ports grounded jump setup <c>$A8:F58B</c>.</summary>
    private void StartKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        SamusState samus)
    {
        state.Function = KiHunterEnemyFunction.NoOp;
        int random = _room.Random.Read();
        bool jumpLeft = unchecked((short)(body.Position.X.Whole - samus.Position.X.Whole)) >= 0;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(
            jumpLeft ? (short)-2 : (short)2,
            unchecked((short)((random & 1) - 8))));
        InstallKiHunterInstruction(
            body,
            RequireKiHunterState(body),
            jumpLeft
                ? KiHunterProgram.JumpLeft
                : KiHunterProgram.JumpRight);
    }

    /// <summary>Ports airborne hop function <c>$A8:F5F0</c>.</summary>
    private void RunKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(
                level,
                body,
                state.Velocity.Y.RawValue))
        {
            if (state.Velocity.Y.WholePixels < 0)
            {
                state.Velocity = state.Velocity.WithWholeY(1);
            }
            else
            {
                state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(-4, 0));
                state.Function = KiHunterEnemyFunction.NoOp;
                state.WaitTimer = NativeCountdown.Start(KiHunterGroundWaitFrames);
                InstallKiHunterInstruction(
                    body,
                    RequireKiHunterState(body),
                    state.Velocity.X.WholePixels < 0
                        ? KiHunterProgram.LandLeft
                        : KiHunterProgram.LandRight);
            }
            return;
        }

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X.RawValue))
        {
            state.Velocity = state.Velocity.WithWholeX(unchecked((short)-state.Velocity.X.WholePixels));
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + KiHunterGravity);
    }

    /// <summary>Ports post-landing wait/decision function <c>$A8:F68B</c>.</summary>
    private static void RunKiHunterGroundWait(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        SamusState samus)
    {
        state.WaitTimer = state.WaitTimer.Decrement().Remaining;
        if (!state.WaitTimer.IsZero)
            return;

        ushort distance = EnemyMath.WrappedMagnitude(unchecked((ushort)(body.Position.X.Whole - samus.Position.X.Whole)));
        state.Function = distance < KiHunterGroundAttackDistance
            ? KiHunterEnemyFunction.SelectAcidSpit
            : KiHunterEnemyFunction.StartGroundJump;
    }

    /// <summary>Ports left/right spit-list selection <c>$A8:F6B3</c>.</summary>
    private static void SelectKiHunterAcidSpit(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        SamusState samus)
    {
        bool samusRight = unchecked((short)(body.Position.X.Whole - samus.Position.X.Whole)) < 0;
        InstallKiHunterInstruction(
            body,
            state,
            samusRight
                ? KiHunterProgram.SpitRight
                : KiHunterProgram.SpitLeft);
        state.Function = KiHunterEnemyFunction.NoOp;
    }

    /// <summary>Ports attached-wing follower <c>$A8:F6F3</c>.</summary>
    private void FollowKiHunterBody(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
    }

    /// <summary>Ports detached wing dispatcher <c>$A8:F7CF</c>.</summary>
    private void RunDetachedKiHunterWing(
        RoomEnemySlot wings,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        switch (state.WingFunction)
        {
            case KiHunterWingFunction.Orbit:
                RunDetachedKiHunterWingOrbit(wings, state);
                return;
            case KiHunterWingFunction.FallingCollisionArc:
                RunDetachedKiHunterWingCollisionArc(wings, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Detached Ki-Hunter wing function $A8:{(ushort)state.WingFunction:X4} is not translated.");
        }
    }

    /// <summary>Ports detached wing orbit <c>$A8:F7DB</c>, including its odd ROM word read.</summary>
    internal static void RunDetachedKiHunterWingOrbit(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.WingArcAngle = state.WingArcAngle.AddRaw(ReadKiHunterQuadraticAngleDelta(
            state.DetachedSpeedIndex,
            negativeHalf: true));
        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        SnesByteAngle byteAngle = state.WingArcAngle.ToByteAngle();
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + EnemyMath.CircleOffset(byteAngle, radius) - state.OrbitOffset);

        if (LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingCollisionArc(wings, state);
            return;
        }
        DecrementKiHunterDetachedSpeedIndex(state);
    }

    /// <summary>Ports collision-bearing detached arc <c>$A8:F8AD</c>.</summary>
    private void RunDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        state.WingArcAngle = state.WingArcAngle.AddRaw(ReadKiHunterQuadraticAngleDelta(
            state.DetachedSpeedIndex,
            negativeHalf: false));
        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        SnesByteAngle byteAngle = state.WingArcAngle.ToByteAngle();
        ushort desiredY = unchecked((ushort)(
            state.OrbitCenter.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(byteAngle, radius) -
            state.FallingArcOffset.Y));
        if (_terrain.MoveEnemyVertically(
                level,
                wings,
                SnesSixteenSixteen.FromWhole((short)(desiredY - wings.Position.Y.Whole))))
        {
            wings.Properties = wings.Properties.With(EnemyProperties.Deleted);
            wings.Position = wings.Position.WithWhole(state.SavedWingPosition);
            DecrementKiHunterDetachedSpeedIndex(state);
            return;
        }

        wings.Position = wings.Position.WithWholeX(unchecked((ushort)(
            state.OrbitCenter.X + EnemyMath.ReadEightBitCosineProduct(byteAngle, radius) - state.FallingArcOffset.X)));
        if (!LiesWithinMiddleHalfTurn(state.WingArcAngle))
        {
            BeginDetachedKiHunterWingOrbit(wings, state);
            return;
        }
        DecrementKiHunterDetachedSpeedIndex(state);
    }

    /// <summary>
    /// Runs the private tail after common shot/power-bomb AI at <c>$A8:F701</c>. A surviving
    /// body mirrors freeze/hurt timing into its wing until health reaches the wing record's
    /// parameter-one threshold; that exact threshold—not a host constant—starts detachment.
    /// </summary>
    private void ResolveKiHunterShotAfterCommon(RoomEnemySlot body)
    {
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState bodyState = RequireKiHunterState(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (body.Health == 0)
        {
            // The native routine assigns exactly $0200, stripping processing and any other
            // population flags rather than merely ORing the deleted bit.
            wings.Properties = new EnemyPropertyWord(EnemyProperties.Deleted);
            return;
        }

        if (body.Health <= wings.Spawn.Population.Parameter1)
        {
            if (!bodyState.HasLostWings)
            {
                bodyState.HasLostWings = true;
                bodyState.Function = KiHunterEnemyFunction.FallingAfterWingLoss;
                bodyState.Velocity = bodyState.Velocity.WithY(SnesSignedSixteenSixteen.FromParts(1, 0));
                if (wingState.Function != KiHunterEnemyFunction.DetachedWing)
                    DetachKiHunterWings(wings, wingState);
            }
            return;
        }

        wings.AiStates = body.AiStates;
        wings.FrozenTimer = body.FrozenTimer;
        wings.InvincibilityTimer = body.InvincibilityTimer;
        wings.FlashTimer = body.FlashTimer;
    }

    /// <summary>Initializes all fields written by <c>$A8:F701/$F851/$F87F/$F98D</c>.</summary>
    private static void DetachKiHunterWings(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.SavedWingPosition = wings.Position.Whole;
        CalculateKiHunterDetachedSpeedReset(state);

        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        state.OrbitOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.DetachedOrbitOffsetAngle, radius);
        state.FallingArcOffset = EnemyMath.CircleOffset(KiHunterMotionDefinitions.FallingArcOffsetAngle, radius);
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.Function = KiHunterEnemyFunction.DetachedWing;
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.OrbitCenter = new Position16<RoomPixels>(
            wings.Position.X.Whole,
            unchecked((ushort)(state.SavedWingPosition - state.LowerPatrol).Y));
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        InstallKiHunterInstruction(
            wings,
            state,
            KiHunterProgram.DetachedWings);
        wings.Frame = EnemyFrameId.Empty;
        wings.Properties = wings.Properties.With(EnemyProperties.ProcessOffScreen);
    }

    /// <summary>
    /// Walks the quadratic speed table until the detached wing's speed sum reaches its threshold. The reset cannot wrap:
    /// the table read rejects an index past its last record long before the reset would leave its sixteen-bit range.
    /// </summary>
    private static void CalculateKiHunterDetachedSpeedReset(KiHunterEnemyState state)
    {
        state.DetachedSpeedReset = 0;
        state.DetachedSpeedIndex = 0;
        do
        {
            state.DetachedSpeedReset += DetachedSpeedStep;
            int index = SnesEightEight.WholeOf(state.DetachedSpeedReset);
            state.AccumulateDetachedSpeed(EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + 1));
        }
        while (unchecked((short)(state.DetachedSpeedAccumulator - KiHunterMotionDefinitions.DetachedSpeedThreshold)) < 0);
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.DetachedOrbitStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.FallingCollisionArc;
        state.DetachedSpeedIndex = state.DetachedSpeedReset;
        state.WingArcAngle = KiHunterMotionDefinitions.FallingArcStartAngle;
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Reads the unaligned quadratic-table word used by one half of detached-wing orbit motion.</summary>
    internal static ushort ReadKiHunterQuadraticAngleDelta(int speedIndex, bool negativeHalf)
    {
        int index = SnesEightEight.WholeOf(speedIndex);
        // These deliberately unaligned reads are literal: F7DB reads at record +5 and
        // F8AD at record +1, combining adjacent bytes instead of consuming a normal 16.16
        // table component. Replacing this with ReadQuadraticEnemySpeed changes the orbit.
        return EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + (negativeHalf ? 5 : 1));
    }

    /// <summary>Slows a detached wing by one speed step, landing on the slowest index when the step would go below zero.</summary>
    private static void DecrementKiHunterDetachedSpeedIndex(KiHunterEnemyState state)
    {
        int next = state.DetachedSpeedIndex - DetachedSpeedStep;
        state.DetachedSpeedIndex = next < 0 ? SlowestDetachedSpeedIndex : next;
    }

    /// <summary>
    /// Whether a detached wing's arc angle lies from a quarter turn up to three quarters: the cartridge's sign test of
    /// the angle plus a quarter turn, which switches the wing from its orbit to its falling arc and back.
    /// </summary>
    private static bool LiesWithinMiddleHalfTurn(SnesAngle angle) =>
        angle.AddRaw(SnesAngle.QuarterTurn.RawValue).CompareTo(SnesAngle.HalfTurn) >= 0;

    /// <summary>Instruction <c>$A8:F526</c>: return to steady art and sync attached wings.</summary>
    private KiHunterProgram ReturnKiHunterToSteadyInstruction(RoomEnemySlot body)
    {
        KiHunterEnemyState state = RequireKiHunterState(body);
        bool movingRight = state.Velocity.X.WholePixels >= 0;
        KiHunterProgram wingProgram = movingRight
            ? KiHunterProgram.WingsRight
            : KiHunterProgram.WingsLeft;
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
            InstallKiHunterInstruction(wings, wingState, wingProgram);
        return movingRight
            ? KiHunterProgram.FlyingRight
            : KiHunterProgram.FlyingLeft;
    }

    /// <summary>Lets the body's animation instruction enter its wingless hop function.</summary>
    private static void StartKiHunterGroundJumpFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundJump;

    /// <summary>Returns the body to its wingless decision function after an animation instruction.</summary>
    private static void StartKiHunterGroundWaitFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundWait;

    /// <summary>Emits the acid projectile and sound, then starts the native 24-frame spit wait.</summary>
    private void SpawnKiHunterAcidFromInstruction(RoomEnemySlot body, bool movingRight)
    {
        LastKiHunterSoundEffect = SoundEffectLibrary2Sounds.KiHunterAcidSpit;
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.KiHunterAcidSpit, maximumQueued: 6);
        SpawnKiHunterAcidSpit(body, movingRight);
        RequireKiHunterState(body).WaitTimer = NativeCountdown.Start(KiHunterSpitWaitFrames);
    }

    /// <summary>Restarts matching body and still-attached wing lists for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemySlot body, bool movingRight)
    {
        InstallKiHunterInstruction(
            body,
            RequireKiHunterState(body),
            movingRight
                ? KiHunterProgram.FlyingRight
                : KiHunterProgram.FlyingLeft);
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
        {
            InstallKiHunterInstruction(
                wings,
                RequireKiHunterState(wings),
                movingRight
                    ? KiHunterProgram.WingsRight
                    : KiHunterProgram.WingsLeft);
        }
    }

    /// <summary>
    /// Returns the physical <c>enemy + $40</c> alias used by every body-to-wing access in
    /// bank $A8. The matching definition words are authoritative while the pair is being
    /// initialized. Afterward, however, <c>DetermineWhichEnemiesToProcess</c> clears only a
    /// deleted wing's definition word; all other words in its 64-byte record remain resident,
    /// and the cartridge deliberately continues reading or writing them through this alias.
    /// A permanently ground-bound red Ki-Hunter therefore has a live body followed by enemy
    /// definition zero on its second frame. Requiring the definition to remain $EB7F changed
    /// valid ROM behavior into a host exception whenever that body was shot.
    /// </summary>
    private RoomEnemySlot GetKiHunterWings(RoomEnemySlot body)
    {
        // NormalEnemyShotAi clears the complete body record before returning to Ki-Hunter's
        // private `$A8:F701` tail on a fatal hit. The cartridge nevertheless keeps X as the
        // dead body's physical slot and accesses `enemy + $40` to delete its wings. Once the
        // typed state has established this slot as a body, a cleared definition word is
        // therefore valid until the callback finishes; revalidating only the mutable header
        // converts the ordinary fatal-shot path into a host exception.
        bool initializedBody = body.SpeciesState<KiHunterEnemyState>() is { IsWing: false };
        if ((!IsKiHunterBodyDefinition(body.EnemyDefinitionPointer) && !initializedBody) ||
            body.SlotIndex >= RoomEnemySystem.MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                $"Enemy slot {body.SlotIndex} is not a Ki-Hunter body with a following slot.");
        }

        RoomEnemySlot wings = _slotTable.Slots[body.SlotIndex + 1];
        if (body.EnemyDefinitionPointer == 0 && initializedBody)
            return wings;

        EnemyDefinitionId expected = body.EnemyDefinitionPointer switch
        {
            EnemyDefinitionId.KihunterGreen => EnemyDefinitionId.KihunterGreenWings,
            EnemyDefinitionId.KihunterYellow => EnemyDefinitionId.KihunterYellowWings,
            EnemyDefinitionId.KihunterRed => EnemyDefinitionId.KihunterRedWings,
            _ => throw new InvalidDataException("Unknown Ki-Hunter body variant."),
        };
        // During wing initialization its typed state does not exist yet, so insist on the
        // authored matching definition. Once initialization has established the pair, retain
        // the native physical alias even if deletion cleared the definition or the slot was
        // subsequently reused. The latter is intentional: the 65C816 routine would alias and
        // mutate that replacement record too rather than performing a type check.
        if (wings.EnemyDefinitionPointer != expected &&
            wings.SpeciesState<KiHunterEnemyState>() is null)
        {
            throw new InvalidDataException(
                $"Ki-Hunter body slot {body.SlotIndex} is followed by ${(int)wings.EnemyDefinitionPointer:X4}, " +
                $"not expected wing ${(int)expected:X4}.");
        }
        return wings;
    }

    /// <summary>Resolves a wing's preceding body record and validates its color pairing.</summary>
    private RoomEnemySlot GetKiHunterBody(RoomEnemySlot wings)
    {
        if (!IsKiHunterWingDefinition(wings.EnemyDefinitionPointer) || wings.SlotIndex == 0)
            throw new InvalidDataException($"Enemy slot {wings.SlotIndex} is not a linked Ki-Hunter wing.");
        RoomEnemySlot body = _slotTable.Slots[wings.SlotIndex - 1];
        _ = GetKiHunterWings(body); // Performs the exact body/variant adjacency validation.
        return body;
    }

    /// <summary>Installs typed body or wing state at the physical enemy-slot index.</summary>
    private static KiHunterEnemyState CreateKiHunterState(RoomEnemySlot slot)
    {
        var state = new KiHunterEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return state;
    }

    /// <summary>Rejects access to a body or wing record before its initializer ran.</summary>
    private static KiHunterEnemyState RequireKiHunterState(RoomEnemySlot slot) =>
        slot.SpeciesState<KiHunterEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Ki-Hunter state.");

    /// <summary>Restarts a body or wing animation from its first step.</summary>
    private static void InstallKiHunterInstruction(RoomEnemySlot slot, KiHunterEnemyState state, KiHunterProgram program)
    {
        state.Program = AnimationProgramCursor<KiHunterProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>The <c>$0000:E000</c> 16.16 downward acceleration added to a falling or hopping body each frame.</summary>
    internal static SnesSignedSixteenSixteen KiHunterGravity => SnesSignedSixteenSixteen.FromParts(
        unchecked((short)KiHunterMotionDefinitions.GravityWhole),
        KiHunterMotionDefinitions.GravityFraction);

    /// <summary>Requires a live Samus, room collision data and a body definition before body-only movement AI.</summary>
    private static void RequireKiHunterWorld(
        RoomEnemySlot actor,
        [NotNull] SamusState? samus,
        [NotNull] RoomLevelData? level)
    {
        RequireKiHunterSamus(samus);
        RequireKiHunterLevel(level);
        if (!IsKiHunterBodyDefinition(actor.EnemyDefinitionPointer))
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
    internal void RegisterKiHunterBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => IsKiHunterBodyDefinition(c.Enemy.EnemyDefinitionPointer) &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.KiHunterShot)
        {
            AfterCommon = c => ResolveKiHunterShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects acid spit startup and gravity movement to bank-$86 pre-instructions.</summary>
    internal static void RegisterKiHunterProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunKiHunterAcidMovement(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KiHunterAcid_Moving);

        table.RegisterPreInstruction(context =>
        {
            StartKiHunterAcidMovement(context.Projectile, movingRight: false);
        }, EnemyProjectilePreInstruction.KiHunterAcid_Left);

        table.RegisterPreInstruction(context =>
        {
            StartKiHunterAcidMovement(context.Projectile, movingRight: true);
        }, EnemyProjectilePreInstruction.KiHunterAcid_Right);
    }

    /// <summary>Ports KihunterGreen's initialization AI.</summary>
    internal static void RunKihunterGreenInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKiHunter(slot);
    }

    /// <summary>Ports KihunterGreenWings's initialization AI.</summary>
    internal void RunKihunterGreenWingsInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKiHunterWings(slot);
    }

    /// <summary>Ports KiHunter's main AI.</summary>
    internal void RunKiHunterMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKiHunterMain(slot, RequireKiHunterState(slot), frame.Samus, frame.Level);
    }

    /// <summary>Advances KiHunter's typed animation.</summary>
    internal void AdvanceKiHunterTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireKiHunterState(slot);
        AnimationProgramCursor<KiHunterProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, KiHunterAnimations.Programs, ref cursor, KiHunterFrameDefinitions.ShownFrames.Of,
                (cue, _) => ApplyKiHunterCue(slot, cue));
        state.Program = cursor;
    }

    /// <summary>Runs wing-detachment logic after common power-bomb damage to a body.</summary>
    internal void RegisterKiHunterPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.KiHunterShot)
        {
            AfterCommon = (enemy, _) => ResolveKiHunterShotAfterCommon(enemy),
        });
    }

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    internal void RegisterKiHunterShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.KiHunterShot)
        {
            AfterCommon = (c, _) => ResolveKiHunterShotAfterCommon(c.Enemy),
        });
    }
    /// <summary>Bank-$86 acid mover installed by the startup callback after the projectile's idle frames.</summary>
    private const EnemyProjectilePreInstruction KiHunterAcidMovingPreInstruction =
        EnemyProjectilePreInstruction.KiHunterAcid_Moving;
    /// <summary>Positive 8.8 horizontal acid speed, negated for leftward shots.</summary>
    private const ushort KiHunterAcidHorizontalSpeed = 768;
    /// <summary>8.8 downward speed added to each active acid frame.</summary>
    private const ushort KiHunterAcidGravity = 16;
    /// <summary>Maximum 8.8 downward speed for a falling acid projectile.</summary>
    private const ushort KiHunterAcidTerminalYSpeed = 512;

    /// <summary>
    /// Ports <c>SpawnEprojWithGfx</c> plus definitions $86:CF18/$CF26 and initializers
    /// $86:CF90/$CFA6. The projectile retains the firing body's exact VRAM/palette words.
    /// </summary>
    internal void SpawnKiHunterAcidSpit(RoomEnemySlot body, bool movingRight)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = movingRight
            ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
            : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(body.VramTilesIndex | body.PaletteIndex)));

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(movingRight
            ? KiHunterAcidHorizontalSpeed
            : unchecked((ushort)-KiHunterAcidHorizontalSpeed)));
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + (movingRight ? 22 : -22))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 16))).WithFractionX(0).WithFractionY(0);
    }

    /// <summary>
    /// Ports one-shot pre-instructions $86:CFD5/$CFE6. The animation list installs one via
    /// common opcode $8161 after its opening map; the callback shifts nineteen pixels and
    /// then installs the $CFF8 mover.
    /// </summary>
    internal static void StartKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        bool movingRight)
    {
        projectile.PreInstruction = KiHunterAcidMovingPreInstruction;
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(
            projectile.Position.X.Whole + (movingRight ? 19 : -19))));
    }

    /// <summary>
    /// Ports $86:CFF8 exactly: move/test Y, then X, then add capped 8.8 gravity. A floor
    /// collision swaps to the cartridge splash list; a wall collision only zeros X speed.
    /// </summary>
    internal static void RunKiHunterAcidMovement(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.StartAnimation(new KiHunterAcidSpitAnimation(KiHunterAcidSpitProgram.HitFloor));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }

        ushort accelerated = unchecked((ushort)(projectile.Velocity.Y.RawValue + KiHunterAcidGravity));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((short)(accelerated - KiHunterAcidTerminalYSpeed)) >= 0
            ? KiHunterAcidTerminalYSpeed
            : accelerated));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        LastKiHunterSoundEffect = null;
        ResetKiHunterRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        LastKiHunterSoundEffect = null;
    }
}
