using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A7 function words stored in an Etecoon's variable F. The names describe
/// the actor-side work performed by each native routine; animation remains owned by the
/// independent ROM instruction interpreter, just as it is on the SNES.
/// </summary>
public enum EtecoonAiFunction : ushort
{
    /// <summary>$A7:E9AF, Function_Etecoon_Initial: waits for Samus within 128 vertical room pixels, runs the $0100 wake/flex countdown, and defers activation during elevator-door transition.</summary>
    WaitingForSamus = 0xe9af,
    /// <summary>$A7:EA00, Function_Etecoon_StartHop_BottomOfRoom: waits for the eleven-update hop countdown, then starts the bottom-room jump at signed 16.16 Y velocity -3 pixels per update.</summary>
    PreJumpCountdown = 0xea00,
    /// <summary>$A7:EA37, Function_Etecoon_Hopping_BottomOfRoom: hops vertically, clears upward velocity on ceiling contact, and chooses a Samus-facing pause or another hop on downward collision.</summary>
    InitialJump = 0xea37,
    /// <summary>$A7:EAB5, Function_Etecoon_LookAtSamus: waits through the $20 look-at countdown, then installs leftward or rightward running at two pixels per update.</summary>
    FaceSamusPause = 0xeab5,
    /// <summary>$A7:EB02, Function_Etecoon_RunningLeft: moves left using terrain collision and reverses into rightward running when it hits a wall.</summary>
    RunLeftToWall = 0xeb02,
    /// <summary>$A7:EB2C, Function_Etecoon_RunningRight: probes 32 room pixels rightward for the solid-block high bit and starts jumping when the wall is near.</summary>
    RunRightToWall = 0xeb2c,
    /// <summary>$A7:EB50, Function_Etecoon_Jumping: resolves horizontal motion before vertical motion, enters the eight-update wall-jump pause on wall contact, and routes the actor after landing.</summary>
    Airborne = 0xeb50,
    /// <summary>$A7:EBCD, Function_Etecoon_WallJump: waits for the wall-pause timer, installs the opposite-facing wall-jump list, and launches with signed Y velocity -3 and X velocity -2 or +2.</summary>
    WallPause = 0xebcd,
    /// <summary>$A7:EC1B, Function_Etecoon_LandedFromJump: after the eleven-update landing wait, selects the left run-up, right run-up, or immediate top-room hop from population parameter two's route byte.</summary>
    RouteAfterLanding = 0xec1b,
    /// <summary>$A7:EC97, Function_Etecoon_RunToLeftRunUpPoint: moves left until whole room X is below 537, then enters the top-room hopping countdown.</summary>
    MoveLeftToTeachingStart = 0xec97,
    /// <summary>$A7:ECBB, Function_Etecoon_RunToRightRunUpPoint: moves right until whole room X reaches 600, then enters the top-room hopping countdown.</summary>
    MoveRightToTeachingStart = 0xecbb,
    /// <summary>$A7:ECDF, Function_Etecoon_RunningForSuccessfulMorphTunnelJump: runs right to room X 600 and begins the successful tunnel jump at signed Y velocity -4 pixels per update.</summary>
    RunRightToLongJump = 0xecdf,
    /// <summary>$A7:ED09, Function_Etecoon_SuccessfulMorphTunnelJump: applies horizontal and vertical motion until room X reaches 680, then switches to the tunnel-running list.</summary>
    LongJump = 0xed09,
    /// <summary>$A7:ED2A, Function_Etecoon_RunningThroughMorphTunnel: runs right to room X 840, then selects the ledge-fall jump list with signed Y velocity -1 pixel per update.</summary>
    RunRightAfterLongJump = 0xed2a,
    /// <summary>$A7:ED54, Function_Etecoon_FallingFromMorphTunnelLedge: moves right and vertically from the tunnel ledge until terrain collision starts the next eleven-update top-room hop wait.</summary>
    LongJumpLanding = 0xed54,
    /// <summary>$A7:ED75, Function_Etecoon_Hopping_TopOfRoom: performs the top-room vertical hop; downward collision may select route two's Samus-proximity wait, while upward collision clears vertical velocity.</summary>
    TeachingJump = 0xed75,
    /// <summary>$A7:EDC7, Function_Etecoon_StartHop_TopOfRoom: counts four teaching hops in parameter one's high byte; route two repeats hop groups, while the other routes begin the failed tunnel-jump run.</summary>
    IdleBetweenTeachingJumps = 0xedc7,
    /// <summary>$A7:EE3E, Function_Etecoon_HopUntilSamusIsNear: after the hop wait, starts the successful tunnel run when Samus is within 64 vertical and 48 horizontal room pixels, otherwise hops again.</summary>
    WaitForSamusBeforeLongRun = 0xee3e,
    /// <summary>$A7:EE9A, Function_Etecoon_RunningForFailedMorphTunnelJump: runs right to room X 600 before selecting the jump that returns the other routes to the wall-jump demonstration.</summary>
    RunRightToReturnJump = 0xee9a,
    /// <summary>$A7:EEB8, Function_Etecoon_FailedTunnelJump: applies horizontal and vertical motion until collision, then restores leftward running and the ordinary -3-pixel jump velocity.</summary>
    ReturnJump = 0xeeb8,
}

