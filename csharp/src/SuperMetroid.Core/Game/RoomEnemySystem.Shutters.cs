namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A2 dispatcher values used by enemy $D4FF. The names describe the four
/// initialization waits and two growth directions without discarding the literal ROM
/// addresses a debugger needs when comparing this port with enemy variable A in WRAM.
/// </summary>
public enum GrowingShutterFunction : ushort
{
    WaitToGrowUpForTimer = 0xeabd,
    WaitToGrowUpForProximity = 0xead1,
    WaitToGrowDownForProximity = 0xeae7,
    WaitToGrowDownForTimer = 0xeafd,
    GrowDown = 0xeb11,
    GrowUp = 0xec13,
}

/// <summary>Bank-$A2 dispatcher values shared by the three vertical shutter definitions.</summary>
public enum VerticalShutterFunction : ushort
{
    Initial = 0xef09,
    WaitForTimer = 0xef15,
    WaitForHorizontalProximity = 0xef28,
    Activate = 0xef39,
    InitialNoOp = 0xef40,
    MovingUp = 0xef68,
    MovingDown = 0xefd4,
    StoppedAfterMovingUp = 0xf040,
    StoppedAfterMovingDown = 0xf072,
    PermanentNoOp = 0xf099,
}

/// <summary>Bank-$A2 dispatcher values used by the unused horizontal shutter $D57F.</summary>
public enum HorizontalShutterFunction : ushort
{
    Initial = 0xf224,
    WaitForTimer = 0xf230,
    WaitForHorizontalProximity = 0xf243,
    Activate = 0xf254,
    InitialNoOp = 0xf25b,
    MovingLeft = 0xf272,
    MovingRight = 0xf2e4,
    StoppedAfterMovingLeft = 0xf38c,
    StoppedAfterMovingRight = 0xf3b0,
    PermanentNoOp = 0xf3d4,
}

