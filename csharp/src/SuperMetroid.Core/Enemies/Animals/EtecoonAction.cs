namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The behaviour an Etecoon's main AI runs each frame. The names describe the actor-side work of each
/// behaviour; animation is owned by the Etecoon's typed animations.
/// Its values are arbitrary.
/// </summary>
public enum EtecoonAction
{
    /// <summary>Waits for Samus within 128 vertical room pixels, runs the 256-frame wake/flex countdown, and defers activation during elevator-door transition.</summary>
    WaitingForSamus = 1,
    /// <summary>Waits for the eleven-update hop countdown, then starts the bottom-room jump at signed 16.16 Y velocity -3 pixels per update.</summary>
    PreJumpCountdown,
    /// <summary>Hops vertically, clears upward velocity on ceiling contact, and chooses a Samus-facing pause or another hop on downward collision.</summary>
    InitialJump,
    /// <summary>Waits through the 32-update look-at countdown, then installs leftward or rightward running at two pixels per update.</summary>
    FaceSamusPause,
    /// <summary>Moves left using terrain collision and reverses into rightward running when it hits a wall.</summary>
    RunLeftToWall,
    /// <summary>Probes 32 room pixels rightward for a block that stops attachment and starts jumping when the wall is near.</summary>
    RunRightToWall,
    /// <summary>Resolves horizontal motion before vertical motion, enters the eight-update wall-jump pause on wall contact, and routes the actor after landing.</summary>
    Airborne,
    /// <summary>Waits for the wall-pause timer, starts the opposite-facing wall-jump animation, and launches with signed Y velocity -3 and X velocity -2 or +2.</summary>
    WallPause,
    /// <summary>After the eleven-update landing wait, selects the left run-up, right run-up, or immediate top-room hop from its spawn route.</summary>
    RouteAfterLanding,
    /// <summary>Moves left until whole room X is below 537, then enters the top-room hopping countdown.</summary>
    MoveLeftToTeachingStart,
    /// <summary>Moves right until whole room X reaches 600, then enters the top-room hopping countdown.</summary>
    MoveRightToTeachingStart,
    /// <summary>Runs right to room X 600 and begins the successful tunnel jump at signed Y velocity -4 pixels per update.</summary>
    RunRightToLongJump,
    /// <summary>Applies horizontal and vertical motion until room X reaches 680, then switches to the tunnel-running animation.</summary>
    LongJump,
    /// <summary>Runs right to room X 840, then selects the ledge-fall jump animation with signed Y velocity -1 pixel per update.</summary>
    RunRightAfterLongJump,
    /// <summary>Moves right and vertically from the tunnel ledge until terrain collision starts the next eleven-update top-room hop wait.</summary>
    LongJumpLanding,
    /// <summary>Performs the top-room vertical hop; downward collision may select route two's Samus-proximity wait, while upward collision clears vertical velocity.</summary>
    TeachingJump,
    /// <summary>Counts four teaching hops; route two repeats hop groups, while the other routes begin the failed tunnel-jump run.</summary>
    IdleBetweenTeachingJumps,
    /// <summary>After the hop wait, starts the successful tunnel run when Samus is within 64 vertical and 48 horizontal room pixels, otherwise hops again.</summary>
    WaitForSamusBeforeLongRun,
    /// <summary>Runs right to room X 600 before selecting the jump that returns the other routes to the wall-jump demonstration.</summary>
    RunRightToReturnJump,
    /// <summary>Applies horizontal and vertical motion until collision, then restores leftward running and the ordinary -3-pixel jump velocity.</summary>
    ReturnJump,
}
