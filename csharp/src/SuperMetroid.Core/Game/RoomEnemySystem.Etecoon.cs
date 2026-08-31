using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A7 function words stored in an Etecoon's variable F. The names describe
/// the actor-side work performed by each native routine; animation remains owned by the
/// independent ROM instruction interpreter, just as it is on the SNES.
/// </summary>
public enum EtecoonAiFunction : ushort
{
    WaitingForSamus = 0xe9af,
    PreJumpCountdown = 0xea00,
    InitialJump = 0xea37,
    FaceSamusPause = 0xeab5,
    RunLeftToWall = 0xeb02,
    RunRightToWall = 0xeb2c,
    Airborne = 0xeb50,
    WallPause = 0xebcd,
    RouteAfterLanding = 0xec1b,
    MoveLeftToTeachingStart = 0xec97,
    MoveRightToTeachingStart = 0xecbb,
    RunRightToLongJump = 0xecdf,
    LongJump = 0xed09,
    RunRightAfterLongJump = 0xed2a,
    LongJumpLanding = 0xed54,
    TeachingJump = 0xed75,
    IdleBetweenTeachingJumps = 0xedc7,
    WaitForSamusBeforeLongRun = 0xee3e,
    RunRightToReturnJump = 0xee9a,
    ReturnJump = 0xeeb8,
}

