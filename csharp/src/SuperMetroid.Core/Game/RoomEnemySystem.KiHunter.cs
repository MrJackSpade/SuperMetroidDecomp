using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function pointers stored in Ki-Hunter variable A. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with enemy WRAM.
/// </summary>
public enum KiHunterEnemyFunction : ushort
{
    FlyingPatrol = 0xf268,
    Swooping = 0xf3b8,
    RecoveringFromSwoop = 0xf4ed,
    FallingAfterWingLoss = 0xf55a,
    StartGroundJump = 0xf58b,
    NoOp = 0xf5e3,
    GroundJump = 0xf5f0,
    GroundWait = 0xf68b,
    SelectAcidSpit = 0xf6b3,
    FollowBody = 0xf6f3,
    DetachedWing = 0xf7cf,
}

/// <summary>The secondary function stored in detached-wing variable zero.</summary>
public enum KiHunterWingFunction : ushort
{
    Orbit = 0xf7db,
    FallingCollisionArc = 0xf8ad,
}

/// <summary>
/// Typed projection of Ki-Hunter's ordinary variables A-F plus the extended enemy words at
/// $7E:7800-$7828,x. Extended words cannot live in <see cref="RoomEnemySlot"/>'s common
/// 64-byte record, so they remain explicit here under the exact meanings used by the ROM.
/// Several words are intentionally aliased: the detached wing reuses the body's flight
/// variables for its orbit geometry, just as the original assembly does.
/// </summary>
public sealed class KiHunterEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly bool _isWing;

    internal KiHunterEnemyState(RoomEnemySlot slot)
    {
        _slot = slot;
        // The definition word is mutable native storage and is cleared during deletion.
        // Role is established by the initializer and must survive that later write because
        // body/wing callbacks continue using physical +/-$40 slot aliases.
        _isWing = RoomEnemySystem.IsKiHunterWingDefinition(slot.EnemyDefinitionPointer);
    }

    public bool IsWing => _isWing;

    /// <summary>Native variable A: main-AI dispatcher.</summary>
    public KiHunterEnemyFunction Function
    {
        get => (KiHunterEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: swoop target X or detached-wing speed-table index.</summary>
    public ushort TargetXOrSpeedIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: the Y origin of the current swoop.</summary>
    public ushort SwoopOriginY
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: detached-wing quadratic-speed accumulator.</summary>
    public ushort DetachedSpeedAccumulator
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: angle at which the swoop changes animation.</summary>
    public ushort AnimationChangeAngle
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native variable F: 8.8-ish swoop/detached-wing angle accumulator.</summary>
    public ushort Angle
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort MaximumAngularVelocity { get; internal set; }
    public KiHunterWingFunction WingFunction { get; internal set; }
    public ushort FallingArcXOffset { get; internal set; }
    public ushort AngularVelocityWhole { get; internal set; }
    public ushort FallingArcYOffset { get; internal set; }
    public ushort AngularVelocityFraction { get; internal set; }
    public ushort OrbitXOffset { get; internal set; }
    public ushort AngularAccelerationWhole { get; internal set; }
    public ushort OrbitYOffset { get; internal set; }
    public ushort AngularAccelerationFraction { get; internal set; }
    public ushort OrbitCenterX { get; internal set; }
    public ushort HorizontalSubvelocity { get; internal set; }
    public ushort OrbitCenterY { get; internal set; }
    public ushort HorizontalVelocity { get; internal set; }
    public ushort SavedWingY { get; internal set; }
    public ushort VerticalSubvelocity { get; internal set; }
    public ushort SavedWingX { get; internal set; }
    public ushort VerticalVelocity { get; internal set; }
    public ushort UpperPatrolY { get; internal set; }
    public ushort DetachedSpeedReset { get; internal set; }
    public ushort LowerPatrolY { get; internal set; }
    public ushort SpawnX { get; internal set; }
    public ushort SpawnY { get; internal set; }
    public ushort WaitTimer { get; internal set; }
    public bool SwoopAnimationChanged { get; internal set; }
    public ushort SwoopHorizontalRadius { get; internal set; }
    public ushort SwoopVerticalRadius { get; internal set; }
    public bool HasLostWings { get; internal set; }
}

/// <summary>
/// Complete translation of the normal, red, and gold Ki-Hunter body/wing pairs. All six
/// enemy headers share these two routines; variant health, damage, palettes, and graphics
/// continue to come from their own retail definition records.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort KiHunterDefinition = 0xeabf;
    internal const ushort KiHunterWingsDefinition = 0xeaff;
    internal const ushort RedKiHunterDefinition = 0xeb3f;
    internal const ushort RedKiHunterWingsDefinition = 0xeb7f;
    internal const ushort GoldKiHunterDefinition = 0xebbf;
    internal const ushort GoldKiHunterWingsDefinition = 0xebff;
    internal const ushort KiHunterShotAi = EnemyAiCodePointers.BankA8.KiHunterShot;

    private const ushort KiHunterFlyingLeftInstruction = 0xe9fa;
    private const ushort KiHunterFlyingRightInstruction = 0xea24;
    private const ushort KiHunterSwoopLeftInstruction = 0xea08;
    private const ushort KiHunterSwoopRightInstruction = 0xea32;
    private const ushort KiHunterWingsLeftInstruction = 0xea4e;
    private const ushort KiHunterWingsRightInstruction = 0xea5e;
    private const ushort KiHunterDetachedWingsInstruction = 0xea7e;
    private const ushort KiHunterJumpLeftInstruction = 0xea8a;
    private const ushort KiHunterJumpRightInstruction = 0xeaa6;
    private const ushort KiHunterLandLeftInstruction = 0xeac2;
    private const ushort KiHunterLandRightInstruction = 0xeada;
    private const ushort KiHunterSpitLeftInstruction = 0xeaf2;
    private const ushort KiHunterSpitRightInstruction = 0xeb10;
    private const ushort EmptyA8Spritemap = 0x804d;
    private const ushort KiHunterGroundWaitFrames = 12;
    private const ushort KiHunterSpitWaitFrames = 24;
    private const ushort KiHunterGroundAttackDistance = 96;

    private readonly KiHunterEnemyState?[] _kiHunterStates =
        new KiHunterEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Ki-Hunter state for every body and wing physical enemy slot.</summary>
    public IReadOnlyList<KiHunterEnemyState?> KiHunterStates => _kiHunterStates;

    /// <summary>Last library-two sound queued by an acid-spit instruction this frame.</summary>
    public ushort? LastKiHunterSoundEffect { get; private set; }

    internal static bool IsKiHunterBodyDefinition(ushort definition) =>
        definition is KiHunterDefinition or RedKiHunterDefinition or GoldKiHunterDefinition;

    internal static bool IsKiHunterWingDefinition(ushort definition) =>
        definition is KiHunterWingsDefinition or RedKiHunterWingsDefinition or GoldKiHunterWingsDefinition;

    private static bool IsKiHunterDefinition(ushort definition) =>
        IsKiHunterBodyDefinition(definition) || IsKiHunterWingDefinition(definition);

    private void ResetKiHunterRoomState()
    {
        Array.Clear(_kiHunterStates);
        LastKiHunterSoundEffect = null;
    }

    /// <summary>Ports body initializer <c>$A8:F188</c>.</summary>
    private void InitializeKiHunter(RoomEnemySlot body)
    {
        KiHunterEnemyState state = CreateKiHunterState(body);

        // Every Ki-Hunter list is interpreted by the common enemy bytecode engine. The
        // disassembly calls this property "disable Samus collision", but the live engine
        // proves bit $2000 is the process-instructions flag.
        body.Properties = body.Properties.With(EnemyProperties.ProcessInstructions);
        state.HasLostWings = false;
        InstallKiHunterInstruction(body, KiHunterFlyingLeftInstruction);
        state.Angle = 0;
        state.Function = KiHunterEnemyFunction.FlyingPatrol;
        state.VerticalSubvelocity = 0;
        state.VerticalVelocity = 1;
        state.HorizontalSubvelocity = 0;
        state.HorizontalVelocity = unchecked((ushort)-1);
        state.UpperPatrolY = unchecked((ushort)(body.YPosition - 16));
        state.LowerPatrolY = unchecked((ushort)(state.UpperPatrolY + 32));
        state.SpawnX = body.XPosition;
        state.SpawnY = body.YPosition;

        // Parameter bit 15 selects the permanently ground-bound form used by two red
        // Ki-Hunters. Its following wing record starts deleted and the body skips every
        // later detachment side effect.
        if ((body.Parameter1 & 0x8000) != 0)
        {
            state.HasLostWings = true;
            state.Function = KiHunterEnemyFunction.FallingAfterWingLoss;
            state.VerticalSubvelocity = 0;
            state.VerticalVelocity = 1;
        }
    }

    /// <summary>Ports wing initializer <c>$A8:F214</c>, including its preceding-slot aliases.</summary>
    private void InitializeKiHunterWings(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        KiHunterEnemyState state = CreateKiHunterState(wings);

        wings.Properties = wings.Properties.With(EnemyProperties.ProcessInstructions);
        InstallKiHunterInstruction(wings, KiHunterWingsLeftInstruction);
        wings.XPosition = body.XPosition;
        wings.YPosition = body.YPosition;
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
        if (MoveEnemyVertically(
                level,
                body,
                ComposeKiHunterFixed(state.VerticalVelocity, state.VerticalSubvelocity)))
        {
            state.VerticalVelocity = unchecked((ushort)-state.VerticalVelocity);
        }
        else if (unchecked((short)(body.YPosition - state.UpperPatrolY)) < 0)
        {
            state.VerticalVelocity = 1;
        }
        else if (unchecked((short)(body.YPosition - state.LowerPatrolY)) >= 0)
        {
            state.VerticalVelocity = unchecked((ushort)-1);
        }

        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                ComposeKiHunterFixed(state.HorizontalVelocity, state.HorizontalSubvelocity)))
        {
            state.HorizontalVelocity = unchecked((ushort)-state.HorizontalVelocity);
            SetKiHunterPairFacing(body, unchecked((short)state.HorizontalVelocity) >= 0);
        }
        AlignEnemyYWithNonSquareSlope(level, body);

        ushort signedXDistance = unchecked((ushort)(samus.XPosition - body.XPosition));
        ushort absoluteXDistance = WrappedMagnitude(signedXDistance);
        ushort triggerDistance = ReadWord(
            _bus!, EnemyRomTablePointers.KiHunter.TriggerDistanceWord);
        if (unchecked((short)(absoluteXDistance - triggerDistance)) >= 0 ||
            unchecked((short)(samus.YPosition - body.YPosition - 32)) < 0)
        {
            return;
        }

        state.AngularVelocityWhole = 0;
        state.AngularVelocityFraction = 0;
        state.SwoopAnimationChanged = false;
        state.SwoopHorizontalRadius = absoluteXDistance;
        state.SwoopVerticalRadius = unchecked((ushort)(samus.YPosition - body.YPosition));
        state.TargetXOrSpeedIndex = samus.XPosition;
        state.SwoopOriginY = body.YPosition;
        state.Function = KiHunterEnemyFunction.Swooping;

        bool swoopLeft = (signedXDistance & 0x8000) != 0;
        if (swoopLeft)
        {
            state.MaximumAngularVelocity = unchecked((ushort)-2);
            state.AngularAccelerationWhole = 0xffff;
            state.AngularAccelerationFraction = 0xe000;
            state.Angle = 255;
            state.AnimationChangeAngle = 240;
            state.HorizontalVelocity = unchecked((ushort)-1);
        }
        else
        {
            state.MaximumAngularVelocity = 2;
            state.AngularAccelerationWhole = 0;
            state.AngularAccelerationFraction = 0x2000;
            state.Angle = 128;
            state.AnimationChangeAngle = 144;
            state.HorizontalVelocity = 1;
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
                leftArc ? KiHunterSwoopLeftInstruction : KiHunterSwoopRightInstruction);
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
            ReadEightBitCosineProduct(state.Angle, state.SwoopHorizontalRadius)));
        int horizontalDisplacement = unchecked((short)(targetX - body.XPosition)) << 16;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            state.HorizontalSubvelocity = 0;
            state.HorizontalVelocity = leftArc ? (ushort)1 : unchecked((ushort)-1);
            StartKiHunterSwoopRecovery(state);
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
        ushort targetY = unchecked((ushort)(
            state.SwoopOriginY +
            ReadEightBitNegativeSineProduct(state.Angle, state.SwoopVerticalRadius)));
        int verticalDisplacement = unchecked((short)(targetY - body.YPosition)) << 16;
        if (MoveEnemyVertically(level, body, verticalDisplacement))
            StartKiHunterSwoopRecovery(state);
    }

    private static void StartKiHunterSwoopRecovery(KiHunterEnemyState state)
    {
        state.Function = KiHunterEnemyFunction.RecoveringFromSwoop;
        state.VerticalSubvelocity = 0;
        state.VerticalVelocity = unchecked((ushort)-1);
    }

    /// <summary>Ports the collision escape leg at <c>$A8:F4ED</c>.</summary>
    private void RunKiHunterSwoopRecovery(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                ComposeKiHunterFixed(state.HorizontalVelocity, state.HorizontalSubvelocity)))
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
        if (MoveEnemyVertically(
                level,
                body,
                ComposeKiHunterFixed(state.VerticalVelocity, state.VerticalSubvelocity)) ||
            unchecked((short)(body.YPosition - state.SpawnY)) < 0)
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
        if (MoveEnemyVertically(
                level,
                body,
                ComposeKiHunterFixed(state.VerticalVelocity, state.VerticalSubvelocity)))
        {
            state.Function = KiHunterEnemyFunction.StartGroundJump;
            return;
        }

        (state.VerticalVelocity, state.VerticalSubvelocity) = AddKiHunterFixed(
            state.VerticalVelocity,
            state.VerticalSubvelocity,
            ReadWord(_bus!, EnemyRomTablePointers.KiHunter.AttackXRadiusWord),
            ReadWord(_bus!, EnemyRomTablePointers.KiHunter.AttackYRadiusWord));
    }

    /// <summary>Ports grounded jump setup <c>$A8:F58B</c>.</summary>
    private void StartKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        SamusState samus)
    {
        state.Function = KiHunterEnemyFunction.NoOp;
        state.VerticalSubvelocity = 0;
        ushort random = RequireRandomNumber();
        state.VerticalVelocity = unchecked((ushort)((random & 1) - 8));
        bool jumpLeft = unchecked((short)(body.XPosition - samus.XPosition)) >= 0;
        state.HorizontalSubvelocity = 0;
        state.HorizontalVelocity = jumpLeft ? unchecked((ushort)-2) : (ushort)2;
        InstallKiHunterInstruction(
            body,
            jumpLeft ? KiHunterJumpLeftInstruction : KiHunterJumpRightInstruction);
    }

    /// <summary>Ports airborne hop function <c>$A8:F5F0</c>.</summary>
    private void RunKiHunterGroundJump(
        RoomEnemySlot body,
        KiHunterEnemyState state,
        RoomLevelData level)
    {
        if (MoveEnemyVertically(
                level,
                body,
                ComposeKiHunterFixed(state.VerticalVelocity, state.VerticalSubvelocity)))
        {
            if ((state.VerticalVelocity & 0x8000) != 0)
            {
                state.VerticalVelocity = 1;
            }
            else
            {
                state.VerticalSubvelocity = 0;
                state.VerticalVelocity = unchecked((ushort)-4);
                state.Function = KiHunterEnemyFunction.NoOp;
                state.WaitTimer = KiHunterGroundWaitFrames;
                InstallKiHunterInstruction(
                    body,
                    unchecked((short)state.HorizontalVelocity) < 0
                        ? KiHunterLandLeftInstruction
                        : KiHunterLandRightInstruction);
            }
            return;
        }

        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                ComposeKiHunterFixed(state.HorizontalVelocity, state.HorizontalSubvelocity)))
        {
            state.HorizontalVelocity = unchecked((ushort)-state.HorizontalVelocity);
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
        (state.VerticalVelocity, state.VerticalSubvelocity) = AddKiHunterFixed(
            state.VerticalVelocity,
            state.VerticalSubvelocity,
            ReadWord(_bus!, EnemyRomTablePointers.KiHunter.AttackXRadiusWord),
            ReadWord(_bus!, EnemyRomTablePointers.KiHunter.AttackYRadiusWord));
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

        ushort distance = WrappedMagnitude(unchecked((ushort)(body.XPosition - samus.XPosition)));
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
        bool samusRight = unchecked((short)(body.XPosition - samus.XPosition)) < 0;
        InstallKiHunterInstruction(
            body,
            samusRight ? KiHunterSpitRightInstruction : KiHunterSpitLeftInstruction);
        state.Function = KiHunterEnemyFunction.NoOp;
    }

    /// <summary>Ports attached-wing follower <c>$A8:F6F3</c>.</summary>
    private void FollowKiHunterBody(RoomEnemySlot wings)
    {
        RoomEnemySlot body = GetKiHunterBody(wings);
        wings.XPosition = body.XPosition;
        wings.YPosition = body.YPosition;
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
    private void RunDetachedKiHunterWingOrbit(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.Angle = unchecked((ushort)(state.Angle + ReadKiHunterQuadraticAngleDelta(
            state.TargetXOrSpeedIndex,
            negativeHalf: true)));
        ushort radius = _bus!.ReadByte(EnemyRomTablePointers.KiHunter.WinglessHopRadiusByte);
        ushort byteAngle = unchecked((byte)(state.Angle >> 8));
        wings.YPosition = unchecked((ushort)(
            state.OrbitCenterY +
            ReadEightBitNegativeSineProduct(byteAngle, radius) -
            state.OrbitYOffset));
        wings.XPosition = unchecked((ushort)(
            state.OrbitCenterX + ReadEightBitCosineProduct(byteAngle, radius) - state.OrbitXOffset));

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
        ushort radius = _bus!.ReadByte(EnemyRomTablePointers.KiHunter.WinglessHopRadiusByte);
        ushort byteAngle = unchecked((byte)(state.Angle >> 8));
        ushort desiredY = unchecked((ushort)(
            state.OrbitCenterY +
            ReadEightBitNegativeSineProduct(byteAngle, radius) -
            state.FallingArcYOffset));
        if (MoveEnemyVertically(
                level,
                wings,
                unchecked((short)(desiredY - wings.YPosition)) << 16))
        {
            wings.Properties = wings.Properties.With(EnemyProperties.Deleted);
            wings.XPosition = state.SavedWingX;
            wings.YPosition = state.SavedWingY;
            DecrementKiHunterDetachedSpeedIndex(state);
            return;
        }

        wings.XPosition = unchecked((ushort)(
            state.OrbitCenterX + ReadEightBitCosineProduct(byteAngle, radius) - state.FallingArcXOffset));
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
                bodyState.VerticalSubvelocity = 0;
                bodyState.VerticalVelocity = 1;
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
    private void DetachKiHunterWings(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.SavedWingY = wings.YPosition;
        state.SavedWingX = wings.XPosition;
        CalculateKiHunterDetachedSpeedReset(state);

        ushort radius = _bus!.ReadByte(EnemyRomTablePointers.KiHunter.WinglessHopRadiusByte);
        state.OrbitXOffset = unchecked((ushort)ReadEightBitCosineProduct(0xe0, radius));
        state.OrbitYOffset = unchecked((ushort)ReadEightBitNegativeSineProduct(0xe0, radius));
        state.FallingArcXOffset = unchecked((ushort)ReadEightBitCosineProduct(0xa0, radius));
        state.FallingArcYOffset = unchecked((ushort)ReadEightBitNegativeSineProduct(0xa0, radius));
        state.Angle = unchecked((ushort)-0x2000);
        state.Function = KiHunterEnemyFunction.DetachedWing;
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.OrbitCenterY = unchecked((ushort)(state.SavedWingY - state.LowerPatrolY));
        state.OrbitCenterX = wings.XPosition;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        InstallKiHunterInstruction(wings, KiHunterDetachedWingsInstruction);
        wings.SpritemapPointer = EmptyA8Spritemap;
        wings.Properties = wings.Properties.With(EnemyProperties.ProcessOffScreen);
    }

    private static void CalculateKiHunterDetachedSpeedReset(KiHunterEnemyState state)
    {
        state.DetachedSpeedReset = 0;
        state.TargetXOrSpeedIndex = 0;
        do
        {
            state.DetachedSpeedReset = unchecked((ushort)(state.DetachedSpeedReset + 384));
            ushort index = unchecked((byte)(state.DetachedSpeedReset >> 8));
            state.DetachedSpeedAccumulator = unchecked((ushort)(
                state.DetachedSpeedAccumulator +
                EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + 1)));
        }
        while (unchecked((short)(state.DetachedSpeedAccumulator - 0x2000)) < 0);
    }

    private static void BeginDetachedKiHunterWingOrbit(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.Orbit;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        state.Angle = unchecked((ushort)-0x2000);
        state.OrbitCenterX = wings.XPosition;
        state.OrbitCenterY = wings.YPosition;
    }

    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.FallingCollisionArc;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        state.Angle = unchecked((ushort)-0x6000);
        state.OrbitCenterX = wings.XPosition;
        state.OrbitCenterY = wings.YPosition;
    }

    private static ushort ReadKiHunterQuadraticAngleDelta(ushort speedIndex, bool negativeHalf)
    {
        int index = unchecked((byte)(speedIndex >> 8));
        // These deliberately unaligned reads are literal: F7DB reads at record +5 and
        // F8AD at record +1, combining adjacent bytes instead of consuming a normal 16.16
        // table component. Replacing this with ReadQuadraticEnemySpeed changes the orbit.
        return EnemyQuadraticSpeedDefinitions.ReadWord(index * 8 + (negativeHalf ? 5 : 1));
    }

    private static void DecrementKiHunterDetachedSpeedIndex(KiHunterEnemyState state)
    {
        short next = unchecked((short)(state.TargetXOrSpeedIndex - 384));
        state.TargetXOrSpeedIndex = next < 0 ? (ushort)256 : unchecked((ushort)next);
    }

    /// <summary>Instruction <c>$A8:F526</c>: return to steady art and sync attached wings.</summary>
    private ushort ReturnKiHunterToSteadyInstruction(RoomEnemySlot body)
    {
        KiHunterEnemyState state = RequireKiHunterState(body);
        bool movingRight = unchecked((short)state.HorizontalVelocity) >= 0;
        ushort wingInstruction = movingRight
            ? KiHunterWingsRightInstruction
            : KiHunterWingsLeftInstruction;
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
            InstallKiHunterInstruction(wings, wingInstruction);
        return movingRight ? KiHunterFlyingRightInstruction : KiHunterFlyingLeftInstruction;
    }

    private static void StartKiHunterGroundJumpFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundJump;

    private static void StartKiHunterGroundWaitFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundWait;

    private void SpawnKiHunterAcidFromInstruction(RoomEnemySlot body, bool movingRight)
    {
        LastKiHunterSoundEffect = 0x004c;
        SpawnKiHunterAcidSpit(body, movingRight);
        RequireKiHunterState(body).WaitTimer = KiHunterSpitWaitFrames;
    }

    private void SetKiHunterPairFacing(RoomEnemySlot body, bool movingRight)
    {
        InstallKiHunterInstruction(
            body,
            movingRight ? KiHunterFlyingRightInstruction : KiHunterFlyingLeftInstruction);
        RoomEnemySlot wings = GetKiHunterWings(body);
        KiHunterEnemyState wingState = RequireKiHunterState(wings);
        if (wingState.Function == KiHunterEnemyFunction.FollowBody)
        {
            InstallKiHunterInstruction(
                wings,
                movingRight ? KiHunterWingsRightInstruction : KiHunterWingsLeftInstruction);
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
            body.SlotIndex >= MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                $"Enemy slot {body.SlotIndex} is not a Ki-Hunter body with a following slot.");
        }

        RoomEnemySlot wings = _slots[body.SlotIndex + 1];
        if (body.EnemyDefinitionPointer == 0 && initializedBody)
            return wings;

        ushort expected = body.EnemyDefinitionPointer switch
        {
            KiHunterDefinition => KiHunterWingsDefinition,
            RedKiHunterDefinition => RedKiHunterWingsDefinition,
            GoldKiHunterDefinition => GoldKiHunterWingsDefinition,
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
                $"Ki-Hunter body slot {body.SlotIndex} is followed by ${wings.EnemyDefinitionPointer:X4}, " +
                $"not expected wing ${expected:X4}.");
        }
        return wings;
    }

    private RoomEnemySlot GetKiHunterBody(RoomEnemySlot wings)
    {
        if (!IsKiHunterWingDefinition(wings.EnemyDefinitionPointer) || wings.SlotIndex == 0)
            throw new InvalidDataException($"Enemy slot {wings.SlotIndex} is not a linked Ki-Hunter wing.");
        RoomEnemySlot body = _slots[wings.SlotIndex - 1];
        _ = GetKiHunterWings(body); // Performs the exact body/variant adjacency validation.
        return body;
    }

    private KiHunterEnemyState CreateKiHunterState(RoomEnemySlot slot)
    {
        var state = new KiHunterEnemyState(slot);
        _kiHunterStates[slot.SlotIndex] = state;
        return state;
    }

    private KiHunterEnemyState RequireKiHunterState(RoomEnemySlot slot) =>
        _kiHunterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Ki-Hunter state.");

    private static void InstallKiHunterInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static int ComposeKiHunterFixed(ushort whole, ushort fraction) =>
        unchecked((int)(((uint)whole << 16) | fraction));

    private static (ushort Whole, ushort Fraction) AddKiHunterFixed(
        ushort whole,
        ushort fraction,
        ushort deltaWhole,
        ushort deltaFraction)
    {
        uint lowSum = (uint)fraction + deltaFraction;
        return (
            unchecked((ushort)(whole + deltaWhole + (lowSum >> 16))),
            unchecked((ushort)lowSum));
    }

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

    private static void RequireKiHunterSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ki-Hunter AI requires the active Samus actor.");
    }

    private static void RequireKiHunterLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Ki-Hunter movement requires room collision data.");
    }
}
