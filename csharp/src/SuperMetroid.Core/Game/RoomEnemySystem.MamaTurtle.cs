using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Indirect function pointers stored in Mama Turtle's ordinary enemy variable A. Keeping the
/// literal bank-$A2 offsets makes debugger watches directly comparable with the cartridge.
/// </summary>
public enum MamaTurtleAiFunction : ushort
{
    Initial = 0x8dd8,
    Idle = 0x8e09,
    Asleep = 0x8e0a,
    LeavingShell = 0x8ee0,
    EnteringShell = 0x8f3f,
    RisingToHover = 0x8f8d,
    Hovering = 0x8feb,
    RisingToPeak = 0x9083,
    HoveringAtPeak = 0x90cc,
    Falling = 0x90e1,
}

/// <summary>Indirect function pointers stored in each Baby Turtle's variable A.</summary>
public enum BabyTurtleAiFunction : ushort
{
    CrawlingNotCarryingSamus = 0x9142,
    HidingCarryingSamus = 0x916e,
    HidingNotCarryingSamus = 0x9198,
    SpinningUnstoppable = 0x91f8,
    SpinningStoppable = 0x9239,
    CrawlingCarryingSamus = 0x925e,
}

/// <summary>
/// Typed view of Mama Turtle's ordinary variables and bank-$7E extension. The six ordinary
/// words remain backed by the physical enemy slot; the extension has no ordinary-slot alias
/// in the host and therefore lives in this one-to-one state object.
/// </summary>
public sealed class MamaTurtleEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MamaTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    public MamaTurtleAiFunction Function
    {
        get => (MamaTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Signed whole half of the horizontal 16.16 velocity at variable E.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Wake counter at variable F; each Baby touch or shot decrements it.</summary>
    public ushort AsleepFlag
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort FunctionTimer { get; internal set; }
    public ushort XSubAcceleration { get; internal set; }
    public ushort XAcceleration { get; internal set; }
    public ushort XSubVelocity { get; internal set; }
    public ushort YVelocity { get; internal set; }
    public ushort YSubVelocity { get; internal set; }

    /// <summary>
    /// Extended word six. Every Baby clears this through its main-AI prologue; a child in
    /// carrying state republishes its Y radius. This odd last-writer-wins behavior is native.
    /// </summary>
    public ushort CarryingChildHeight { get; internal set; }
}

/// <summary>Typed view of a Baby Turtle's ordinary variables and required extension words.</summary>
public sealed class BabyTurtleEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BabyTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    public BabyTurtleAiFunction Function
    {
        get => (BabyTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort ParentNativeIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort SpawnXPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort SpawnTopBoundary
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Signed whole-pixel horizontal velocity at ordinary variable E.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort FunctionTimer { get; internal set; }
    public ushort YVelocity { get; internal set; }
    public ushort NotCarryingSamusReactionTimer { get; internal set; }
}

/// <summary>
/// Literal translation of the coupled Mama Turtle / four Baby Turtle family at
/// <c>$A2:8B60-$94D8</c>. Their parent-slot references, shell contour, rider displacement,
/// wake counter, animation commands, movement, and custom combat callbacks are inseparable,
/// so the whole retail room family deliberately lives in one partial.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MamaTurtleDefinition = 0xcf3f;
    internal const ushort BabyTurtleDefinition = 0xcf7f;

    private const ushort MamaTurtleAsleepInstruction = 0x8c44;
    private const ushort MamaTurtleSpinningInstruction = 0x8c02;
    private const ushort MamaTurtleEnterShellLeftInstruction = 0x8c1c;
    private const ushort MamaTurtleLeaveShellLeftInstruction = 0x8c4a;
    private const ushort MamaTurtleEnterShellRightInstruction = 0x8d00;
    private const ushort MamaTurtleLeaveShellRightInstruction = 0x8d28;
    private const ushort BabyTurtleCrawlingLeftInstruction = 0x8b80;
    private const ushort BabyTurtleSpinningInstruction = 0x8bd2;
    private const ushort BabyTurtleHidingLeftInstruction = 0x8c30;
    private const ushort BabyTurtleLeaveShellLeftInstruction = 0x8c62;
    private const ushort BabyTurtleCrawlingRightInstruction = 0x8c72;
    private const ushort BabyTurtleHidingRightInstruction = 0x8d14;
    private const ushort BabyTurtleLeaveShellRightInstruction = 0x8d40;
    private const int SleepingMamaTurtleShellShape = 0xa28e80;

    private const ushort MamaTurtleSolidProperty = 0x8000;
    private const ushort BabyTurtleTravelDistance = 0x0030;
    private const ushort MamaTurtlePeakYPosition = 0x01e8;
    private const ushort MamaTurtleRisingSpeed = 7;
    private const ushort MamaTurtlePeakPauseFrames = 30;
    private const ushort MamaTurtleMaximumFallingSpeed = 4;
    private const ushort MamaTurtleMaximumHoveringSpeed = 3;
    private const ushort MamaTurtleSpinSound = 0x003a;
    private const ushort MamaTurtleWallSound = 0x001b;

    private readonly MamaTurtleEnemyState?[] _mamaTurtleStates =
        new MamaTurtleEnemyState?[MaximumEnemyCount];
    private readonly BabyTurtleEnemyState?[] _babyTurtleStates =
        new BabyTurtleEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<MamaTurtleEnemyState?> MamaTurtleStates => _mamaTurtleStates;
    public IReadOnlyList<BabyTurtleEnemyState?> BabyTurtleStates => _babyTurtleStates;

    /// <summary>Last library-two sound requested by this family during the enemy frame.</summary>
    public ushort? LastMamaTurtleSoundEffect { get; private set; }

    /// <summary>Ports Mama Turtle initialization AI <c>$A2:8D6C</c>.</summary>
    private void InitializeMamaTurtle(RoomEnemySlot slot)
    {
        var state = new MamaTurtleEnemyState(slot)
        {
            Function = MamaTurtleAiFunction.Initial,
            AsleepFlag = 1,
        };
        _mamaTurtleStates[slot.SlotIndex] = state;

        // The population already supplies $A800, but the initializer explicitly ORs $2000.
        // Spell that write out because instruction processing is essential to leaving shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.YRadius = 0;
        slot.CurrentInstruction = MamaTurtleAsleepInstruction;
    }

    /// <summary>Ports Baby Turtle initialization AI <c>$A2:8D9D</c>.</summary>
    private void InitializeBabyTurtle(RoomEnemySlot slot)
    {
        var state = new BabyTurtleEnemyState(slot)
        {
            SpawnXPosition = slot.XPosition,
            SpawnTopBoundary = unchecked((ushort)(slot.YPosition - slot.YRadius)),
            Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus,
            XVelocity = slot.Parameter1,
        };
        _babyTurtleStates[slot.SlotIndex] = state;

        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = unchecked((short)slot.Parameter1) < 0
            ? BabyTurtleCrawlingLeftInstruction
            : BabyTurtleCrawlingRightInstruction;
    }

    /// <summary>Ports Mama Turtle main AI <c>$A2:8DD2</c> and its indirect functions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemySlot slot,
        MamaTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort controllerInput,
        byte nmiFrameCounter8)
    {
        if (samus is null)
            throw new InvalidOperationException("Mama Turtle AI requires the active Samus actor.");

        switch (state.Function)
        {
            case MamaTurtleAiFunction.Initial:
                LinkMamaTurtleChildren(slot, state);
                return;
            case MamaTurtleAiFunction.Idle:
                return;
            case MamaTurtleAiFunction.Asleep:
                RunMamaTurtleAsleep(slot, state, samus);
                return;
            case MamaTurtleAiFunction.LeavingShell:
                RequireTurtleLevel(level);
                RunMamaTurtleLeavingShell(
                    slot, state, samus, level!, controllerInput, nmiFrameCounter8);
                return;
            case MamaTurtleAiFunction.EnteringShell:
                RunMamaTurtleEnteringShell(slot, state, samus);
                return;
            case MamaTurtleAiFunction.RisingToHover:
                RequireTurtleLevel(level);
                RunMamaTurtleRisingToHover(slot, state, samus, level!, controllerInput);
                return;
            case MamaTurtleAiFunction.Hovering:
                RequireTurtleLevel(level);
                RunMamaTurtleHovering(slot, state, samus, level!, controllerInput);
                return;
            case MamaTurtleAiFunction.RisingToPeak:
                RunMamaTurtleRisingToPeak(slot, state, samus, controllerInput);
                return;
            case MamaTurtleAiFunction.HoveringAtPeak:
                RunMamaTurtleHoveringAtPeak(slot, state, samus, controllerInput);
                return;
            case MamaTurtleAiFunction.Falling:
                RequireTurtleLevel(level);
                RunMamaTurtleFalling(slot, state, samus, level!, controllerInput);
                return;
            default:
                throw new NotSupportedException(
                    $"Mama Turtle main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the first-frame parent/child setup at <c>$A2:8DD8</c>.</summary>
    private void LinkMamaTurtleChildren(RoomEnemySlot mama, MamaTurtleEnemyState state)
    {
        if (mama.SlotIndex + 4 >= MaximumEnemyCount)
            throw new InvalidDataException("Mama Turtle does not have four following physical slots.");

        for (int childOffset = 1; childOffset <= 4; childOffset++)
        {
            RoomEnemySlot child = _slots[mama.SlotIndex + childOffset];
            if (child.EnemyDefinitionPointer != BabyTurtleDefinition)
            {
                throw new InvalidDataException(
                    $"Mama Turtle slot {mama.SlotIndex} expected Baby Turtle in following " +
                    $"slot {child.SlotIndex}, found ${child.EnemyDefinitionPointer:X4}.");
            }

            BabyTurtleEnemyState childState = RequireBabyTurtleState(child);
            child.PaletteIndex = mama.PaletteIndex;
            child.VramTilesIndex = mama.VramTilesIndex;
            childState.ParentNativeIndex = mama.NativeIndex;
        }

        state.Function = MamaTurtleAiFunction.Asleep;
    }

    /// <summary>Ports the sleeping shell height/carry routine at <c>$A2:8E0A</c>.</summary>
    private void RunMamaTurtleAsleep(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (state.AsleepFlag == 0)
        {
            state.Function = MamaTurtleAiFunction.LeavingShell;
            mama.Properties = mama.Properties.Without(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        mama.YRadius = 0;
        short signedDifference = unchecked((short)(mama.XPosition - samus.XPosition));
        int distance = Math.Abs((int)signedDifference);
        if (distance >= 24)
            return;

        // The first 24 contour words describe Samus to the left; the second 24 describe
        // Samus to the right. This asymmetry is visible around the sleeping shell's lip.
        int contourIndex = signedDifference < 0 ? distance + 24 : distance;
        short contourOffset = unchecked((short)ReadWord(
            _bus!, SleepingMamaTurtleShellShape + contourIndex * 2));
        mama.YRadius = unchecked((ushort)-contourOffset);
        mama.Properties = unchecked((ushort)(mama.Properties | MamaTurtleSolidProperty));

        if (!IsSamusRidingPlatform(mama, samus))
            return;

        ushort shellTop = unchecked((ushort)(mama.YPosition - mama.YRadius));
        ushort overlap = unchecked((ushort)(samus.Kinematics.BottomBoundary - shellTop));
        if (unchecked((short)overlap) < 0)
            return;

        samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
            samus.Kinematics.ExtraYDisplacement - overlap));
    }

    /// <summary>Ports the deliberately jittery shell-opening routine at <c>$A2:8EE0</c>.</summary>
    private void RunMamaTurtleLeavingShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput,
        byte nmiFrameCounter8)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);
        if ((nmiFrameCounter8 & 1) != 0)
            return;

        if (IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraXDisplacement - 1));
        }

        mama.YPosition = unchecked((ushort)(mama.YPosition - 1));
        mama.YRadius = 16;
        mama.XPosition = (mama.YPosition & 1) == 0
            ? unchecked((ushort)(mama.XPosition + 1))
            : unchecked((ushort)(mama.XPosition - 1));

        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, 1 << 16))
            return;

        InstallTurtleInstruction(mama, MamaTurtleLeaveShellLeftInstruction);

        // $8F35 is `STA $0006,x`, not `STA Enemy+$0006,x`. It corrupts low WRAM with $20
        // in retail and does not alter the enemy. Reproducing unrelated low-WRAM corruption
        // would be harmful; importantly, we do not invent an enemy-field meaning for it.
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports the facing-dependent return-to-shell selector at <c>$A2:8F3F</c>.</summary>
    private static void RunMamaTurtleEnteringShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        InstallTurtleInstruction(
            mama,
            unchecked((short)(mama.XPosition - samus.XPosition)) >= 0
                ? MamaTurtleEnterShellLeftInstruction
                : MamaTurtleEnterShellRightInstruction);
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports the 16-frame upward transition at <c>$A2:8F8D</c>.</summary>
    private void RunMamaTurtleRisingToHover(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);
        if (MoveEnemyVertically(level, mama, -1 << 16))
            return;

        if (IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraYDisplacement - 1));
        }

        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if (state.FunctionTimer != 0)
            return;

        // The two adjacent {sub,whole} pairs at $8D56 accelerate toward Samus's side.
        if (unchecked((short)(mama.XPosition - samus.XPosition)) < 0)
        {
            state.XSubAcceleration = 0x1000;
            state.XAcceleration = 0;
        }
        else
        {
            state.XSubAcceleration = 0xf000;
            state.XAcceleration = 0xffff;
        }
        state.XVelocity = 0;
        state.XSubVelocity = 0;
        state.Function = MamaTurtleAiFunction.Hovering;
    }

    /// <summary>Ports accelerating horizontal hover and its original wall bug at $A2:8FEB.</summary>
    private void RunMamaTurtleHovering(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);
        int displacement = ((int)(short)state.XVelocity << 16) | state.XSubVelocity;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, displacement))
        {
            ReverseMamaTurtleAtWall(state);
            EarthquakeType = 0;
            EarthquakeTimer = 16;
            LastMamaTurtleSoundEffect = MamaTurtleWallSound;
            return;
        }

        HandleSamusLandingOnMamaTurtle(mama, state, samus);
        AddAndClampMamaTurtleHorizontalAcceleration(state);
    }

    /// <summary>Ports <c>HandleSamusLandingOnHoveringTatori</c> at $A2:8F5F.</summary>
    private static void HandleSamusLandingOnMamaTurtle(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (!IsSamusRidingPlatform(mama, samus))
            return;

        state.Function = MamaTurtleAiFunction.RisingToPeak;
        uint samusExtra = ((uint)samus.Kinematics.ExtraXDisplacement << 16) |
            samus.Kinematics.ExtraXSubdisplacement;
        uint turtleVelocity = ((uint)state.XVelocity << 16) | state.XSubVelocity;
        uint result = unchecked(samusExtra - turtleVelocity);
        samus.Kinematics.ExtraXSubdisplacement = unchecked((ushort)result);
        ushort whole = unchecked((ushort)(result >> 16));
        samus.Kinematics.ExtraXDisplacement = unchecked((short)whole) < -16
            ? (ushort)0xfff0
            : whole;
    }

    private static void AddAndClampMamaTurtleHorizontalAcceleration(
        MamaTurtleEnemyState state)
    {
        uint subSum = (uint)state.XSubVelocity + state.XSubAcceleration;
        ushort oldWhole = state.XVelocity;
        ushort newWhole = unchecked((ushort)(
            oldWhole + state.XAcceleration + (subSum >> 16)));
        state.XSubVelocity = unchecked((ushort)subSum);

        int magnitude = Math.Abs((int)unchecked((short)newWhole));
        state.XVelocity = magnitude < MamaTurtleMaximumHoveringSpeed
            ? newWhole
            // Native chooses the cap's sign from the old velocity word, before storing the
            // just-computed result. Preserve that one-frame zero-crossing behavior.
            : unchecked((short)oldWhole) < 0
                ? unchecked((ushort)-MamaTurtleMaximumHoveringSpeed)
                : MamaTurtleMaximumHoveringSpeed;
    }

    /// <summary>
    /// Reproduces $903C-$906B, including carry propagation across two otherwise separate
    /// negations and the missing final INC on the whole acceleration word.
    /// </summary>
    private static void ReverseMamaTurtleAtWall(MamaTurtleEnemyState state)
    {
        state.XSubVelocity = unchecked((ushort)(~state.XSubVelocity + 1));

        uint xVelocitySum = (uint)state.XVelocity + 1; // BCS supplied carry-in one.
        bool carry = xVelocitySum > ushort.MaxValue;
        state.XVelocity = unchecked((ushort)(~(ushort)xVelocitySum + 1));

        state.XSubAcceleration = unchecked((ushort)(~state.XSubAcceleration + 1));
        uint accelerationSum = (uint)state.XAcceleration + (carry ? 1u : 0u);
        state.XAcceleration = unchecked((ushort)~(ushort)accelerationSum); // No INC in ROM.
    }

    /// <summary>Ports the rider-powered seven-pixel ascent at <c>$A2:9083</c>.</summary>
    private void RunMamaTurtleRisingToPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);
        if (unchecked((short)(mama.YPosition - MamaTurtlePeakYPosition)) < 0)
        {
            state.FunctionTimer = MamaTurtlePeakPauseFrames;
            state.Function = MamaTurtleAiFunction.HoveringAtPeak;
            state.YSubVelocity = 0;
            state.YVelocity = 0;
            return;
        }

        if (IsSamusRidingPlatform(mama, samus))
        {
            mama.YPosition = unchecked((ushort)(mama.YPosition - MamaTurtleRisingSpeed));
            samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraYDisplacement - MamaTurtleRisingSpeed));
            return;
        }

        state.Function = MamaTurtleAiFunction.Falling;
        state.YSubVelocity = 0;
        state.YVelocity = 0;
    }

    /// <summary>Ports the 30-frame peak delay at <c>$A2:90CC</c>.</summary>
    private void RunMamaTurtleHoveringAtPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if (state.FunctionTimer == 0)
            state.Function = MamaTurtleAiFunction.Falling;
    }

    /// <summary>Ports gravity, collision, and landing-list selection at <c>$A2:90E1</c>.</summary>
    private void RunMamaTurtleFalling(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, controllerInput);

        // The original cap comparison accidentally omits X and reads slot-zero's extension.
        // The retail parent is slot zero, but retaining the explicit source documents the
        // behavior for debugger-created populations as well.
        MamaTurtleEnemyState comparisonState = _mamaTurtleStates[0] ?? state;
        if (unchecked((short)(comparisonState.YVelocity - MamaTurtleMaximumFallingSpeed)) < 0)
        {
            uint sum = (uint)state.YSubVelocity + 0x2000;
            state.YSubVelocity = unchecked((ushort)sum);
            state.YVelocity = unchecked((ushort)(state.YVelocity + (sum >> 16)));
        }

        // Only the whole word is passed to MoveEnemyDown; YSubVelocity influences later
        // whole-speed carries but is not itself part of this frame's displacement.
        if (!MoveEnemyVertically(level, mama, (int)(short)state.YVelocity << 16))
            return;

        InstallTurtleInstruction(
            mama,
            unchecked((short)state.XVelocity) < 0
                ? MamaTurtleLeaveShellLeftInstruction
                : MamaTurtleLeaveShellRightInstruction);
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports Baby Turtle main AI <c>$A2:912E</c> and its six live functions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle AI requires the active Samus actor.");

        MamaTurtleEnemyState parent = RequireMamaTurtleParent(state);
        parent.CarryingChildHeight = 0;
        switch (state.Function)
        {
            case BabyTurtleAiFunction.CrawlingNotCarryingSamus:
                if (IsSamusRidingPlatform(baby, samus))
                {
                    state.Function = BabyTurtleAiFunction.HidingCarryingSamus;
                    state.NotCarryingSamusReactionTimer = 4;
                    InstallTurtleInstruction(
                        baby,
                        unchecked((short)state.XVelocity) < 0
                            ? BabyTurtleHidingLeftInstruction
                            : BabyTurtleHidingRightInstruction);
                }
                return;

            case BabyTurtleAiFunction.HidingCarryingSamus:
                if (IsSamusRidingPlatform(baby, samus))
                {
                    state.NotCarryingSamusReactionTimer = 4;
                    return;
                }
                state.NotCarryingSamusReactionTimer = unchecked((ushort)(
                    state.NotCarryingSamusReactionTimer - 1));
                if (state.NotCarryingSamusReactionTimer == 0)
                {
                    state.Function = BabyTurtleAiFunction.HidingNotCarryingSamus;
                    state.FunctionTimer = 60;
                }
                return;

            case BabyTurtleAiFunction.HidingNotCarryingSamus:
                if (IsSamusRidingPlatform(baby, samus))
                {
                    state.Function = BabyTurtleAiFunction.SpinningUnstoppable;
                    InstallTurtleInstruction(baby, BabyTurtleSpinningInstruction);
                    state.YVelocity = 1;
                    state.XVelocity = (samus.ReadPoseXDirection(_bus!) & 0x0f) == 8
                        ? (ushort)3
                        : unchecked((ushort)-3);
                    return;
                }
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if (state.FunctionTimer == 0)
                {
                    InstallBabyTurtleCrawlingInstruction(baby, state);
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                }
                return;

            case BabyTurtleAiFunction.SpinningUnstoppable:
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level!);
                return;

            case BabyTurtleAiFunction.SpinningStoppable:
                if (IsSamusRidingPlatform(baby, samus))
                {
                    InstallBabyTurtleCrawlingInstruction(baby, state);
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                    return;
                }
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level!);
                return;

            case BabyTurtleAiFunction.CrawlingCarryingSamus:
                parent.CarryingChildHeight = baby.YRadius;
                if (!IsSamusRidingPlatform(baby, samus))
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                return;

            default:
                throw new NotSupportedException(
                    $"Baby Turtle main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void MoveSpinningBabyTurtle(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        RoomLevelData level)
    {
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level, baby, (int)(short)state.XVelocity << 16))
        {
            state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
            return;
        }
        MoveEnemyVertically(level, baby, (int)(short)state.YVelocity << 16);
    }

    /// <summary>Instruction <c>$A2:9381</c>: crawl across the sleeping parent contour.</summary>
    private void ProcessBabyTurtleCrawlInstruction(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null || level is null)
        {
            throw new InvalidOperationException(
                "Baby Turtle crawl bytecode requires Samus and active room collision data.");
        }

        bool carryingSamus = IsSamusRidingPlatform(baby, samus);
        if (carryingSamus)
        {
            samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraXDisplacement + state.XVelocity));
        }

        ushort previousY = baby.YPosition;
        baby.YPosition = state.SpawnTopBoundary;
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, (int)(short)state.XVelocity << 16);

        RoomEnemySlot parentSlot = SlotFromNativeIndex(state.ParentNativeIndex);
        MamaTurtleEnemyState parent = RequireMamaTurtleState(parentSlot);
        if (parent.Function != MamaTurtleAiFunction.Asleep)
            return;

        short signedDifference = unchecked((short)(parentSlot.XPosition - baby.XPosition));
        int distance = Math.Abs((int)signedDifference);
        short contourOffset = distance >= 24
            ? (short)1
            : unchecked((short)ReadWord(
                _bus!,
                SleepingMamaTurtleShellShape +
                (signedDifference < 0 ? distance + 24 : distance) * 2));
        MoveEnemyVertically(level, baby, contourOffset << 16);

        if (carryingSamus)
        {
            samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraYDisplacement + baby.YPosition - previousY));
        }
    }

    /// <summary>Instruction <c>$A2:9412</c>: turn around at 48 pixels from spawn.</summary>
    private static ushort SelectBabyTurtleCrawlLoop(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state)
    {
        short signedDifference = unchecked((short)(state.SpawnXPosition - baby.XPosition));
        int distance = Math.Abs((int)signedDifference);
        if (distance >= BabyTurtleTravelDistance)
        {
            state.XVelocity = signedDifference < 0
                ? unchecked((ushort)-1)
                : (ushort)1;
        }
        return unchecked((short)state.XVelocity) < 0
            ? BabyTurtleCrawlingLeftInstruction
            : BabyTurtleCrawlingRightInstruction;
    }

    /// <summary>Instruction <c>$A2:9447</c>: begin Mama's shell-entry behavior.</summary>
    private static void StartMamaTurtleEnteringShell(MamaTurtleEnemyState state) =>
        state.Function = MamaTurtleAiFunction.EnteringShell;

    /// <summary>Instructions <c>$9451/$946B</c>: begin rising with selected X direction.</summary>
    private static void StartMamaTurtleRisingToHover(
        MamaTurtleEnemyState state,
        bool rightward)
    {
        // Cartridge comments describe the visual facing, while the stored velocity is the
        // opposite sign: $9451 writes -1 and $946B writes +1.
        state.Function = MamaTurtleAiFunction.RisingToHover;
        state.XVelocity = rightward ? unchecked((ushort)-1) : (ushort)1;
        state.FunctionTimer = 16;
    }

    /// <summary>Instruction <c>$A2:9485</c>: branch out of hiding only while ridden.</summary>
    private static ushort SelectBabyTurtleLeaveShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        ushort fallthroughCursor)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        if (!IsSamusRidingPlatform(baby, samus))
            return fallthroughCursor;
        return unchecked((short)state.XVelocity) < 0
            ? BabyTurtleLeaveShellLeftInstruction
            : BabyTurtleLeaveShellRightInstruction;
    }

    /// <summary>Instruction <c>$A2:94A1</c>: restore crawling after leaving the shell.</summary>
    private static ushort FinishBabyTurtleLeavingShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        state.Function = IsSamusRidingPlatform(baby, samus)
            ? BabyTurtleAiFunction.CrawlingCarryingSamus
            : BabyTurtleAiFunction.CrawlingNotCarryingSamus;
        return unchecked((short)state.XVelocity) < 0
            ? BabyTurtleCrawlingLeftInstruction
            : BabyTurtleCrawlingRightInstruction;
    }

    /// <summary>Ports Mama's custom ordinary-touch callback at <c>$A2:9281</c>.</summary>
    private void ResolveMamaTurtleTouch(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        if ((mama.Properties & MamaTurtleSolidProperty) != 0)
            return;
        ResolveNormalEnemyTouch(mama, samus, controllerInput);
        state.Function = MamaTurtleAiFunction.Falling;
        state.YVelocity = 2;
    }

    /// <summary>Ports Baby's direction flip, separation, and wake callback at $A2:929F.</summary>
    private void ResolveBabyTurtleTouch(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData? level)
    {
        if (state.Function == BabyTurtleAiFunction.CrawlingCarryingSamus)
            return;
        RequireTurtleLevel(level);

        if (unchecked((short)state.XVelocity) < 0)
        {
            InstallTurtleInstruction(baby, BabyTurtleCrawlingRightInstruction);
            state.XVelocity = 1;
        }
        else
        {
            InstallTurtleInstruction(baby, BabyTurtleCrawlingLeftInstruction);
            state.XVelocity = unchecked((ushort)-1);
        }

        baby.XPosition = unchecked((short)(baby.XPosition - samus.XPosition)) >= 0
            ? unchecked((ushort)(samus.XPosition + samus.Kinematics.XRadius + baby.XRadius))
            : unchecked((ushort)(samus.XPosition - samus.Kinematics.XRadius - baby.XRadius));
        state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level!, baby, (int)(short)state.XVelocity << 16);
        WakeMamaTurtle(state);
    }

    /// <summary>Private tail of Baby shot AI <c>$A2:930F</c>.</summary>
    private void ResolveBabyTurtleShotAfterCommon(BabyTurtleEnemyState state) =>
        WakeMamaTurtle(state);

    private void WakeMamaTurtle(BabyTurtleEnemyState child)
    {
        MamaTurtleEnemyState parent = RequireMamaTurtleParent(child);
        if (parent.AsleepFlag != 0)
            parent.AsleepFlag = unchecked((ushort)(parent.AsleepFlag - 1));
    }

    /// <summary>Ports Mama's expanded private damage box at <c>$A2:9315</c>.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemySlot mama,
        SamusState samus,
        ushort controllerInput)
    {
        // Four bounds deliberately come from physical slot zero because all four native
        // loads omit `,x`. The damage callback still runs for the current Mama slot.
        RoomEnemySlot boundsSource = _slots[0];
        ushort left = unchecked((ushort)(
            boundsSource.XPosition - boundsSource.XRadius - 8));
        ushort right = unchecked((ushort)(
            boundsSource.XPosition + boundsSource.XRadius + 8));
        ushort top = unchecked((ushort)(
            boundsSource.YPosition - boundsSource.YRadius + 4));
        ushort bottom = unchecked((ushort)(
            boundsSource.YPosition + boundsSource.YRadius - 4));

        ushort samusLeftMinusOne = unchecked((ushort)(
            samus.XPosition - samus.Kinematics.XRadius - 1));
        if (unchecked((short)(samusLeftMinusOne - right)) >= 0)
            return;
        ushort samusRight = unchecked((ushort)(
            samus.XPosition + samus.Kinematics.XRadius));
        if (unchecked((short)(samusRight - left)) < 0)
            return;
        ushort samusTopPlusOne = unchecked((ushort)(
            samus.YPosition - samus.Kinematics.YRadius + 1));
        if (unchecked((short)(samusTopPlusOne - bottom)) >= 0)
            return;
        ushort samusBottom = unchecked((ushort)(
            samus.YPosition + samus.Kinematics.YRadius));
        if (unchecked((short)(samusBottom - top)) < 0 || samus.InvincibilityTimer != 0)
            return;

        ResolveNormalEnemyTouch(mama, samus, controllerInput);
    }

    private MamaTurtleEnemyState RequireMamaTurtleParent(BabyTurtleEnemyState child)
    {
        RoomEnemySlot parent = SlotFromNativeIndex(child.ParentNativeIndex);
        if (parent.EnemyDefinitionPointer != MamaTurtleDefinition)
        {
            throw new InvalidDataException(
                $"Baby Turtle parent index ${child.ParentNativeIndex:X4} names enemy " +
                $"${parent.EnemyDefinitionPointer:X4}, not Mama Turtle.");
        }
        return RequireMamaTurtleState(parent);
    }

    private MamaTurtleEnemyState RequireMamaTurtleState(RoomEnemySlot slot) =>
        _mamaTurtleStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Mama Turtle state.");

    private BabyTurtleEnemyState RequireBabyTurtleState(RoomEnemySlot slot) =>
        _babyTurtleStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Baby Turtle state.");

    private static void InstallBabyTurtleCrawlingInstruction(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state) =>
        InstallTurtleInstruction(
            baby,
            unchecked((short)state.XVelocity) < 0
                ? BabyTurtleCrawlingLeftInstruction
                : BabyTurtleCrawlingRightInstruction);

    private static void InstallTurtleInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
    }

    private static void RequireTurtleLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Turtle movement requires active room collision data.");
    }
}