/// <summary>
/// Typed view of growing-shutter enemy variables and its two extended movement words.
/// Origin words B-E are the four independently positioned 16-pixel growth sections.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    public GrowingShutterFunction Function
    {
        get => (GrowingShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort GrowthLevel0OriginY { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    public ushort GrowthLevel1OriginY { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    public ushort GrowthLevel2OriginY { get => _slot.VariableD; internal set => _slot.VariableD = value; }
    public ushort GrowthLevel3OriginY { get => _slot.VariableE; internal set => _slot.VariableE = value; }

    /// <summary>Variable F; zero through four, where four is the fully grown terminal state.</summary>
    public ushort GrowthLevel { get => _slot.VariableF; internal set => _slot.VariableF = value; }

    /// <summary>Signed whole word of the positive 16.16 growth speed.</summary>
    public short GrowthVelocity { get; internal set; }

    /// <summary>Fraction word of the positive 16.16 growth speed.</summary>
    public ushort GrowthSubvelocity { get; internal set; }

    /// <summary>Extended word $7E:8800,x used to derive Samus's whole-pixel carry.</summary>
    public ushort PreviousYPosition { get; internal set; }

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
    private readonly RoomEnemySlot _slot;

    internal VerticalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    public VerticalShutterFunction Function
    {
        get => (VerticalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort FunctionTimer { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    public ushort DownSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    public short DownVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    public ushort UpSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    public short UpVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    public ushort SpeedTableIndex { get; internal set; }
    public ushort PrimaryDirection { get; internal set; }
    public ushort MovedUpRestParameter { get; internal set; }
    public ushort MovedDownRestParameter { get; internal set; }
    public ushort TriggerMode { get; internal set; }
    public ushort TravelDistance { get; internal set; }
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    public ushort InitialFunctionTableOffset { get; internal set; }
    public ushort MovedUpRestTime { get; internal set; }
    public ushort MovedDownRestTime { get; internal set; }
    public bool MovingSamus { get; internal set; }
    public bool ShotActivated { get; internal set; }
    public ushort PreviousYPosition { get; internal set; }
    public ushort MinimumYPosition { get; internal set; }
    public ushort MaximumYPosition { get; internal set; }
    public ushort ReactionDirection { get; internal set; }
}

/// <summary>Typed state for the cartridge's complete, though retail-unused, horizontal shutter.</summary>
public sealed class HorizontalShutterEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal HorizontalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    public HorizontalShutterFunction Function
    {
        get => (HorizontalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort FunctionTimer { get => _slot.VariableB; internal set => _slot.VariableB = value; }
    public ushort RightSubvelocity { get => _slot.VariableC; internal set => _slot.VariableC = value; }
    public short RightVelocity { get => unchecked((short)_slot.VariableD); internal set => _slot.VariableD = unchecked((ushort)value); }
    public ushort LeftSubvelocity { get => _slot.VariableE; internal set => _slot.VariableE = value; }
    public short LeftVelocity { get => unchecked((short)_slot.VariableF); internal set => _slot.VariableF = unchecked((ushort)value); }

    public ushort SpeedTableIndex { get; internal set; }
    public ushort PrimaryDirection { get; internal set; }
    public ushort MovedLeftRestParameter { get; internal set; }
    public ushort MovedRightRestParameter { get; internal set; }
    public ushort TriggerMode { get; internal set; }
    public ushort TravelDistance { get; internal set; }
    public ushort HorizontalProximityOrWaitTime { get; internal set; }
    public ushort InitialFunctionTableOffset { get; internal set; }
    public ushort MovedLeftRestTime { get; internal set; }
    public ushort MovedRightRestTime { get; internal set; }
    public bool ShotActivated { get; internal set; }
    public ushort PreviousXPosition { get; internal set; }
    public ushort MinimumXPosition { get; internal set; }
    public ushort MaximumXPosition { get; internal set; }
    public bool MovingSamus { get; internal set; }
    public ushort ReactionDirection { get; internal set; }
    public ushort PreviousSamusXPosition { get; internal set; }
    public ushort PreviousSamusXSubposition { get; internal set; }
}

/// <summary>Shared identity, lifecycle, and fixed-point helpers for bank-$A2 shutters.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort GrowingShutterDefinition = 0xd4ff;
    internal const ushort ShootableVerticalShutterDefinition = 0xd53f;
    internal const ushort ShootableHorizontalShutterDefinition = 0xd57f;
    internal const ushort DestroyableVerticalShutterDefinition = 0xd5bf;
    internal const ushort KamerVerticalPlatformDefinition = 0xd5ff;

    internal const ushort VerticalShutterTouchAi = 0xf09d;
    internal const ushort ShootableVerticalShutterShotAi = 0xf0a2;
    internal const ushort DestroyableVerticalShutterShotAi = 0xf0aa;
    internal const ushort VerticalShutterPowerBombAi = 0xf0b6;
    internal const ushort HorizontalShutterTouchAi = 0xf3d8;
    internal const ushort HorizontalShutterShotAi = 0xf40e;
    internal const ushort HorizontalShutterPowerBombAi = 0xf41a;
    internal const ushort GrowingShutterNoOpAi = 0x804c;

    private const ushort ShutterActivationSound = 0x000e;
    private const ushort PermanentStopRestTime = 0x0ff0;

    private readonly GrowingShutterEnemyState?[] _growingShutterStates = new GrowingShutterEnemyState?[MaximumEnemyCount];
    private readonly VerticalShutterEnemyState?[] _verticalShutterStates = new VerticalShutterEnemyState?[MaximumEnemyCount];
    private readonly HorizontalShutterEnemyState?[] _horizontalShutterStates = new HorizontalShutterEnemyState?[MaximumEnemyCount];
    private ushort _shutterCameraX;
    private ushort _shutterCameraY;

    public IReadOnlyList<GrowingShutterEnemyState?> GrowingShutterStates => _growingShutterStates;
    public IReadOnlyList<VerticalShutterEnemyState?> VerticalShutterStates => _verticalShutterStates;
    public IReadOnlyList<HorizontalShutterEnemyState?> HorizontalShutterStates => _horizontalShutterStates;

    /// <summary>Last library-two shutter sound queued during the current enemy frame.</summary>
    public ushort? LastShutterSoundEffect { get; private set; }

    private void ResetShutterRoomState(ushort cameraX, ushort cameraY)
    {
        Array.Clear(_growingShutterStates);
        Array.Clear(_verticalShutterStates);
        Array.Clear(_horizontalShutterStates);
        _shutterCameraX = cameraX;
        _shutterCameraY = cameraY;
        LastShutterSoundEffect = null;
    }

    private void BeginShutterFrame(ushort cameraX, ushort cameraY)
    {
        _shutterCameraX = cameraX;
        _shutterCameraY = cameraY;
        LastShutterSoundEffect = null;
    }

    private static bool IsVerticalShutterDefinition(ushort definition) => definition is
        ShootableVerticalShutterDefinition or DestroyableVerticalShutterDefinition or
        KamerVerticalPlatformDefinition;

    private static bool IsAnyShutterDefinition(ushort definition) =>
        definition == GrowingShutterDefinition ||
        definition == ShootableHorizontalShutterDefinition ||
        IsVerticalShutterDefinition(definition);

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

    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY)
    {
        if (IsShutterCenterOnScreen(slot, cameraX, cameraY))
            LastShutterSoundEffect = ShutterActivationSound;
    }

    private GrowingShutterEnemyState RequireGrowingShutterState(RoomEnemySlot slot) =>
        _growingShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized growing-shutter state.");

    private VerticalShutterEnemyState RequireVerticalShutterState(RoomEnemySlot slot) =>
        _verticalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized vertical-shutter state.");

    private HorizontalShutterEnemyState RequireHorizontalShutterState(RoomEnemySlot slot) =>
        _horizontalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized horizontal-shutter state.");
}
