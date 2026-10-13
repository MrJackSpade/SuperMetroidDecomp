using SuperMetroid.Core.Hardware;
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
    /// <summary>Physical enemy record whose variables A through F hold Etecoon's native state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the existing Etecoon slot without copying its variables.</summary>
    internal EtecoonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Signed 16.16 velocity: the vertical whole and fraction words in variables A and B, the
    /// horizontal whole and fraction words in variables C and D.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableC, _slot.VariableD, _slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.Y.WholeWord;
            _slot.VariableB = value.Y.Fraction;
            _slot.VariableC = value.X.WholeWord;
            _slot.VariableD = value.X.Fraction;
        }
    }

    /// <summary>Wrapped countdown in variable E.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Bank-$A7 function address in variable F.</summary>
    public EtecoonAiFunction Function
    {
        get => (EtecoonAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
