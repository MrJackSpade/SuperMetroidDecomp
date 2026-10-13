using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Ki-Hunter species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KiHunterBehavior : IEnemySpecies
{
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

    /// <summary>Executes the Ki-Hunter body's private animation instructions.</summary>
    private bool TryProcessKiHunterInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (!IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((KiHunterInstruction)word))
            return false;

        switch ((KiHunterInstruction)word)
        {
            case KiHunterInstruction.SetIdlingInstListsFacingForwards:
                // The callback returns a direct body-list pointer while separately
                // restarting the following attached wing list.
                cursor = ReturnKiHunterToSteadyInstruction(slot);
                return true;
            case KiHunterInstruction.SetFunctionToHop:
                StartKiHunterGroundJumpFromInstruction(RequireKiHunterState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case KiHunterInstruction.SetFunctionToWinglessThinking:
                StartKiHunterGroundWaitFromInstruction(RequireKiHunterState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case KiHunterInstruction.FireAcidSpitLeft:
                SpawnKiHunterAcidFromInstruction(slot, movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case KiHunterInstruction.FireAcidSpitRight:
                SpawnKiHunterAcidFromInstruction(slot, movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Ki-Hunter does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Bank-$A8 empty drawing map installed when wings detach.</summary>
    private const ushort EmptyA8Spritemap = 0x804d;
    /// <summary>Native grounded decision delay between jumps or spits.</summary>
    private const ushort KiHunterGroundWaitFrames = 12;
    /// <summary>Native cooldown after an acid-spit instruction.</summary>
    private const ushort KiHunterSpitWaitFrames = 24;
    /// <summary>Horizontal Samus range admitted by the grounded acid attack.</summary>
    private const ushort KiHunterGroundAttackDistance = 96;

    /// <summary>Per-physical-slot body and wing state for this room's Ki-Hunters.</summary>
    internal readonly KiHunterEnemyState?[] _kiHunterStates =
        new KiHunterEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two sound queued by an acid-spit instruction this frame.</summary>
    public ushort? LastKiHunterSoundEffect { get; private set; }

    /// <summary>Recognizes the green, yellow and red body definition records.</summary>
    internal static bool IsKiHunterBodyDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed;

    /// <summary>Recognizes the three corresponding wing definition records.</summary>
    internal static bool IsKiHunterWingDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings;

    /// <summary>Admits either physical component of any Ki-Hunter color variant.</summary>
    internal static bool IsKiHunterDefinition(EnemyDefinitionId definition) =>
        IsKiHunterBodyDefinition(definition) || IsKiHunterWingDefinition(definition);

    /// <summary>Discards per-slot typed state and prior sound publication on room load.</summary>
    internal void ResetKiHunterRoomState()
    {
        Array.Clear(_kiHunterStates);
        LastKiHunterSoundEffect = null;
    }

    /// <summary>Ports body initializer <c>$A8:F188</c>.</summary>
    internal void InitializeKiHunter(RoomEnemySlot body)
    {
        KiHunterEnemyState state = CreateKiHunterState(body);

        // Every Ki-Hunter list is interpreted by the common enemy bytecode engine. The
        // disassembly calls this property "disable Samus collision", but the live engine
        // proves bit $2000 is the process-instructions flag.
        body.Properties = body.Properties.With(EnemyProperties.ProcessInstructions);
        state.HasLostWings = false;
        InstallKiHunterInstruction(body, KiHunterInstructionProgramDefinitions.FlyingLeft);
        state.Angle = 0;
        state.Function = KiHunterEnemyFunction.FlyingPatrol;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(-1, 1));
        state.UpperPatrolY = unchecked((ushort)(body.Position.Y.Whole - 16));
        state.LowerPatrolY = unchecked((ushort)(state.UpperPatrolY + 32));
        state.SpawnPosition = body.Position.Whole;

        // Parameter bit 15 selects the permanently ground-bound form used by two red
        // Ki-Hunters. Its following wing record starts deleted and the body skips every
        // later detachment side effect.
        if ((body.Parameter1 & 0x8000) != 0)
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
        InstallKiHunterInstruction(wings, KiHunterInstructionProgramDefinitions.WingsLeft);
        wings.Position = wings.Position.WithWhole(body.Position.Whole);
        state.Function = KiHunterEnemyFunction.FollowBody;
        wings.PaletteIndex = body.PaletteIndex;
        wings.VramTilesIndex = body.VramTilesIndex;
        if ((body.Parameter1 & 0x8000) != 0)
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
                RunKiHunterFlyingPatrol(actor, state, samus!, level!);
                return;
            case KiHunterEnemyFunction.Swooping:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoop(actor, state, level!);
                return;
            case KiHunterEnemyFunction.RecoveringFromSwoop:
                RequireKiHunterWorld(actor, samus, level);
                RunKiHunterSwoopRecovery(actor, state, level!);
                return;
            case KiHunterEnemyFunction.FallingAfterWingLoss:
                RequireKiHunterLevel(level);
                RunKiHunterFalling(actor, state, level!);
                return;
            case KiHunterEnemyFunction.StartGroundJump:
                RequireKiHunterSamus(samus);
                StartKiHunterGroundJump(actor, state, samus!);
                return;
            case KiHunterEnemyFunction.NoOp:
                return;
            case KiHunterEnemyFunction.GroundJump:
                RequireKiHunterLevel(level);
                RunKiHunterGroundJump(actor, state, level!);
                return;
            case KiHunterEnemyFunction.GroundWait:
                RequireKiHunterSamus(samus);
                RunKiHunterGroundWait(actor, state, samus!);
                return;
            case KiHunterEnemyFunction.SelectAcidSpit:
                RequireKiHunterSamus(samus);
                SelectKiHunterAcidSpit(actor, state, samus!);
                return;
            case KiHunterEnemyFunction.FollowBody:
                FollowKiHunterBody(actor);
                return;
            case KiHunterEnemyFunction.DetachedWing:
                RequireKiHunterLevel(level);
                RunDetachedKiHunterWing(actor, state, level!);
                return;
            default:
                throw new InvalidDataException(
                    $"Ki-Hunter function $A8:{(ushort)state.Function:X4} is not translated.");
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
        else if (unchecked((short)(body.Position.Y.Whole - state.UpperPatrolY)) < 0)
        {
            state.Velocity = state.Velocity.WithWholeY(1);
        }
        else if (unchecked((short)(body.Position.Y.Whole - state.LowerPatrolY)) >= 0)
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

        state.AngularVelocityWhole = 0;
        state.AngularVelocityFraction = 0;
        state.SwoopAnimationChanged = false;
        state.SwoopRadius = new Extent16<RoomPixels>(
            absoluteXDistance,
            unchecked((ushort)(samus.Position.Y.Whole - body.Position.Y.Whole)));
        state.TargetXOrSpeedIndex = samus.Position.X.Whole;
        state.SwoopOriginY = body.Position.Y.Whole;
        state.Function = KiHunterEnemyFunction.Swooping;

        bool swoopLeft = (signedXDistance & 0x8000) != 0;
        if (swoopLeft)
        {
            state.MaximumAngularVelocity = unchecked((ushort)-2);
            state.AngularAccelerationWhole = 0xffff;
            state.AngularAccelerationFraction = 0xe000;
            state.Angle = 255;
            state.AnimationChangeAngle = 240;
            state.Velocity = state.Velocity.WithWholeX(-1);
        }
        else
        {
            state.MaximumAngularVelocity = 2;
            state.AngularAccelerationWhole = 0;
            state.AngularAccelerationFraction = 0x2000;
            state.Angle = 128;
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
        bool leftArc = (state.AngularAccelerationWhole & 0x8000) != 0;
        bool beforeAnimationChange = leftArc
            ? unchecked((short)(state.Angle - state.AnimationChangeAngle)) >= 0
            : unchecked((short)(state.Angle - state.AnimationChangeAngle)) < 0;
        if (!beforeAnimationChange && !state.SwoopAnimationChanged)
        {
            state.SwoopAnimationChanged = true;
            InstallKiHunterInstruction(
                body,
                leftArc
                    ? KiHunterInstructionProgramDefinitions.SwoopLeft
                    : KiHunterInstructionProgramDefinitions.SwoopRight);
        }

        (state.AngularVelocityWhole, state.AngularVelocityFraction) = AddKiHunterFixed(
            state.AngularVelocityWhole,
            state.AngularVelocityFraction,
            state.AngularAccelerationWhole,
            state.AngularAccelerationFraction);
        if (leftArc)
        {
            if (unchecked((short)(state.AngularVelocityWhole - state.MaximumAngularVelocity)) < 0)
                state.AngularVelocityWhole = state.MaximumAngularVelocity;
        }
        else if (unchecked((short)(state.AngularVelocityWhole - state.MaximumAngularVelocity)) >= 0)
        {
            state.AngularVelocityWhole = state.MaximumAngularVelocity;
        }

        state.Angle = unchecked((ushort)(state.Angle + state.AngularVelocityWhole));
        if ((!leftArc && unchecked((short)(state.Angle - 256)) >= 0) ||
            (leftArc && unchecked((short)(state.Angle - 128)) < 0))
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
            return;
        }

        ushort targetX = unchecked((ushort)(
            state.TargetXOrSpeedIndex +
            EnemyMath.ReadEightBitCosineProduct(state.Angle, state.SwoopRadius.X)));
        int horizontalDisplacement = SnesSixteenSixteen.FromWhole((short)(targetX - body.Position.X.Whole));
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(leftArc ? (short)1 : (short)-1, 0));
            StartKiHunterSwoopRecovery(state);
            return;
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, body);
        ushort targetY = unchecked((ushort)(
            state.SwoopOriginY +
            EnemyMath.ReadEightBitNegativeSineProduct(state.Angle, state.SwoopRadius.Y)));
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
        ushort random = _room.Random.Read();
        bool jumpLeft = unchecked((short)(body.Position.X.Whole - samus.Position.X.Whole)) >= 0;
        state.Velocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(
            jumpLeft ? (short)-2 : (short)2,
            unchecked((short)((random & 1) - 8))));
        InstallKiHunterInstruction(
            body,
            jumpLeft
                ? KiHunterInstructionProgramDefinitions.JumpLeft
                : KiHunterInstructionProgramDefinitions.JumpRight);
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
                state.WaitTimer = KiHunterGroundWaitFrames;
                InstallKiHunterInstruction(
                    body,
                    state.Velocity.X.WholePixels < 0
                        ? KiHunterInstructionProgramDefinitions.LandLeft
                        : KiHunterInstructionProgramDefinitions.LandRight);
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
        state.WaitTimer = unchecked((ushort)(state.WaitTimer - 1));
        if (state.WaitTimer != 0)
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
            samusRight
                ? KiHunterInstructionProgramDefinitions.SpitRight
                : KiHunterInstructionProgramDefinitions.SpitLeft);
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
        state.Angle = unchecked((ushort)(state.Angle + ReadKiHunterQuadraticAngleDelta(
            state.TargetXOrSpeedIndex,
            negativeHalf: true)));
        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        ushort byteAngle = SnesAngle.FromRaw(state.Angle).TableIndex;
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + EnemyMath.CircleOffset(byteAngle, radius) - state.OrbitOffset);

        if (unchecked((short)(state.Angle + 0x4000)) < 0)
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
        state.Angle = unchecked((ushort)(state.Angle + ReadKiHunterQuadraticAngleDelta(
            state.TargetXOrSpeedIndex,
            negativeHalf: false)));
        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        ushort byteAngle = SnesAngle.FromRaw(state.Angle).TableIndex;
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
        if (unchecked((short)(state.Angle + 0x4000)) >= 0)
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
            wings.Properties = (ushort)EnemyProperties.Deleted;
            return;
        }

        if (body.Health <= wings.Parameter1)
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

        wings.AiHandlerBits = body.AiHandlerBits;
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
        state.OrbitOffset = EnemyMath.CircleOffset(0xe0, radius);
        state.FallingArcOffset = EnemyMath.CircleOffset(0xa0, radius);
        state.Angle = unchecked((ushort)-0x2000);
        state.Function = KiHunterEnemyFunction.DetachedWing;
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.OrbitCenter = new Position16<RoomPixels>(
            wings.Position.X.Whole,
            unchecked((ushort)(state.SavedWingPosition.Y - state.LowerPatrolY)));
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        InstallKiHunterInstruction(
            wings,
            KiHunterInstructionProgramDefinitions.DetachedWings);
        wings.SpritemapPointer = EmptyA8Spritemap;
        wings.Properties = wings.Properties.With(EnemyProperties.ProcessOffScreen);
    }

    /// <summary>Walks the quadratic speed table until detached-wing acceleration crosses the native $2000 threshold.</summary>
    private static void CalculateKiHunterDetachedSpeedReset(KiHunterEnemyState state)
    {
        state.DetachedSpeedReset = 0;
        state.TargetXOrSpeedIndex = 0;
        do
        {
            state.DetachedSpeedReset = unchecked((ushort)(state.DetachedSpeedReset + 384));
            ushort index = unchecked((byte)(SnesEightEight.WholeOf(state.DetachedSpeedReset)));
            state.DetachedSpeedAccumulator = unchecked((ushort)(
                state.DetachedSpeedAccumulator +
                EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + 1)));
        }
        while (unchecked((short)(state.DetachedSpeedAccumulator - 0x2000)) < 0);
    }

    /// <summary>Centers the detached wing's orbit at its current position with a negative quarter-turn seed.</summary>
    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        state.Angle = unchecked((ushort)-0x2000);
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Seeds the detached wing's collision-bearing fall arc at its current position.</summary>
    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.FallingCollisionArc;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        state.Angle = unchecked((ushort)-0x6000);
        state.OrbitCenter = wings.Position.Whole;
    }

    /// <summary>Reads the unaligned quadratic-table word used by one half of detached-wing orbit motion.</summary>
    internal static ushort ReadKiHunterQuadraticAngleDelta(ushort speedIndex, bool negativeHalf)
    {
        int index = unchecked((byte)(SnesEightEight.WholeOf(speedIndex)));
        // These deliberately unaligned reads are literal: F7DB reads at record +5 and
        // F8AD at record +1, combining adjacent bytes instead of consuming a normal 16.16
        // table component. Replacing this with ReadQuadraticEnemySpeed changes the orbit.
        return EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + (negativeHalf ? 5 : 1));
    }

    /// <summary>Subtracts the native $0180 speed step, wrapping a negative result to $0100.</summary>
    private static void DecrementKiHunterDetachedSpeedIndex(KiHunterEnemyState state)
    {
        short next = unchecked((short)(state.TargetXOrSpeedIndex - 384));
        state.TargetXOrSpeedIndex = next < 0 ? (ushort)256 : unchecked((ushort)next);
    }

    /// <summary>Instruction <c>$A8:F526</c>: return to steady art and sync attached wings.</summary>
    private ushort ReturnKiHunterToSteadyInstruction(RoomEnemySlot body)
    {
        KiHunterEnemyState state = RequireKiHunterState(body);
        bool movingRight = state.Velocity.X.WholePixels >= 0;
        ushort wingInstruction = movingRight
            ? KiHunterInstructionProgramDefinitions.WingsRight
            : KiHunterInstructionProgramDefinitions.WingsLeft;
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
            InstallKiHunterInstruction(wings, wingInstruction);
        return movingRight
            ? KiHunterInstructionProgramDefinitions.FlyingRight
            : KiHunterInstructionProgramDefinitions.FlyingLeft;
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
        LastKiHunterSoundEffect = SoundEffectLibrary2Sounds.KiHunterAcidSpit.Value;
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.KiHunterAcidSpit, maximumQueued: 6);
        SpawnKiHunterAcidSpit(body, movingRight);
        RequireKiHunterState(body).WaitTimer = KiHunterSpitWaitFrames;
    }

    /// <summary>Restarts matching body and still-attached wing lists for the selected flight direction.</summary>
    private void SetKiHunterPairFacing(RoomEnemySlot body, bool movingRight)
    {
        InstallKiHunterInstruction(
            body,
            movingRight
                ? KiHunterInstructionProgramDefinitions.FlyingRight
                : KiHunterInstructionProgramDefinitions.FlyingLeft);
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
        {
            InstallKiHunterInstruction(
                wings,
                movingRight
                    ? KiHunterInstructionProgramDefinitions.WingsRight
                    : KiHunterInstructionProgramDefinitions.WingsLeft);
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
        bool initializedBody = _kiHunterStates[body.SlotIndex] is { IsWing: false };
        if ((!IsKiHunterBodyDefinition(body.EnemyDefinitionPointer) && !initializedBody) ||
            body.SlotIndex >= RoomEnemySystem.MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                $"Enemy slot {body.SlotIndex} is not a Ki-Hunter body with a following slot.");
        }

        RoomEnemySlot wings = _slotTable._slots[body.SlotIndex + 1];
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
            _kiHunterStates[wings.SlotIndex] is null)
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
        RoomEnemySlot body = _slotTable._slots[wings.SlotIndex - 1];
        _ = GetKiHunterWings(body); // Performs the exact body/variant adjacency validation.
        return body;
    }

    /// <summary>Installs typed body or wing state at the physical enemy-slot index.</summary>
    private KiHunterEnemyState CreateKiHunterState(RoomEnemySlot slot)
    {
        var state = new KiHunterEnemyState(slot);
        _kiHunterStates[slot.SlotIndex] = state;
        return state;
    }

    /// <summary>Rejects access to a body or wing record before its initializer ran.</summary>
    private KiHunterEnemyState RequireKiHunterState(RoomEnemySlot slot) =>
        _kiHunterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Ki-Hunter state.");

    /// <summary>Restarts a body or wing instruction list from its first timed entry.</summary>
    private static void InstallKiHunterInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>The <c>$0000:E000</c> 16.16 downward acceleration added to a falling or hopping body each frame.</summary>
    internal static SnesSignedSixteenSixteen KiHunterGravity => SnesSignedSixteenSixteen.FromParts(
        unchecked((short)KiHunterMotionDefinitions.GravityWhole),
        KiHunterMotionDefinitions.GravityFraction);

    /// <summary>Adds split 16.16 motion with the fractional carry retained in the whole word.</summary>
    private static (ushort Whole, ushort Fraction) AddKiHunterFixed(
        ushort whole,
        ushort fraction,
        ushort deltaWhole,
        ushort deltaFraction)
    {
        uint lowSum = (uint)fraction + deltaFraction;
        return (
            unchecked((ushort)(whole + deltaWhole + (SnesCarry.OutOfWord(lowSum)))),
            unchecked((ushort)lowSum));
    }

    /// <summary>Requires a live Samus, room collision data and a body definition before body-only movement AI.</summary>
    private static void RequireKiHunterWorld(
        RoomEnemySlot actor,
        SamusState? samus,
        RoomLevelData? level)
    {
        RequireKiHunterSamus(samus);
        RequireKiHunterLevel(level);
        if (!IsKiHunterBodyDefinition(actor.EnemyDefinitionPointer))
            throw new InvalidDataException("A Ki-Hunter wing entered body-only movement AI.");
    }

    /// <summary>Rejects a missing Samus actor for target-dependent Ki-Hunter movement.</summary>
    private static void RequireKiHunterSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ki-Hunter AI requires the active Samus actor.");
    }

    /// <summary>Rejects missing room collision data for flight and ground movement.</summary>
    private static void RequireKiHunterLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Ki-Hunter movement requires room collision data.");
    }

    /// <summary>Runs wing-detachment logic after common bomb damage to a Ki-Hunter body.</summary>
    internal void RegisterKiHunterBombReactions(EnemyAiTable table)
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

    /// <summary>Registers separate bank-$A8 initializers for body and wing records.</summary>
    internal void RegisterKiHunterInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Kihunter, slot => IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKiHunter(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_KihunterWings, slot => IsKiHunterWingDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKiHunterWings(slot);
        });
    }

    /// <summary>Routes both body and wing main addresses through the shared per-record dispatcher.</summary>
    internal void RegisterKiHunterMainAi(EnemyAiTable table)
    {

        EnemyMainAiHandler shared85 = (slot, frame, nmiFrameCounter8) =>
        {
            RunKiHunterMain(slot, RequireKiHunterState(slot), frame.Samus, frame.Level);
        };

        table.RegisterMain(EnemyAiRoutine.MainAI_Kihunter, slot => IsKiHunterBodyDefinition(slot.EnemyDefinitionPointer), shared85);

        table.RegisterMain(EnemyAiRoutine.MainAI_KihunterWings, slot => IsKiHunterWingDefinition(slot.EnemyDefinitionPointer), shared85);
    }

    /// <summary>Dispatches the body's private animation callbacks before generic enemy bytecode.</summary>
    internal void RegisterKiHunterOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessKiHunterInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Runs wing-detachment logic after common power-bomb damage to a body.</summary>
    internal void RegisterKiHunterPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.KiHunterShot)
        {
            AfterCommon = (enemy, _) => ResolveKiHunterShotAfterCommon(enemy),
        });
    }

    /// <summary>Runs wing-detachment logic after common shot damage to a body.</summary>
    internal void RegisterKiHunterShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.KiHunterShot)
        {
            AfterCommon = (c, _) => ResolveKiHunterShotAfterCommon(c.Enemy),
        });
    }
    /// <summary>Bank-$86 acid mover installed by the startup callback after the projectile's idle frames.</summary>
    private const ushort KiHunterAcidMovingPreInstruction =
        (ushort)EnemyProjectilePreInstruction.KiHunterAcid_Moving;
    /// <summary>Positive 8.8 horizontal acid speed, negated for leftward shots.</summary>
    private const ushort KiHunterAcidHorizontalSpeed = 0x0300;
    /// <summary>8.8 downward speed added to each active acid frame.</summary>
    private const ushort KiHunterAcidGravity = 0x0010;
    /// <summary>Maximum 8.8 downward speed for a falling acid projectile.</summary>
    private const ushort KiHunterAcidTerminalYSpeed = 0x0200;

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
            projectile.InstructionPointer = KiHunterAcidSpitInstructionProgramDefinitions.HitFloor;
            projectile.InstructionTimer = 1;
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