/// <summary>
/// Debugger-facing projection of the six native Etecoon variables. All values remain backed
/// by the common enemy slot, so a watch window shows the same wrapped words consumed by the
/// translated dispatcher and the cartridge instruction interpreter.
/// </summary>
public sealed class EtecoonEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal EtecoonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Signed whole pixels of vertical velocity in variable A.</summary>
    public ushort VerticalVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Fractional vertical velocity in variable B.</summary>
    public ushort VerticalSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Signed whole pixels of horizontal velocity in variable C.</summary>
    public ushort HorizontalVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Fractional horizontal velocity in variable D.</summary>
    public ushort HorizontalSubvelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Wrapped countdown in variable E.</summary>
    public ushort FunctionTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Bank-$A7 function address in variable F.</summary>
    public EtecoonAiFunction Function
    {
        get => (EtecoonAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Literal translation of friendly Etecoon enemy <c>$E5BF</c> from $A7:E912-$EEEA.
/// The family deliberately has no touch, shot, grapple, or power-bomb combat behavior:
/// its retail header points those entries at shared no-ops and gives it zero contact damage.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort EtecoonDefinition = 0xe5bf;

    private const ushort EtecoonInitialInstructionList = 0xe8ce;
    private const ushort EtecoonWakeInstructionList = 0xe8d6;
    private const ushort EtecoonCrouchInstructionList = 0xe854;
    private const ushort EtecoonFacingLeftInstructionList = 0xe81e;
    private const ushort EtecoonFacingRightInstructionList = 0xe876;
    private const ushort EtecoonStationaryFrame = 0xe862;
    private const ushort EtecoonRunLeftFrame = 0xe828;
    private const ushort EtecoonRunRightFrame = 0xe880;
    private const ushort EtecoonAirborneRightFrame = 0xe898;
    private const ushort EtecoonTurnLeftInstructionList = 0xe870;
    private const ushort EtecoonTurnRightInstructionList = 0xe8c8;
    private const ushort EtecoonLandLeftInstructionList = 0xe854;
    private const ushort EtecoonLandRightInstructionList = 0xe8ac;
    private const ushort EtecoonResumeRightInstructionList = 0xe894;
    private const ushort EtecoonResumeLeftInstructionList = 0xe83c;

    private const ushort EtecoonWakeSound = 0x0035;
    private const ushort EtecoonJumpSound = 0x0033;
    private const ushort EtecoonWallSound = 0x0032;
    private const int EtecoonActivationYDistance = 0x80;
    private const int EtecoonNearbyDistance = 0x40;
    private const int EtecoonDirectionBand = 0x20;
    private const int EtecoonLongRunTriggerDistance = 0x30;
    private const int EtecoonRightWallProbe = 32 << 16;
    private const ushort EtecoonFirstTeachingX = 537;
    private const ushort EtecoonRunStartX = 600;
    private const ushort EtecoonLongJumpMidpointX = 680;
    private const ushort EtecoonLongJumpEndX = 840;
    private const ushort EtecoonLongRunMaximumX = 832;

    // These are the literal eight ROM words at $A7:E900-$E90F. Keeping them named beside
    // their consumers avoids replacing authored fixed-point values with host-side tuning.
    private const ushort EtecoonJumpYVelocity = 0xfffd;
    private const ushort EtecoonJumpYSubvelocity = 0x0000;
    private const ushort EtecoonLongJumpYVelocity = 0xfffc;
    private const ushort EtecoonLongJumpYSubvelocity = 0x0000;
    private const ushort EtecoonRightVelocity = 0x0002;
    private const ushort EtecoonRightSubvelocity = 0x0000;
    private const ushort EtecoonLeftVelocity = 0xfffe;
    private const ushort EtecoonLeftSubvelocity = 0x0000;

    private readonly EtecoonEnemyState?[] _etecoonStates =
        new EtecoonEnemyState?[MaximumEnemyCount];

    /// <summary>Typed native state for every physical Etecoon slot.</summary>
    public IReadOnlyList<EtecoonEnemyState?> EtecoonStates => _etecoonStates;

    /// <summary>Most recent library-two Etecoon sound request during this frame.</summary>
    public ushort? LastEtecoonSoundEffect { get; private set; }

    /// <summary>Ports <c>Etecoon_Init</c> at <c>$A7:E912</c>.</summary>
    private void InitializeEtecoon(RoomEnemySlot slot)
    {
        var state = new EtecoonEnemyState(slot);
        _etecoonStates[slot.SlotIndex] = state;

        // Native ORs property $2000 (`DisableSamusColl`). The host scheduler also uses the
        // same raw bit to admit the ROM instruction interpreter, so retain the authored
        // bit instead of inferring animation activity from the population record.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = EtecoonInitialInstructionList;
        state.Function = EtecoonAiFunction.WaitingForSamus;
        state.FunctionTimer = 0xffff;
    }

    /// <summary>Ports <c>Etecoon_Main</c> and its indirect function dispatcher.</summary>
    private void RunEtecoonMain(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // The high byte of parameter two doubles as a native 128-frame quake suspension.
        // Its low byte is the actor's permanent route (zero, one, or two) and must survive.
        if ((slot.Parameter2 & 0xff00) != 0)
        {
            slot.Parameter2 = unchecked((ushort)(slot.Parameter2 - 0x0100));
            return;
        }

        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Etecoon AI requires the active Samus actor.");
        RoomLevelData activeLevel = level ?? throw new InvalidOperationException(
            "Etecoon AI requires active room collision data.");

        switch (state.Function)
        {
            case EtecoonAiFunction.WaitingForSamus:
                RunEtecoonWaiting(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.PreJumpCountdown:
                RunEtecoonPreJumpCountdown(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.InitialJump:
                RunEtecoonInitialJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.FaceSamusPause:
                RunEtecoonFaceSamusPause(slot, state);
                break;
            case EtecoonAiFunction.RunLeftToWall:
                RunEtecoonLeftToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToWall:
                RunEtecoonRightToWall(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.Airborne:
                RunEtecoonAirborne(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.WallPause:
                RunEtecoonWallPause(slot, state);
                break;
            case EtecoonAiFunction.RouteAfterLanding:
                RunEtecoonRouteAfterLanding(slot, state);
                break;
            case EtecoonAiFunction.MoveLeftToTeachingStart:
                RunEtecoonMoveLeftToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.MoveRightToTeachingStart:
                RunEtecoonMoveRightToTeachingStart(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.RunRightToLongJump:
                RunEtecoonRightToLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJump:
                RunEtecoonLongJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.RunRightAfterLongJump:
                RunEtecoonRightAfterLongJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.LongJumpLanding:
                RunEtecoonLongJumpLanding(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.TeachingJump:
                RunEtecoonTeachingJump(slot, state, activeSamus, activeLevel);
                break;
            case EtecoonAiFunction.IdleBetweenTeachingJumps:
                RunEtecoonIdleBetweenTeachingJumps(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.WaitForSamusBeforeLongRun:
                RunEtecoonWaitForSamusBeforeLongRun(slot, state, activeSamus);
                break;
            case EtecoonAiFunction.RunRightToReturnJump:
                RunEtecoonRightToReturnJump(slot, state, activeLevel);
                break;
            case EtecoonAiFunction.ReturnJump:
                RunEtecoonReturnJump(slot, state, activeSamus, activeLevel);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon function $A7:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the dormant/wake loop at <c>$A7:E9AF</c>.</summary>
    private void RunEtecoonWaiting(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        // RoomEnemySystem is run only after the door-enemy transition flag clears. Native
        // tests that flag here before touching E; no separate host branch is therefore lost.
        if ((state.FunctionTimer & 0x8000) == 0)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                state.FunctionTimer = 11;
            }
            return;
        }

        if (!EtecoonIsWithinY(slot, samus, EtecoonActivationYDistance))
            return;

        if ((slot.Parameter2 & 3) == 0)
            LastEtecoonSoundEffect = EtecoonWakeSound;
        InstallEtecoonInstruction(slot, EtecoonWakeInstructionList);
        state.FunctionTimer = 256;
    }

    private void RunEtecoonPreJumpCountdown(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        slot.InstructionTimer = 1;
        state.Function = EtecoonAiFunction.InitialJump;
        if (unchecked((short)(samus.XPosition - 256)) >= 0)
            LastEtecoonSoundEffect = EtecoonJumpSound;
    }

    private void RunEtecoonInitialJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if ((state.VerticalVelocity & 0x8000) == 0)
        {
            if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
                EtecoonIsWithinX(slot, samus, EtecoonNearbyDistance))
            {
                ushort direction = DetermineEtecoonDirectionToSamus(slot, samus);
                if (unchecked((short)(direction - 5)) < 0)
                {
                    InstallEtecoonInstruction(slot, EtecoonFacingLeftInstructionList);
                    slot.Parameter1 = 0;
                }
                else
                {
                    InstallEtecoonInstruction(slot, EtecoonFacingRightInstructionList);
                    slot.Parameter1 = 1;
                }
                state.FunctionTimer = 32;
                state.Function = EtecoonAiFunction.FaceSamusPause;
            }
            else
            {
                state.FunctionTimer = 11;
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonStationaryFrame;
    }

    private static void RunEtecoonFaceSamusPause(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        slot.InstructionTimer = 1;
        if (slot.Parameter1 != 0)
        {
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
            state.Function = EtecoonAiFunction.RunRightToWall;
        }
        else
        {
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
            state.Function = EtecoonAiFunction.RunLeftToWall;
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    private void RunEtecoonLeftToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(slot, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        state.Function = EtecoonAiFunction.RunRightToWall;
        InstallEtecoonInstruction(slot, EtecoonRunRightFrame);
        slot.Parameter1 = 1;
    }

    private void RunEtecoonRightToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (EnemyHasSolidHighBitHorizontallyAhead(
                level,
                slot,
                EtecoonRightWallProbe))
        {
            InstallEtecoonInstruction(slot, EtecoonAirborneRightFrame);
            state.Function = EtecoonAiFunction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(slot, state, level);
    }

    private void RunEtecoonAirborne(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot);
        if (MoveEtecoonHorizontally(slot, state, level))
        {
            if (slot.Parameter1 != 0)
            {
                InstallEtecoonInstruction(slot, EtecoonTurnLeftInstructionList);
                slot.Parameter1 = 0;
            }
            else
            {
                InstallEtecoonInstruction(slot, EtecoonTurnRightInstructionList);
                slot.Parameter1 = 1;
            }
            state.Function = EtecoonAiFunction.WallPause;
            state.FunctionTimer = 8;
            if (unchecked((short)(samus.XPosition - 256)) >= 0)
                LastEtecoonSoundEffect = EtecoonWallSound;
            return;
        }

        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        InstallEtecoonInstruction(
            slot,
            slot.Parameter1 != 0
                ? EtecoonLandLeftInstructionList
                : EtecoonLandRightInstructionList);
        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    private void RunEtecoonWallPause(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (slot.Parameter1 != 0)
        {
            InstallEtecoonInstruction(slot, EtecoonResumeRightInstructionList);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        else
        {
            InstallEtecoonInstruction(slot, EtecoonResumeLeftInstructionList);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        }
        state.Function = EtecoonAiFunction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    private static void RunEtecoonRouteAfterLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state)
    {
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        switch ((byte)slot.Parameter2)
        {
            case 0:
                state.Function = EtecoonAiFunction.MoveLeftToTeachingStart;
                slot.CurrentInstruction = EtecoonRunLeftFrame;
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
                break;
            case 1:
                state.Function = EtecoonAiFunction.MoveRightToTeachingStart;
                slot.CurrentInstruction = EtecoonRunRightFrame;
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            case 2:
                state.Function = EtecoonAiFunction.TeachingJump;
                slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
                SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
                break;
            default:
                throw new InvalidDataException(
                    $"Etecoon route {(byte)slot.Parameter2} exceeds the retail three-entry table.");
        }
        slot.InstructionTimer = 1;
    }

    private void RunEtecoonMoveLeftToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonFirstTeachingX)) >= 0)
            return;

        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
    }

    private void RunEtecoonMoveRightToTeachingStart(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonRunStartX)) < 0)
            return;

        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
        InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
    }

    private void RunEtecoonRightToLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.LongJump;
        SetEtecoonVerticalVelocity(state, EtecoonLongJumpYVelocity, EtecoonLongJumpYSubvelocity);
        InstallEtecoonInstruction(slot, EtecoonAirborneRightFrame);
    }

    private void RunEtecoonLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        MoveEtecoonVertically(slot, state, samus, level);
        if (unchecked((short)(slot.XPosition - EtecoonLongJumpMidpointX)) < 0)
            return;

        InstallEtecoonInstruction(slot, EtecoonRunRightFrame);
        state.Function = EtecoonAiFunction.RunRightAfterLongJump;
    }

    private void RunEtecoonRightAfterLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonLongJumpEndX)) < 0)
            return;

        InstallEtecoonInstruction(slot, EtecoonAirborneRightFrame);
        state.Function = EtecoonAiFunction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, 0xffff, EtecoonLongJumpYSubvelocity);
    }

    private void RunEtecoonLongJumpLanding(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        state.FunctionTimer = 11;
        InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
    }

    private void RunEtecoonTeachingJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        PauseEtecoonForEarthquake(slot);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        if ((state.VerticalVelocity & 0x8000) == 0)
        {
            state.FunctionTimer = 11;
            InstallEtecoonInstruction(slot, EtecoonCrouchInstructionList);
            state.Function = (slot.Parameter2 & 2) != 0 &&
                unchecked((short)(slot.XPosition - EtecoonLongRunMaximumX)) < 0
                    ? EtecoonAiFunction.WaitForSamusBeforeLongRun
                    : EtecoonAiFunction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonStationaryFrame;
    }

    private void RunEtecoonIdleBetweenTeachingJumps(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        // Parameter one's high byte counts four teaching hops. Native then clears only
        // that byte, preserving the low-byte facing selector used by the airborne states.
        slot.Parameter1 = unchecked((ushort)(slot.Parameter1 + 0x0100));
        bool fewerThanFourTeachingJumps =
            unchecked((short)((slot.Parameter1 & 0xff00) - 0x0400)) < 0;
        if (!fewerThanFourTeachingJumps)
        {
            // `$A7:EDFD` clears the completed high-byte counter before testing route two.
            // Route two therefore loops in groups of four rather than overflowing forever.
            slot.Parameter1 = (byte)slot.Parameter1;
        }
        if (fewerThanFourTeachingJumps || (slot.Parameter2 & 2) != 0)
        {
            state.Function = EtecoonAiFunction.TeachingJump;
            slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
        }
        else
        {
            state.Function = EtecoonAiFunction.RunRightToReturnJump;
            slot.CurrentInstruction = EtecoonRunRightFrame;
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.InstructionTimer = 1;
        if (unchecked((short)(samus.XPosition - 256)) >= 0)
            LastEtecoonSoundEffect = EtecoonJumpSound;
    }

    private void RunEtecoonWaitForSamusBeforeLongRun(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (EtecoonIsWithinY(slot, samus, EtecoonNearbyDistance) &&
            EtecoonIsWithinX(slot, samus, EtecoonLongRunTriggerDistance))
        {
            slot.CurrentInstruction = EtecoonRunRightFrame;
            state.Function = EtecoonAiFunction.RunRightToLongJump;
        }
        else
        {
            SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
            slot.CurrentInstruction = unchecked((ushort)(slot.CurrentInstruction + 2));
            state.Function = EtecoonAiFunction.TeachingJump;
            if (unchecked((short)(samus.XPosition - 256)) >= 0)
                LastEtecoonSoundEffect = EtecoonJumpSound;
        }
        slot.InstructionTimer = 1;
    }

    private void RunEtecoonRightToReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.ReturnJump;
        InstallEtecoonInstruction(slot, EtecoonAirborneRightFrame);
    }

    private void RunEtecoonReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (!MoveEtecoonVertically(slot, state, samus, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        state.Function = EtecoonAiFunction.RunLeftToWall;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        InstallEtecoonInstruction(slot, EtecoonRunLeftFrame);
    }

    /// <summary>Ports <c>Etecoon_Func_1</c>'s quake-owned parameter and timer writes.</summary>
    private void PauseEtecoonForEarthquake(RoomEnemySlot slot)
    {
        if (EarthquakeTimer == 0)
            return;

        slot.Parameter2 = unchecked((ushort)((byte)slot.Parameter2 | 0x8000));
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer + 128));
    }

    /// <summary>Ports <c>Etecoon_Func_2</c>'s signed 16.16 horizontal mover.</summary>
    private bool MoveEtecoonHorizontally(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level) =>
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            ComposeEtecoonFixed(state.HorizontalVelocity, state.HorizontalSubvelocity));

    /// <summary>Ports <c>Etecoon_Func_3</c>, including its five-pixel gravity cap.</summary>
    private bool MoveEtecoonVertically(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        int displacement = ComposeEtecoonFixed(
            state.VerticalVelocity,
            state.VerticalSubvelocity);
        if (unchecked((short)(state.VerticalVelocity - 5)) < 0)
        {
            AddEtecoonFixed(
                state,
                samus.Kinematics.YAcceleration,
                samus.Kinematics.YSubacceleration);
        }
        return MoveEnemyVertically(level, slot, displacement);
    }

    private static int ComposeEtecoonFixed(ushort whole, ushort fraction) =>
        unchecked(((int)(short)whole << 16) | fraction);

    private static void AddEtecoonFixed(
        EtecoonEnemyState state,
        ushort wholeIncrement,
        ushort fractionIncrement)
    {
        uint value = ((uint)state.VerticalVelocity << 16) | state.VerticalSubvelocity;
        uint increment = ((uint)wholeIncrement << 16) | fractionIncrement;
        value = unchecked(value + increment);
        state.VerticalVelocity = unchecked((ushort)(value >> 16));
        state.VerticalSubvelocity = unchecked((ushort)value);
    }

    private static void SetEtecoonVerticalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.VerticalVelocity = whole;
        state.VerticalSubvelocity = fraction;
    }

    private static void SetEtecoonHorizontalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.HorizontalVelocity = whole;
        state.HorizontalSubvelocity = fraction;
    }

    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        bool wasOne = state.FunctionTimer == 1;
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return wasOne || (state.FunctionTimer & 0x8000) != 0;
    }

    private static bool EtecoonIsWithinX(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) < distance;

    private static bool EtecoonIsWithinY(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition))) < distance;

    /// <summary>Ports <c>DetermineDirectionOfSamusFromEnemy</c> for the facing branch.</summary>
    private static ushort DetermineEtecoonDirectionToSamus(
        RoomEnemySlot slot,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.XPosition - slot.XPosition));
        short deltaY = unchecked((short)(samus.YPosition - slot.YPosition));
        if (Math.Abs(deltaY) < EtecoonDirectionBand)
            return deltaX < 0 ? (ushort)7 : (ushort)2;
        if (Math.Abs(deltaX) < EtecoonDirectionBand)
            return deltaY < 0 ? (ushort)0 : (ushort)4;
        if (deltaX < 0)
            return deltaY < 0 ? (ushort)8 : (ushort)6;
        return deltaY < 0 ? (ushort)1 : (ushort)3;
    }

    private static void InstallEtecoonInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    private EtecoonEnemyState RequireEtecoonState(RoomEnemySlot slot) =>
        _etecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Etecoon state.");
}
