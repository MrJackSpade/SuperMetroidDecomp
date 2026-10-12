using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function pointers stored in Ki-Hunter variable A. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with enemy WRAM.
/// </summary>
public enum KiHunterEnemyFunction : ushort
{
    /// <summary><c>$A8:F268 Function_Kihunter_Winged_IdleFlying</c>: patrols within the spawn-height band, reflects movement on collision, and starts a swoop toward nearby Samus below.</summary>
    FlyingPatrol = 0xf268,
    /// <summary><c>$A8:F3B8 Function_Kihunter_Winged_Swoop</c>: follows an accelerating elliptic dive, changing art at the swipe angle and backing off on collision.</summary>
    Swooping = 0xf3b8,
    /// <summary><c>$A8:F4ED Function_Kihunter_Winged_BackOff</c>: moves upward away from a blocked dive until collision or passage above the spawn Y restores patrol.</summary>
    RecoveringFromSwoop = 0xf4ed,
    /// <summary><c>$A8:F55A Function_Kihunter_Wingless_InitialFalling</c>: applies gravity after clipping wings or a ground-bound spawn, then prepares a hop on landing.</summary>
    FallingAfterWingLoss = 0xf55a,
    /// <summary><c>$A8:F58B Function_Kihunter_Wingless_PrepareToHop</c>: aims a two-pixel-per-frame hop toward Samus with an RNG-selected initial upward speed of seven or eight pixels per frame.</summary>
    StartGroundJump = 0xf58b,
    /// <summary><c>$A8:F5E3 RTL_A8F5E3</c>: leaves movement to the jump, landing, or spit animation until its instruction stream selects the next function.</summary>
    NoOp = 0xf5e3,
    /// <summary><c>$A8:F5F0 Function_Kihunter_Wingless_Hop</c>: advances the collision-bearing ballistic hop and starts the landing animation after downward collision.</summary>
    GroundJump = 0xf5f0,
    /// <summary><c>$A8:F68B Function_Kihunter_Wingless_Thinking</c>: counts down the post-landing or post-spit wait, then spits within 96 horizontal pixels or prepares another hop.</summary>
    GroundWait = 0xf68b,
    /// <summary><c>$A8:F6B3 Function_Kihunter_Wingless_FireAcidSpit</c>: installs the acid-spit list facing Samus and suspends movement while bytecode owns the attack.</summary>
    SelectAcidSpit = 0xf6b3,
    /// <summary><c>$A8:F6F3 Function_KihunterWings_Attached</c>: copies X and Y from the physically preceding body slot, native enemy minus $40.</summary>
    FollowBody = 0xf6f3,
    /// <summary><c>$A8:F7CF Function_KihunterWings_Falling</c>: dispatches the detached wing's secondary arc function stored in its extended variable zero.</summary>
    DetachedWing = 0xf7cf,
}

/// <summary>The secondary function stored in detached-wing variable zero.</summary>
public enum KiHunterWingFunction : ushort
{
    /// <summary><c>$A8:F7DB Function_KihunterWings_Falling_DriftingLeft</c>: advances a radius-$30 orbit without collision, switching arcs when the angle crosses $C000.</summary>
    Orbit = 0xf7db,
    /// <summary><c>$A8:F8AD Function_KihunterWings_Falling_DriftingRight</c>: advances the opposite radius-$30 arc with vertical collision, deleting the wing on impact or switching back across $C000.</summary>
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

    internal KiHunterEnemyState(RoomEnemySlot slot)
    {
        _slot = slot;
        // The definition word is mutable native storage and is cleared during deletion.
        // Role is established by the initializer and must survive that later write because
        // body/wing callbacks continue using physical +/-$40 slot aliases.
        IsWing = RoomEnemySystem.IsKiHunterWingDefinition(slot.EnemyDefinitionPointer);
    }

    /// <summary>Role fixed by the initializer, retained even if deletion clears the native definition word; body and wing callbacks still use adjacent physical slots.</summary>
    public bool IsWing { get; }

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