/// <summary>
/// Debugger-facing projection of the six native Etecoon variables. All values remain backed
/// by the common enemy slot, so a watch window shows the same wrapped words consumed by the
/// translated dispatcher and the cartridge instruction interpreter.
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>Backing physical enemy slot whose variables are exposed to diagnostics.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a debugger projection over the live enemy slot.</summary>
    /// <param name="slot">Initialized Etecoon slot supplying the native variable words.</param>
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
    /// <summary>Native enemy definition pointer used to recognize friendly Etecoon instances.</summary>
    internal const ushort EtecoonDefinition = 0xe5bf;

    /// <summary>Library-two sound requested when an Etecoon wakes.</summary>
    private const ushort EtecoonWakeSound = 0x0035;
    /// <summary>Library-two sound requested when a hop or teaching jump begins.</summary>
    private const ushort EtecoonJumpSound = 0x0033;
    /// <summary>Library-two sound requested when the actor reaches a wall.</summary>
    private const ushort EtecoonWallSound = 0x0032;
    /// <summary>Vertical distance in room pixels required to activate a dormant actor.</summary>
    private const int EtecoonActivationYDistance = 0x80;
    /// <summary>Room-pixel proximity used to face Samus and qualify the nearby teaching hop.</summary>
    private const int EtecoonNearbyDistance = 0x40;
    /// <summary>Room-pixel component threshold used to classify Samus as horizontally or vertically aligned.</summary>
    private const int EtecoonDirectionBand = 0x20;
    /// <summary>Horizontal room-pixel distance for the successful long-run trigger.</summary>
    private const int EtecoonLongRunTriggerDistance = 0x30;
    /// <summary>Rightward wall probe distance in signed 16.16 room coordinates.</summary>
    private const int EtecoonRightWallProbe = 32 << 16;
    /// <summary>Whole-pixel boundary where the first route begins its teaching run-up.</summary>
    private const ushort EtecoonFirstTeachingX = 537;
    /// <summary>Whole-pixel boundary where run-up transitions into the tunnel jump.</summary>
    private const ushort EtecoonRunStartX = 600;
    /// <summary>Whole-pixel X boundary where the successful tunnel jump changes to running.</summary>
    private const ushort EtecoonLongJumpMidpointX = 680;
    /// <summary>Whole-pixel X boundary where tunnel running begins the ledge fall.</summary>
    private const ushort EtecoonLongJumpEndX = 840;
    /// <summary>Whole-pixel X boundary used to choose the failed or Samus-wait route.</summary>
    private const ushort EtecoonLongRunMaximumX = 832;

    // These are the literal eight ROM words at $A7:E900-$E90F. Keeping them named beside
    // their consumers avoids replacing authored fixed-point values with host-side tuning.
    /// <summary>Native signed whole-pixel Y velocity for ordinary jumps, equal to -3.</summary>
    private const ushort EtecoonJumpYVelocity = 0xfffd;
    /// <summary>Fractional Y velocity paired with the ordinary jump value.</summary>
    private const ushort EtecoonJumpYSubvelocity = 0x0000;
    /// <summary>Native signed whole-pixel Y velocity for the long tunnel jump, equal to -4.</summary>
    private const ushort EtecoonLongJumpYVelocity = 0xfffc;
    /// <summary>Fractional Y velocity paired with the long-jump value.</summary>
    private const ushort EtecoonLongJumpYSubvelocity = 0x0000;
    /// <summary>Native signed whole-pixel rightward speed.</summary>
    private const ushort EtecoonRightVelocity = 0x0002;
    /// <summary>Fractional rightward speed.</summary>
    private const ushort EtecoonRightSubvelocity = 0x0000;
    /// <summary>Native signed whole-pixel leftward speed.</summary>
    private const ushort EtecoonLeftVelocity = 0xfffe;
    /// <summary>Fractional leftward speed.</summary>
    private const ushort EtecoonLeftSubvelocity = 0x0000;

    /// <summary>Per-slot debugger views for initialized Etecoons; null marks other enemy families.</summary>
    private readonly EtecoonEnemyState?[] _etecoonStates =
        new EtecoonEnemyState?[MaximumEnemyCount];

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
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.Initial;
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
        // Destination OAM construction runs AI while door-transition audio is disabled.
        // Preserve the initial wake edge and the countdown until the native pause clears.
        if (ElevatorDoorTransitionActive)
            return;
        if ((state.FunctionTimer & 0x8000) == 0)
        {
            if (DecrementEtecoonTimerAndTestExpired(state))
            {
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                state.FunctionTimer = 11;
            }
            return;
        }

        if (!EtecoonIsWithinY(slot, samus, EtecoonActivationYDistance))
            return;

        if ((slot.Parameter2 & 3) == 0)
            LastEtecoonSoundEffect = EtecoonWakeSound;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.Flexing);
        state.FunctionTimer = 256;
    }

    /// <summary>Waits for the native hop countdown, then launches with ordinary jump velocity.</summary>
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

    /// <summary>Moves through the initial hop and chooses a Samus-facing pause or another hop on landing.</summary>
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
                    InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.LookRightAtSamusAndRunLeft);
                    slot.Parameter1 = 0;
                }
                else
                {
                    InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.LookLeftAtSamusAndRunRight);
                    slot.Parameter1 = 1;
                }
                state.FunctionTimer = 32;
                state.Function = EtecoonAiFunction.FaceSamusPause;
            }
            else
            {
                state.FunctionTimer = 11;
                state.Function = EtecoonAiFunction.PreJumpCountdown;
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
            }
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.HitCeiling;
    }

    /// <summary>Completes the look-at pause, selects the facing run, and restores ordinary jump velocity.</summary>
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

    /// <summary>Runs left until collision, then reverses into the right-wall approach.</summary>
    private void RunEtecoonLeftToWall(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        if (!MoveEtecoonHorizontally(slot, state, level))
            return;

        SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        state.Function = EtecoonAiFunction.RunRightToWall;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningRight);
        slot.Parameter1 = 1;
    }

    /// <summary>Probes ahead for a solid wall and begins the wall jump when one is near.</summary>
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
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
            state.Function = EtecoonAiFunction.Airborne;
            return;
        }

        MoveEtecoonHorizontally(slot, state, level);
    }

    /// <summary>Applies horizontal then vertical movement and routes wall contact or landing.</summary>
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
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpLeftEligible);
                slot.Parameter1 = 0;
            }
            else
            {
                InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpRightEligible);
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
                ? EtecoonInstructionProgramDefinitions.HoppingFacingLeft
                : EtecoonInstructionProgramDefinitions.HoppingFacingRight);
        state.FunctionTimer = 11;
        state.Function = EtecoonAiFunction.RouteAfterLanding;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    /// <summary>Waits out the wall pause, then launches away from the contacted wall.</summary>
    private void RunEtecoonWallPause(RoomEnemySlot slot, EtecoonEnemyState state)
    {
        PauseEtecoonForEarthquake(slot);
        if (!DecrementEtecoonTimerAndTestExpired(state))
            return;

        if (slot.Parameter1 != 0)
        {
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpRight);
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        else
        {
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.WallJumpLeft);
            SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
        }
        state.Function = EtecoonAiFunction.Airborne;
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
    }

    /// <summary>Selects the route-specific run-up or teaching hop after the landing countdown.</summary>
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
                slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningLeft;
                SetEtecoonHorizontalVelocity(state, EtecoonLeftVelocity, EtecoonLeftSubvelocity);
                break;
            case 1:
                state.Function = EtecoonAiFunction.MoveRightToTeachingStart;
                slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
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

    /// <summary>Runs left to the first route's authored X boundary before starting teaching hops.</summary>
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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
    }

    /// <summary>Runs right to the second route's authored X boundary before starting teaching hops.</summary>
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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
    }

    /// <summary>Runs to the tunnel takeoff boundary and installs the successful long-jump state.</summary>
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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
    }

    /// <summary>Moves through the tunnel jump until the midpoint boundary changes the actor to running.</summary>
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

        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningRight);
        state.Function = EtecoonAiFunction.RunRightAfterLongJump;
    }

    /// <summary>Runs through the tunnel to its end boundary before beginning the ledge fall.</summary>
    private void RunEtecoonRightAfterLongJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonLongJumpEndX)) < 0)
            return;

        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
        state.Function = EtecoonAiFunction.LongJumpLanding;
        SetEtecoonVerticalVelocity(state, 0xffff, EtecoonLongJumpYSubvelocity);
    }

    /// <summary>Completes the ledge fall and schedules the next top-room hop group.</summary>
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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
        state.Function = EtecoonAiFunction.IdleBetweenTeachingJumps;
    }

    /// <summary>Performs a teaching hop and selects another hop or the proximity wait after landing.</summary>
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
            InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.HoppingFacingLeft);
            state.Function = (slot.Parameter2 & 2) != 0 &&
                unchecked((short)(slot.XPosition - EtecoonLongRunMaximumX)) < 0
                    ? EtecoonAiFunction.WaitForSamusBeforeLongRun
                    : EtecoonAiFunction.IdleBetweenTeachingJumps;
            return;
        }

        SetEtecoonVerticalVelocity(state, 0, 0);
        slot.InstructionTimer = 3;
        slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.HitCeiling;
    }

    /// <summary>Counts the teaching-hop group and dispatches the next hop or route-specific run.</summary>
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
            slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
            SetEtecoonHorizontalVelocity(state, EtecoonRightVelocity, EtecoonRightSubvelocity);
        }
        SetEtecoonVerticalVelocity(state, EtecoonJumpYVelocity, EtecoonJumpYSubvelocity);
        slot.InstructionTimer = 1;
        if (unchecked((short)(samus.XPosition - 256)) >= 0)
            LastEtecoonSoundEffect = EtecoonJumpSound;
    }

    /// <summary>Waits between hop groups until Samus meets the successful-jump proximity bounds.</summary>
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
            slot.CurrentInstruction = EtecoonInstructionProgramDefinitions.RunningRight;
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

    /// <summary>Runs to the return-jump takeoff boundary for the unsuccessful tunnel route.</summary>
    private void RunEtecoonRightToReturnJump(
        RoomEnemySlot slot,
        EtecoonEnemyState state,
        RoomLevelData level)
    {
        MoveEtecoonHorizontally(slot, state, level);
        if (unchecked((short)(slot.XPosition - EtecoonRunStartX)) < 0)
            return;

        state.Function = EtecoonAiFunction.ReturnJump;
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.JumpingRight);
    }

    /// <summary>Completes the return jump and restores leftward wall-running behavior on landing.</summary>
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
        InstallEtecoonInstruction(slot, EtecoonInstructionProgramDefinitions.RunningLeft);
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

    /// <summary>Combines signed whole pixels and an unsigned fraction into the mover's signed 16.16 value.</summary>
    private static int ComposeEtecoonFixed(ushort whole, ushort fraction) =>
        unchecked(((int)(short)whole << 16) | fraction);

    /// <summary>Adds a wrapped 16.16 acceleration pair to the actor's vertical velocity.</summary>
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

    /// <summary>Stores the native whole and fractional vertical velocity words.</summary>
    private static void SetEtecoonVerticalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.VerticalVelocity = whole;
        state.VerticalSubvelocity = fraction;
    }

    /// <summary>Stores the native whole and fractional horizontal velocity words.</summary>
    private static void SetEtecoonHorizontalVelocity(
        EtecoonEnemyState state,
        ushort whole,
        ushort fraction)
    {
        state.HorizontalVelocity = whole;
        state.HorizontalSubvelocity = fraction;
    }

    /// <summary>Decrements the wrapped function timer and reports one or underflow as expired.</summary>
    private static bool DecrementEtecoonTimerAndTestExpired(EtecoonEnemyState state)
    {
        bool wasOne = state.FunctionTimer == 1;
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return wasOne || (state.FunctionTimer & 0x8000) != 0;
    }

    /// <summary>Checks signed wrapped horizontal separation against a strict room-pixel distance.</summary>
    private static bool EtecoonIsWithinX(
        RoomEnemySlot slot,
        SamusState samus,
        int distance) =>
        Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) < distance;

    /// <summary>Checks signed wrapped vertical separation against a strict room-pixel distance.</summary>
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

    /// <summary>Installs an instruction-list pointer and makes its first instruction due next update.</summary>
    private static void InstallEtecoonInstruction(
        RoomEnemySlot slot,
        ushort instructionPointer)
    {
        slot.CurrentInstruction = instructionPointer;
        slot.InstructionTimer = 1;
    }

    /// <summary>Returns the initialized per-slot view or fails if the slot is not an Etecoon.</summary>
    private EtecoonEnemyState RequireEtecoonState(RoomEnemySlot slot) =>
        _etecoonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Etecoon state.");
}
