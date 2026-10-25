namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The routine an Etecoon's main AI runs each frame. The names describe the actor-side work of each
/// routine; animation is owned by the Etecoon's typed programs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum EtecoonAiFunction : ushort
{
    /// <summary>$A7:E9AF, Function_Etecoon_Initial: waits for Samus within 128 vertical room pixels, runs the $0100 wake/flex countdown, and defers activation during elevator-door transition.</summary>
    WaitingForSamus = 1,
    /// <summary>$A7:EA00, Function_Etecoon_StartHop_BottomOfRoom: waits for the eleven-update hop countdown, then starts the bottom-room jump at signed 16.16 Y velocity -3 pixels per update.</summary>
    PreJumpCountdown,
    /// <summary>$A7:EA37, Function_Etecoon_Hopping_BottomOfRoom: hops vertically, clears upward velocity on ceiling contact, and chooses a Samus-facing pause or another hop on downward collision.</summary>
    InitialJump,
    /// <summary>$A7:EAB5, Function_Etecoon_LookAtSamus: waits through the $20 look-at countdown, then installs leftward or rightward running at two pixels per update.</summary>
    FaceSamusPause,
    /// <summary>$A7:EB02, Function_Etecoon_RunningLeft: moves left using terrain collision and reverses into rightward running when it hits a wall.</summary>
    RunLeftToWall,
    /// <summary>$A7:EB2C, Function_Etecoon_RunningRight: probes 32 room pixels rightward for the solid-block high bit and starts jumping when the wall is near.</summary>
    RunRightToWall,
    /// <summary>$A7:EB50, Function_Etecoon_Jumping: resolves horizontal motion before vertical motion, enters the eight-update wall-jump pause on wall contact, and routes the actor after landing.</summary>
    Airborne,
    /// <summary>$A7:EBCD, Function_Etecoon_WallJump: waits for the wall-pause timer, installs the opposite-facing wall-jump list, and launches with signed Y velocity -3 and X velocity -2 or +2.</summary>
    WallPause,
    /// <summary>$A7:EC1B, Function_Etecoon_LandedFromJump: after the eleven-update landing wait, selects the left run-up, right run-up, or immediate top-room hop from population parameter two's route byte.</summary>
    RouteAfterLanding,
    /// <summary>$A7:EC97, Function_Etecoon_RunToLeftRunUpPoint: moves left until whole room X is below 537, then enters the top-room hopping countdown.</summary>
    MoveLeftToTeachingStart,
    /// <summary>$A7:ECBB, Function_Etecoon_RunToRightRunUpPoint: moves right until whole room X reaches 600, then enters the top-room hopping countdown.</summary>
    MoveRightToTeachingStart,
    /// <summary>$A7:ECDF, Function_Etecoon_RunningForSuccessfulMorphTunnelJump: runs right to room X 600 and begins the successful tunnel jump at signed Y velocity -4 pixels per update.</summary>
    RunRightToLongJump,
    /// <summary>$A7:ED09, Function_Etecoon_SuccessfulMorphTunnelJump: applies horizontal and vertical motion until room X reaches 680, then switches to the tunnel-running list.</summary>
    LongJump,
    /// <summary>$A7:ED2A, Function_Etecoon_RunningThroughMorphTunnel: runs right to room X 840, then selects the ledge-fall jump list with signed Y velocity -1 pixel per update.</summary>
    RunRightAfterLongJump,
    /// <summary>$A7:ED54, Function_Etecoon_FallingFromMorphTunnelLedge: moves right and vertically from the tunnel ledge until terrain collision starts the next eleven-update top-room hop wait.</summary>
    LongJumpLanding,
    /// <summary>$A7:ED75, Function_Etecoon_Hopping_TopOfRoom: performs the top-room vertical hop; downward collision may select route two's Samus-proximity wait, while upward collision clears vertical velocity.</summary>
    TeachingJump,
    /// <summary>$A7:EDC7, Function_Etecoon_StartHop_TopOfRoom: counts four teaching hops in parameter one's high byte; route two repeats hop groups, while the other routes begin the failed tunnel-jump run.</summary>
    IdleBetweenTeachingJumps,
    /// <summary>$A7:EE3E, Function_Etecoon_HopUntilSamusIsNear: after the hop wait, starts the successful tunnel run when Samus is within 64 vertical and 48 horizontal room pixels, otherwise hops again.</summary>
    WaitForSamusBeforeLongRun,
    /// <summary>$A7:EE9A, Function_Etecoon_RunningForFailedMorphTunnelJump: runs right to room X 600 before selecting the jump that returns the other routes to the wall-jump demonstration.</summary>
    RunRightToReturnJump,
    /// <summary>$A7:EEB8, Function_Etecoon_FailedTunnelJump: applies horizontal and vertical motion until collision, then restores leftward running and the ordinary -3-pixel jump velocity.</summary>
    ReturnJump,
}