    /// <summary>Body $7E:7800,x signed whole-angle velocity cap: $FFFE for a leftward swoop or two for a rightward swoop.</summary>
    public ushort MaximumAngularVelocity { get; internal set; }
    /// <summary>Wing $7E:7800,x bank-$A8 secondary function pointer, alternating between non-colliding and collision-bearing detached arcs.</summary>
    public KiHunterWingFunction WingFunction { get; internal set; }
    /// <summary>Body $7E:7804,x signed high word of 16.16 angular velocity; this whole-angle step advances the swoop angle each frame.</summary>
    public ushort AngularVelocityWhole { get; internal set; }
    /// <summary>Body $7E:7806,x fractional low word of angular velocity; acceleration carries into <see cref="AngularVelocityWhole"/>.</summary>
    public ushort AngularVelocityFraction { get; internal set; }
    /// <summary>Body $7E:7808,x signed high word of angular acceleration, paired with <see cref="AngularAccelerationFraction"/> for a plus or minus one-eighth angle step per frame squared.</summary>
    public ushort AngularAccelerationWhole { get; internal set; }
    /// <summary>Body $7E:780A,x low word of 16.16 angular acceleration: $2000 rightward or $E000 with high word $FFFF leftward.</summary>
    public ushort AngularAccelerationFraction { get; internal set; }
    /// <summary>Body $7E:7814,x upper room-pixel patrol boundary, initialized sixteen pixels above the spawn Y.</summary>
    public ushort UpperPatrolY { get; internal set; }
    /// <summary>Wing $7E:7814,x 8.8 speed-table reset index computed at detachment by accumulating angular deltas until $2000; reapplied at each arc transition.</summary>
    public ushort DetachedSpeedReset { get; internal set; }
    /// <summary>Body $7E:7816,x lower room-pixel patrol boundary, initialized sixteen pixels below spawn Y; wing detachment also reads its own same-address word.</summary>
    public ushort LowerPatrolY { get; internal set; }
    /// <summary>Body $7E:7818/$781A,x initial room position retained from body initialization rather than the changing patrol position; collision recovery returns to patrol after rising above its Y.</summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }
    /// <summary>Body $7E:781E,x gameplay-frame thinking countdown: twelve after landing or twenty-four after acid emission, consumed once the animation selects ground wait.</summary>
    public ushort WaitTimer { get; internal set; }
    /// <summary>Body $7E:7820,x swipe-animation latch, cleared on swoop setup and set once the angle passes its facing-specific trigger.</summary>
    public bool SwoopAnimationChanged { get; internal set; }
    /// <summary>Body $7E:7822/$7824,x whole-pixel swoop ellipse radii: the absolute horizontal separation from Samus and Samus Y minus body Y when a swoop begins.</summary>
    public Extent16<RoomPixels> SwoopRadius { get; internal set; }
    /// <summary>Body $7E:780C-$7812,x signed 16.16 velocity in pixels per gameplay frame (X fraction/whole at $780C/$780E, Y at $7810/$7812); positive Y is downward and gravity accumulates through the Y fraction.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Wing $7E:7806/$7808,x signed pixel correction for the non-colliding arc, calculated at angle $E0 and radius $30.</summary>
    public Offset16<RoomPixels> OrbitOffset { get; internal set; }
    /// <summary>Wing $7E:7802/$7804,x signed pixel correction for the collision-bearing arc, calculated at angle $A0 and radius $30.</summary>
    public Offset16<RoomPixels> FallingArcOffset { get; internal set; }
    /// <summary>Wing $7E:780A/$780C,x room-pixel anchor used with an arc-specific correction; recaptured whenever the detached arc changes.</summary>
    public Position16<RoomPixels> OrbitCenter { get; internal set; }
    /// <summary>Wing $7E:7810/$780E,x original room position saved at detachment and restored when the collision-bearing arc deletes the wing.</summary>
    public Position16<RoomPixels> SavedWingPosition { get; internal set; }
    /// <summary>Body $7E:7828,x wingless latch, set by spawn parameter-one bit 15 or by health reaching the following wing record's parameter-one threshold.</summary>
    public bool HasLostWings { get; internal set; }
}

/// <summary>
/// Complete translation of the normal, red, and gold Ki-Hunter body/wing pairs. All six
/// enemy headers share these two routines; variant health, damage, palettes, and graphics
/// continue to come from their own retail definition records.
/// </summary>
public sealed partial class RoomEnemySystem
{
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


    private const ushort EmptyA8Spritemap = 0x804d;
    private const ushort KiHunterGroundWaitFrames = 12;
    private const ushort KiHunterSpitWaitFrames = 24;
    private const ushort KiHunterGroundAttackDistance = 96;

    private readonly KiHunterEnemyState?[] _kiHunterStates =
        new KiHunterEnemyState?[MaximumEnemyCount];

    /// <summary>Last library-two sound queued by an acid-spit instruction this frame.</summary>
    public ushort? LastKiHunterSoundEffect { get; private set; }

    internal static bool IsKiHunterBodyDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed;

    internal static bool IsKiHunterWingDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings;

    private static bool IsKiHunterDefinition(EnemyDefinitionId definition) =>
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
    private void InitializeKiHunterWings(RoomEnemySlot wings)
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
        if (MoveEnemyVertically(
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

        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X.RawValue))
        {
            state.Velocity = state.Velocity.WithWholeX(unchecked((short)-state.Velocity.X.WholePixels));
            SetKiHunterPairFacing(body, state.Velocity.X.WholePixels >= 0);
        }
        AlignEnemyYWithNonSquareSlope(level, body);

