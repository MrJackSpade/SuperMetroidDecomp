namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A2 dispatcher values used by enemy $D4FF. The names describe the four
/// initialization waits and two growth directions without discarding the literal ROM
/// addresses a debugger needs when comparing this port with enemy variable A in WRAM.
/// </summary>
public enum GrowingShutterFunction : ushort
{
    /// <summary><c>$A2:EABD Function_ShutterGrowing_Initial_Upwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts upward growth.</summary>
    WaitToGrowUpForTimer = 0xeabd,
    /// <summary><c>$A2:EAD1 Func_ShutterGrowing_Initial_Upwards_WaitForSamusToGetNear</c>: starts upward growth when Samus's absolute horizontal distance is strictly below parameter 1.</summary>
    WaitToGrowUpForProximity = 0xead1,
    /// <summary><c>$A2:EAE7 Func_ShutterGrowing_Initial_Downwards_WaitForSamusToGetNear</c>: starts downward growth at the strict parameter-1 horizontal proximity threshold.</summary>
    WaitToGrowDownForProximity = 0xeae7,
    /// <summary><c>$A2:EAFD Function_ShutterGrowing_Initial_Downwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts downward growth.</summary>
    WaitToGrowDownForTimer = 0xeafd,
    /// <summary><c>$A2:EB11 Function_ShutterGrowing_Growing_Downwards</c>: advances four growth sections with positive fixed-point speed, then remains at growth level 4.</summary>
    GrowDown = 0xeb11,
    /// <summary><c>$A2:EC13 Function_ShutterGrowing_Growing_Upwards</c>: subtracts fixed-point growth speed and carries a riding Samus by the whole-pixel upward delta.</summary>
    GrowUp = 0xec13,
}

/// <summary>Bank-$A2 dispatcher values shared by the three vertical shutter definitions.</summary>
public enum VerticalShutterFunction : ushort
{
    /// <summary><c>$A2:EF09 Function_Shutter_Kamer_Initial</c>: calls the trigger-mode table entry during this same AI update until activation replaces the function.</summary>
    Initial = 0xef09,
    /// <summary><c>$A2:EF15 Function_Shutter_Kamer_WaitForTimer</c>: decrements the initial timer and activates on zero, restoring the configured wait value.</summary>
    WaitForTimer = 0xef15,
    /// <summary><c>$A2:EF28 Function_Shutter_Kamer_WaitForSamusToGetNear</c>: waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity = 0xef28,
    /// <summary><c>$A2:EF39 Function_Shutter_Kamer_Activate</c>: selects primary upward/downward movement and queues the activation sound when on screen.</summary>
    Activate = 0xef39,
    /// <summary><c>$A2:EF40 Function_Shutter_Kamer_GetEnemyIndex</c>: initial inert function for the one-shot and repeatable reaction-triggered modes.</summary>
    InitialNoOp = 0xef40,
    /// <summary><c>$A2:EF68 Function_Kamer_MovingUp</c>: adds upward 16.16 velocity, carries a rider, and stops at the minimum Y boundary.</summary>
    MovingUp = 0xef68,
    /// <summary><c>$A2:EFD4 Function_Kamer_MovingDown</c>: adds downward 16.16 velocity, carries a rider, and stops at the maximum Y boundary.</summary>
    MovingDown = 0xefd4,
    /// <summary><c>$A2:F040 Function_Kamer_StoppedMovingUp</c>: waits for signed timer underflow before moving down or restoring proximity waiting.</summary>
    StoppedAfterMovingUp = 0xf040,
    /// <summary><c>$A2:F072 Function_Kamer_StoppedMovingDown</c>: waits for signed timer underflow before moving up or restoring proximity waiting.</summary>
    StoppedAfterMovingDown = 0xf072,
    /// <summary><c>$A2:F099 Function_Shutter_Kamer_GetEnemyIndex_duplicate</c>: inert endpoint selected by rest time $0FF0; eligible reaction modes can still reactivate it.</summary>
    PermanentNoOp = 0xf099,
}

