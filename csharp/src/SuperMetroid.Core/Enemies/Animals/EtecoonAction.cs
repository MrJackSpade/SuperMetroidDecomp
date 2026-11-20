namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The behaviour an Etecoon's main AI runs each frame. The names describe the actor-side work of each
/// behaviour; animation is owned by the Etecoon's typed animations.
/// </summary>
public enum EtecoonAction
{
    /// <summary>
    /// Lies dormant until Samus comes within its wake distance (<see cref="EtecoonRouteDefinitions.ActivationYDistance"/>, #1627), when it starts flexing; a door transition
    /// defers the wake.
    /// </summary>
    Dormant,
    /// <summary>
    /// Flexes for <see cref="EtecoonTimingDefinitions.FlexDuration"/> updates after waking, then crouches for its first hop; a
    /// door transition holds the countdown.
    /// </summary>
    Flexing,
    /// <summary>Waits out its crouch before the hop (<see cref="EtecoonTimingDefinitions.HopCrouch"/>), then starts the bottom-room jump upward at <see cref="EtecoonMotionDefinitions.HopSpeed"/>.</summary>
    PreJumpCountdown,
    /// <summary>Hops vertically, clears upward velocity on ceiling contact, and chooses a Samus-facing pause or another hop on downward collision.</summary>
    InitialJump,
    /// <summary>Looks at Samus for <see cref="EtecoonTimingDefinitions.FaceSamusPause"/>, then runs left or right.</summary>
    FaceSamusPause,
    /// <summary>Moves left using terrain collision and reverses into rightward running when it hits a wall.</summary>
    RunLeftToWall,
    /// <summary>Probes its lookahead rightward (<see cref="EtecoonMotionDefinitions.RightWallLookahead"/>) for a block that stops attachment and starts jumping when the wall is near.</summary>
    RunRightToWall,
    /// <summary>Resolves horizontal motion before vertical motion, enters the wall-jump pause (<see cref="EtecoonTimingDefinitions.WallPause"/>) on wall contact, and routes the actor after landing.</summary>
    Airborne,
    /// <summary>Waits for the wall-pause timer, starts the opposite-facing wall-jump animation, and launches up at its hop speed and away from the wall at its run speed.</summary>
    WallPause,
    /// <summary>After its crouch on landing (<see cref="EtecoonTimingDefinitions.HopCrouch"/>), selects the left run-up, right run-up, or immediate top-room hop from its spawn route.</summary>
    RouteAfterLanding,
    /// <summary>Moves left until whole room X is below <see cref="EtecoonRouteDefinitions.FirstTeachingX"/>, then enters the top-room hopping countdown.</summary>
    MoveLeftToTeachingStart,
    /// <summary>Moves right until whole room X reaches <see cref="EtecoonRouteDefinitions.RunStartX"/>, then enters the top-room hopping countdown.</summary>
    MoveRightToTeachingStart,
    /// <summary>Runs right to <see cref="EtecoonRouteDefinitions.RunStartX"/> and begins the successful tunnel jump upward at <see cref="EtecoonMotionDefinitions.LongJumpSpeed"/>.</summary>
    RunRightToLongJump,
    /// <summary>Applies horizontal and vertical motion until room X reaches <see cref="EtecoonRouteDefinitions.LongJumpMidpointX"/>, then switches to the tunnel-running animation.</summary>
    LongJump,
    /// <summary>Runs right to <see cref="EtecoonRouteDefinitions.LongJumpEndX"/>, then selects the ledge-fall jump animation upward at <see cref="EtecoonMotionDefinitions.LongJumpLandingSpeed"/>.</summary>
    RunRightAfterLongJump,
    /// <summary>Moves right and vertically from the tunnel ledge until terrain collision starts its crouch before the next top-room hop (<see cref="EtecoonTimingDefinitions.HopCrouch"/>).</summary>
    LongJumpLanding,
    /// <summary>Performs the top-room vertical hop; downward collision may select the teach-at-once route's Samus-proximity wait, while upward collision clears vertical velocity.</summary>
    TeachingJump,
    /// <summary>Counts a lesson's teaching hops (<see cref="EtecoonRouteDefinitions.TeachingHopsPerLesson"/>); the teach-at-once route repeats hop groups, while the other routes begin the failed tunnel-jump run.</summary>
    IdleBetweenTeachingJumps,
    /// <summary>After the hop wait, starts the successful tunnel run when Samus is near (<see cref="EtecoonRouteDefinitions.NearbyDistance"/>, <see cref="EtecoonRouteDefinitions.LongRunTriggerDistance"/>), otherwise hops again.</summary>
    WaitForSamusBeforeLongRun,
    /// <summary>Runs right to <see cref="EtecoonRouteDefinitions.RunStartX"/> before selecting the jump that returns the other routes to the wall-jump demonstration.</summary>
    RunRightToReturnJump,
    /// <summary>Applies horizontal and vertical motion until collision, then restores leftward running and the ordinary hop speed.</summary>
    ReturnJump,
}