        ushort signedXDistance = unchecked((ushort)(samus.Position.X.Whole - body.Position.X.Whole));
        ushort absoluteXDistance = WrappedMagnitude(signedXDistance);
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
            ReadEightBitCosineProduct(state.Angle, state.SwoopRadius.X)));
        int horizontalDisplacement = unchecked((short)(targetX - body.Position.X.Whole)) << 16;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, horizontalDisplacement))
        {
            state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromParts(leftArc ? (short)1 : (short)-1, 0));
            StartKiHunterSwoopRecovery(state);
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
        ushort targetY = unchecked((ushort)(
            state.SwoopOriginY +
            ReadEightBitNegativeSineProduct(state.Angle, state.SwoopRadius.Y)));
        int verticalDisplacement = unchecked((short)(targetY - body.Position.Y.Whole)) << 16;
        if (MoveEnemyVertically(level, body, verticalDisplacement))
            StartKiHunterSwoopRecovery(state);
    }

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
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X.RawValue))
        {
            state.Function = KiHunterEnemyFunction.FlyingPatrol;
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
        if (MoveEnemyVertically(
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
        if (MoveEnemyVertically(
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
        ushort random = RequireRandomNumber();
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
        if (MoveEnemyVertically(
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

        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                body,
                state.Velocity.X.RawValue))
        {
            state.Velocity = state.Velocity.WithWholeX(unchecked((short)-state.Velocity.X.WholePixels));
            return;
        }

        AlignEnemyYWithNonSquareSlope(level, body);
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

        ushort distance = WrappedMagnitude(unchecked((ushort)(body.Position.X.Whole - samus.Position.X.Whole)));
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
    private static void RunDetachedKiHunterWingOrbit(RoomEnemySlot wings, KiHunterEnemyState state)
    {
        state.Angle = unchecked((ushort)(state.Angle + ReadKiHunterQuadraticAngleDelta(
            state.TargetXOrSpeedIndex,
            negativeHalf: true)));
        ushort radius = KiHunterMotionDefinitions.DetachedWingRadius;
        ushort byteAngle = SnesAngle.FromRaw(state.Angle).TableIndex;
        wings.Position = wings.Position.WithWhole(state.OrbitCenter + CircleOffset(byteAngle, radius) - state.OrbitOffset);

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
            ReadEightBitNegativeSineProduct(byteAngle, radius) -
            state.FallingArcOffset.Y));
        if (MoveEnemyVertically(
                level,
                wings,
                unchecked((short)(desiredY - wings.Position.Y.Whole)) << 16))
        {
            wings.Properties = wings.Properties.With(EnemyProperties.Deleted);
            wings.Position = wings.Position.WithWhole(state.SavedWingPosition);
            DecrementKiHunterDetachedSpeedIndex(state);
            return;
        }

        wings.Position = wings.Position.WithWholeX(unchecked((ushort)(
            state.OrbitCenter.X + ReadEightBitCosineProduct(byteAngle, radius) - state.FallingArcOffset.X)));
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
        state.OrbitOffset = CircleOffset(0xe0, radius);
        state.FallingArcOffset = CircleOffset(0xa0, radius);
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
        state.OrbitCenter = wings.Position.Whole;
    }

    private static void BeginDetachedKiHunterWingCollisionArc(
        RoomEnemySlot wings,
        KiHunterEnemyState state)
    {
        state.WingFunction = KiHunterWingFunction.FallingCollisionArc;
        state.TargetXOrSpeedIndex = state.DetachedSpeedReset;
        state.Angle = unchecked((ushort)-0x6000);
        state.OrbitCenter = wings.Position.Whole;
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

    private static void StartKiHunterGroundJumpFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundJump;

    private static void StartKiHunterGroundWaitFromInstruction(KiHunterEnemyState state) =>
        state.Function = KiHunterEnemyFunction.GroundWait;

    private void SpawnKiHunterAcidFromInstruction(RoomEnemySlot body, bool movingRight)
    {
        LastKiHunterSoundEffect = SoundEffectLibrary2Sounds.KiHunterAcidSpit.Value;
        QueueEnemySound(SoundEffectLibrary2Sounds.KiHunterAcidSpit, maximumQueued: 6);
        SpawnKiHunterAcidSpit(body, movingRight);
        RequireKiHunterState(body).WaitTimer = KiHunterSpitWaitFrames;
    }

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
            body.SlotIndex >= MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                $"Enemy slot {body.SlotIndex} is not a Ki-Hunter body with a following slot.");
        }

        RoomEnemySlot wings = _slots[body.SlotIndex + 1];
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

    /// <summary>The <c>$0000:E000</c> 16.16 downward acceleration added to a falling or hopping body each frame.</summary>
    private static SnesSignedSixteenSixteen KiHunterGravity => SnesSignedSixteenSixteen.FromParts(
        unchecked((short)KiHunterMotionDefinitions.GravityWhole),
        KiHunterMotionDefinitions.GravityFraction);

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