/// <summary>Bank-$A2 dispatcher values used by the unused horizontal shutter $D57F.</summary>
public enum HorizontalShutterFunction : ushort
{
    /// <summary><c>$A2:F224 Function_HorizontalShutter_Initial</c>: installs the trigger-mode table entry for the following AI update.</summary>
    Initial = 0xf224,
    /// <summary><c>$A2:F230 Function_HorizontalShutter_Initial_WaitForTimer</c>: decrements the initial timer and activates when it becomes zero.</summary>
    WaitForTimer = 0xf230,
    /// <summary><c>$A2:F243 Function_HorizontalShutter_Initial_WaitForSamusToGetNear</c>: waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity = 0xf243,
    /// <summary><c>$A2:F254 Function_HorizontalShutter_Initial_Activate</c>: selects left or right movement from the primary direction.</summary>
    Activate = 0xf254,
    /// <summary><c>$A2:F25B Function_HorizontalShutter_Initial_Nothing</c>: holds still until an eligible one-shot or repeatable reaction.</summary>
    InitialNoOp = 0xf25b,
    /// <summary><c>$A2:F272 Function_HorizontalShutter_MovingLeft</c>: adds leftward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at minimum X.</summary>
    MovingLeft = 0xf272,
    /// <summary><c>$A2:F2E4 Function_HorizontalShutter_MovingRight</c>: adds rightward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at maximum X.</summary>
    MovingRight = 0xf2e4,
    /// <summary><c>$A2:F38C Function_HorizontalShutter_StoppedMovingLeft</c>: waits for signed timer underflow before moving right or restoring proximity waiting.</summary>
    StoppedAfterMovingLeft = 0xf38c,
    /// <summary><c>$A2:F3B0 Function_HorizontalShutter_StoppedMovingRight</c>: waits for signed timer underflow before moving left or restoring proximity waiting.</summary>
    StoppedAfterMovingRight = 0xf3b0,
    /// <summary><c>$A2:F3D4 RTS_A2F3D4</c>: inert endpoint for rest time $0FF0, retaining stationary manual ejection and eligible reaction activation.</summary>
    PermanentNoOp = 0xf3d4,
}

/// <summary>
/// Typed view of growing-shutter enemy variables and its two extended movement words.
/// Origin words B-E are the four independently positioned 16-pixel growth sections.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Common enemy slot whose variables store section origins and growth level.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a growth-state view over one shutter's common enemy variables.</summary>
    /// <param name="slot">Enemy slot owned by the growing shutter.</param>
    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer; initialization selects a wait function and activation replaces it with a growth function.</summary>
    public GrowingShutterFunction Function
    {
        get => (GrowingShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: the first section's whole-pixel Y origin, initialized to the spawn position.</summary>
    public ushort GrowthLevel0OriginY { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Variable C: the second section's Y origin, eight pixels above or below the spawn position according to growth direction.</summary>
    public ushort GrowthLevel1OriginY { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: the third section's Y origin, sixteen pixels above or below the spawn position, wrapping as a native word.</summary>
    public ushort GrowthLevel2OriginY { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    /// <summary>Variable E: the fourth section's Y origin, twenty-four pixels above or below the spawn position, wrapping as a native word.</summary>
    public ushort GrowthLevel3OriginY { get => _slot.VariableE; internal set => _slot.VariableE = value; }

    /// <summary>Variable F; zero through four, where four is the fully grown terminal state.</summary>
    public ushort GrowthLevel { get => _slot.VariableF; internal set => _slot.VariableF = value; }

    /// <summary>Signed whole word of the positive 16.16 growth speed.</summary>
    public short GrowthVelocity { get; internal set; }

    /// <summary>Fraction word of the positive 16.16 growth speed.</summary>
    public ushort GrowthSubvelocity { get; internal set; }

    /// <summary>Extended word $7E:8800,x used to derive Samus's whole-pixel carry.</summary>
    public ushort PreviousYPosition { get; internal set; }

    /// <summary>Returns the saved Y origin for the active section; level four is terminal.</summary>
    internal ushort OriginForLevel() => GrowthLevel switch
    {
        0 => GrowthLevel0OriginY,
        1 => GrowthLevel1OriginY,
        2 => GrowthLevel2OriginY,
        3 => GrowthLevel3OriginY,
        _ => throw new InvalidOperationException(
            $"Growing shutter level {GrowthLevel} has no section origin."),
    };
}

/// <summary>
/// Typed host representation of the vertical shutter's ordinary and extended enemy words.
/// Values not backed by variables A-F correspond one-for-one with the native $7800/$8800
/// arrays; keeping them explicit avoids another opaque bag of numbered scratch fields.
/// </summary>
public sealed class VerticalShutterEnemyState
{
    /// <summary>Common enemy slot holding the vertical shutter's dispatcher and velocity words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a movement-state view over one vertical shutter's common variables.</summary>
    /// <param name="slot">Enemy slot whose configuration and motion this state represents.</param>
    internal VerticalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer, shared by shootable/destroyable shutters and the Kamer vertical platform.</summary>
    public VerticalShutterFunction Function
    {
        get => (VerticalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: wrapping AI-update countdown; initial waiting activates at zero, while endpoint resting resumes on signed underflow.</summary>
    public ushort FunctionTimer { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Variable C: low word of downward 16.16 pixels-per-update velocity, whose carry contributes to the whole-pixel movement.</summary>
    public ushort DownSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: signed high word of downward 16.16 pixels-per-update velocity from the common linear-speed table.</summary>
    public short DownVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    /// <summary>Variable E: low word of the signed upward 16.16 velocity; it is added with the negative whole word, not independently negated.</summary>
    public ushort UpSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F: signed high word of upward 16.16 pixels-per-update velocity.</summary>
    public short UpVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    /// <summary>Low spawn instruction byte selecting an eight-byte common speed record containing downward and upward velocity pairs.</summary>
    public ushort SpeedTableIndex { get; internal set; }
    /// <summary>High spawn instruction byte: zero starts upward; nonzero starts downward.</summary>
    public ushort PrimaryDirection { get; internal set; }
    /// <summary>Low extra-properties byte specifying the upward endpoint's rest time in units of sixteen AI updates; $FF selects the inert endpoint.</summary>
    public ushort MovedUpRestParameter { get; internal set; }
    /// <summary>High extra-properties byte specifying the downward endpoint's rest time in units of sixteen AI updates; $FF selects the inert endpoint.</summary>
    public ushort MovedDownRestParameter { get; internal set; }
    /// <summary>Low parameter-1 byte: 0 timer, 1 proximity, 2 immediate, 3 one-shot reaction, or 4 repeatable reaction activation.</summary>
    public ushort TriggerMode { get; internal set; }
    /// <summary>High parameter-1 byte giving whole-pixel separation of the two Y endpoints.</summary>
    public ushort TravelDistance { get; internal set; }
    /// <summary>Parameter 2: initial AI-update wait in timer mode, or strict absolute-X pixel-distance threshold in proximity mode.</summary>
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode: byte offset 0, 2, 4, 6, or 8 into the native initial-function pointer table.</summary>
    public ushort InitialFunctionTableOffset { get; internal set; }
    /// <summary>Upward endpoint countdown loaded from the rest parameter shifted left four; $0FF0 selects the inert function instead of counting down.</summary>
    public ushort MovedUpRestTime { get; internal set; }
    /// <summary>Downward endpoint countdown loaded from the rest parameter shifted left four; $0FF0 selects the inert function instead of counting down.</summary>
    public ushort MovedDownRestTime { get; internal set; }
    /// <summary>Whether the pre-movement riding test found Samus supported by this shutter; enables whole-pixel extra-Y carry for the current movement.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>One-shot reaction latch, initially clear and set by the first mode-3 reaction so later reactions cannot activate movement again.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel Y saved before its current move to derive the rider's wrapped displacement.</summary>
    public ushort PreviousYPosition { get; internal set; }
    /// <summary>Upper whole-pixel travel boundary derived from spawn Y and travel distance; upward movement stops upon reaching or passing it.</summary>
    public ushort MinimumYPosition { get; internal set; }
    /// <summary>Lower whole-pixel travel boundary derived from spawn Y and travel distance; downward movement stops upon reaching or passing it.</summary>
    public ushort MaximumYPosition { get; internal set; }
    /// <summary>Next reaction's direction selector, initially the primary direction: zero moves up, nonzero moves down, then bit zero is toggled.</summary>
    public ushort ReactionDirection { get; internal set; }
}

/// <summary>Typed state for the cartridge's complete, though retail-unused, horizontal shutter.</summary>
public sealed class HorizontalShutterEnemyState
{
    /// <summary>Common enemy slot holding the horizontal shutter's dispatcher and velocity words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a movement-state view over one horizontal shutter's common variables.</summary>
    /// <param name="slot">Enemy slot whose configuration and motion this state represents.</param>
    internal HorizontalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer for the retail-unused horizontal shutter, retained across wait, movement, and endpoint-rest updates.</summary>
    public HorizontalShutterFunction Function
    {
        get => (HorizontalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: wrapping AI-update countdown; initial wait tests for zero after decrement, whereas endpoint rests test for signed underflow.</summary>
    public ushort FunctionTimer { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    /// <summary>Variable C: low word of rightward 16.16 pixels-per-update velocity, also supplied to an overlapping Samus's extra-X displacement.</summary>
    public ushort RightSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    /// <summary>Variable D: signed whole word of rightward 16.16 pixels-per-update velocity.</summary>
    public short RightVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    /// <summary>Variable E: fractional low word of signed leftward 16.16 pixels-per-update velocity.</summary>
    public ushort LeftSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    /// <summary>Variable F: signed whole word of leftward 16.16 pixels-per-update velocity.</summary>
    public short LeftVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    /// <summary>Low spawn instruction byte selecting an eight-byte common speed record, used for the rightward and leftward velocity pairs.</summary>
    public ushort SpeedTableIndex { get; internal set; }
    /// <summary>High spawn instruction byte: zero starts leftward; nonzero starts rightward.</summary>
    public ushort PrimaryDirection { get; internal set; }
    /// <summary>Low extra-properties byte giving the left endpoint's rest count in sixteen-update units; $FF selects an inert endpoint.</summary>
    public ushort MovedLeftRestParameter { get; internal set; }
    /// <summary>High extra-properties byte giving the right endpoint's rest count in sixteen-update units; $FF selects an inert endpoint.</summary>
    public ushort MovedRightRestParameter { get; internal set; }
    /// <summary>Low parameter-1 byte: 0 timer, 1 horizontal proximity, 2 immediate, 3 one-shot reaction, or 4 repeatable reaction activation.</summary>
    public ushort TriggerMode { get; internal set; }
    /// <summary>High parameter-1 byte specifying whole-pixel X endpoint separation; the native storage calls this field YDistance.</summary>
    public ushort TravelDistance { get; internal set; }
    /// <summary>Parameter 2: initial AI-update countdown or strict absolute horizontal pixel-distance threshold, according to trigger mode.</summary>
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode, giving a byte offset 0 through 8 into the native five-entry initial-function table.</summary>
    public ushort InitialFunctionTableOffset { get; internal set; }
    /// <summary>Left endpoint's wrapping rest countdown, the parameter shifted left four; $0FF0 installs the inert function.</summary>
    public ushort MovedLeftRestTime { get; internal set; }
    /// <summary>Right endpoint's wrapping rest countdown, the parameter shifted left four; $0FF0 installs the inert function.</summary>
    public ushort MovedRightRestTime { get; internal set; }
    /// <summary>Initially clear mode-3 activation latch; the first eligible reaction sets it permanently for this enemy lifetime.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel X snapshot immediately before its current fixed-point move.</summary>
    public ushort PreviousXPosition { get; internal set; }
    /// <summary>Left whole-pixel travel boundary derived from spawn X and travel distance; leftward motion stops upon reaching or passing it.</summary>
    public ushort MinimumXPosition { get; internal set; }
    /// <summary>Right whole-pixel travel boundary derived from spawn X and travel distance; rightward motion stops upon reaching or passing it.</summary>
    public ushort MaximumXPosition { get; internal set; }
    /// <summary>Whether Samus overlaps the shutter on its leading side before movement, enabling fixed-point extra-X displacement and opposing-input ejection.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>Alternating reaction direction, initialized as primary direction XOR 1 and toggled before choosing left for zero or right for nonzero.</summary>
    public ushort ReactionDirection { get; internal set; }
    /// <summary>Samus's whole-pixel X captured after each main-AI dispatch, including inert updates, preserving the native extended-word snapshot.</summary>
    public ushort PreviousSamusXPosition { get; internal set; }
    /// <summary>Samus's fractional X low word captured after every main-AI dispatch alongside the whole-pixel snapshot.</summary>
    public ushort PreviousSamusXSubposition { get; internal set; }
}

/// <summary>Shared identity, lifecycle, and fixed-point helpers for bank-$A2 shutters.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy-definition pointer for the growing shutter.</summary>
    internal const ushort GrowingShutterDefinition = 0xd4ff;
    /// <summary>Native enemy-definition pointer for the shootable vertical shutter.</summary>
    internal const ushort ShootableVerticalShutterDefinition = 0xd53f;
    /// <summary>Native enemy-definition pointer for the retail-unused horizontal shutter.</summary>
    internal const ushort ShootableHorizontalShutterDefinition = 0xd57f;
    /// <summary>Native enemy-definition pointer for the destroyable vertical shutter.</summary>
    internal const ushort DestroyableVerticalShutterDefinition = 0xd5bf;
    /// <summary>Native enemy-definition pointer for the Kamer vertical platform.</summary>
    internal const ushort KamerVerticalPlatformDefinition = 0xd5ff;

    /// <summary>Native touch callback shared by vertical shutter variants.</summary>
    internal const ushort VerticalShutterTouchAi = EnemyAiCodePointers.BankA2.VerticalShutterTouch;
    /// <summary>Native shot callback for the shootable vertical shutter.</summary>
    internal const ushort ShootableVerticalShutterShotAi = EnemyAiCodePointers.BankA2.ShootableVerticalShutterShot;
    /// <summary>Native shot callback for the destroyable vertical shutter.</summary>
    internal const ushort DestroyableVerticalShutterShotAi = EnemyAiCodePointers.BankA2.DestroyableVerticalShutterShot;
    /// <summary>Native power-bomb callback shared by vertical shutter variants.</summary>
    internal const ushort VerticalShutterPowerBombAi = EnemyAiCodePointers.BankA2.VerticalShutterPowerBomb;
    /// <summary>Native touch callback for the horizontal shutter.</summary>
    internal const ushort HorizontalShutterTouchAi = EnemyAiCodePointers.BankA2.HorizontalShutterTouch;
    /// <summary>Native shot callback for the horizontal shutter.</summary>
    internal const ushort HorizontalShutterShotAi = EnemyAiCodePointers.BankA2.HorizontalShutterShot;
    /// <summary>Native power-bomb callback for the horizontal shutter.</summary>
    internal const ushort HorizontalShutterPowerBombAi = EnemyAiCodePointers.BankA2.HorizontalShutterPowerBomb;

    /// <summary>Library-two sound selected when an on-screen shutter activates.</summary>
    private const ushort ShutterActivationSound = 0x000e;
    /// <summary>Endpoint rest value that selects the permanent inert dispatcher state.</summary>
    private const ushort PermanentStopRestTime = 0x0ff0;

    /// <summary>Growing-shutter extension states indexed by enemy slot.</summary>
    private readonly GrowingShutterEnemyState?[] _growingShutterStates = new GrowingShutterEnemyState?[MaximumEnemyCount];
    /// <summary>Vertical shutter and Kamer extension states indexed by enemy slot.</summary>
    internal readonly VerticalShutterEnemyState?[] _verticalShutterStates = new VerticalShutterEnemyState?[MaximumEnemyCount];
    /// <summary>Horizontal shutter extension states indexed by enemy slot.</summary>
    private readonly HorizontalShutterEnemyState?[] _horizontalShutterStates = new HorizontalShutterEnemyState?[MaximumEnemyCount];
    /// <summary>Camera X captured for shutter screen-window checks.</summary>
    private ushort _shutterCameraX;
    /// <summary>Camera Y captured for shutter screen-window checks.</summary>
    private ushort _shutterCameraY;

    /// <summary>Last library-two shutter sound queued during the current enemy frame.</summary>
    public ushort? LastShutterSoundEffect { get; private set; }

    /// <summary>Clears slot-indexed shutter state and captures the room camera origin.</summary>
    private void ResetShutterRoomState(ushort cameraX, ushort cameraY)
    {
        Array.Clear(_growingShutterStates);
        Array.Clear(_verticalShutterStates);
        Array.Clear(_horizontalShutterStates);
        _shutterCameraX = cameraX;
        _shutterCameraY = cameraY;
        LastShutterSoundEffect = null;
    }

    /// <summary>Captures this frame's camera position and clears the prior sound request.</summary>
    private void BeginShutterFrame(ushort cameraX, ushort cameraY)
    {
        _shutterCameraX = cameraX;
        _shutterCameraY = cameraY;
        LastShutterSoundEffect = null;
    }

    /// <summary>Recognizes definitions routed through the vertical-shutter dispatcher.</summary>
    private static bool IsVerticalShutterDefinition(ushort definition) => definition is
        ShootableVerticalShutterDefinition or DestroyableVerticalShutterDefinition or
        KamerVerticalPlatformDefinition;

    /// <summary>Adds a signed 16.16 velocity using the same wrapping carry as ADC.</summary>
    private static (ushort Position, ushort Subposition) AddShutterVelocity(
        ushort position,
        ushort subposition,
        short wholeVelocity,
        ushort subvelocity)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        int velocity = unchecked((wholeVelocity << 16) | subvelocity);
        fixedPosition = unchecked(fixedPosition + (uint)velocity);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    /// <summary>Implements the native strict wrapped absolute-X-distance comparison.</summary>
    /// <summary>
    /// The shared idle-shutter contact test at $A2:EEE9 and $A2:F1F6: the bitwise AND of
    /// Samus's four solid-enemy collision words equals this enemy (one or more directions
    /// hit it and the rest hold $FFFF), and her contact-damage index is nonzero.
    /// </summary>
    private static bool SamusContactsIdleShutter(RoomEnemySlot slot, SamusState samus)
    {
        IReadOnlyList<ushort> collisions = samus.Kinematics.SolidEnemyCollisionIndexes;
        ushort combined = unchecked((ushort)(collisions[0] & collisions[1] & collisions[2] & collisions[3]));
        return combined != 0xffff &&
            combined == slot.NativeIndex &&
            samus.HorizontalSpeed.ContactDamageIndex != 0;
    }

    /// <summary>Tests wrapped absolute X distance with the native strict less-than threshold.</summary>
    private static bool IsSamusWithinShutterHorizontalDistance(
        RoomEnemySlot slot,
        SamusState samus,
        ushort maximumDistance)
    {
        ushort distance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        if (unchecked((short)distance) < 0)
            distance = unchecked((ushort)-distance);
        return distance < maximumDistance;
    }

    /// <summary>
    /// Ports $A0:AD70. Its bottom edge is 256 pixels, deliberately not the 224-line visible
    /// viewport, because the original sound admission test uses a square screen window.
    /// </summary>
    private static bool IsShutterCenterOnScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        unchecked((short)(slot.XPosition - cameraX)) >= 0 &&
        unchecked((short)(cameraX + 0x0100 - slot.XPosition)) >= 0 &&
        unchecked((short)(slot.YPosition - cameraY)) >= 0 &&
        unchecked((short)(cameraY + 0x0100 - slot.YPosition)) >= 0;

    /// <summary>Requests activation audio only when the shutter center is inside the native screen window.</summary>
    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY)
    {
        if (IsShutterCenterOnScreen(slot, cameraX, cameraY))
            LastShutterSoundEffect = ShutterActivationSound;
    }

    /// <summary>Returns initialized growing-shutter state for the supplied enemy slot.</summary>
    private GrowingShutterEnemyState RequireGrowingShutterState(RoomEnemySlot slot) =>
        _growingShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized growing-shutter state.");

    /// <summary>Returns initialized vertical-shutter state for the supplied enemy slot.</summary>
    private VerticalShutterEnemyState RequireVerticalShutterState(RoomEnemySlot slot) =>
        _verticalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized vertical-shutter state.");

    /// <summary>Returns initialized horizontal-shutter state for the supplied enemy slot.</summary>
    private HorizontalShutterEnemyState RequireHorizontalShutterState(RoomEnemySlot slot) =>
        _horizontalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized horizontal-shutter state.");
}
