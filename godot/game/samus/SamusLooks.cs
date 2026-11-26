using Engine.Actors;

namespace Game;

/// <summary>
/// The names of the ways Samus looks, one for each thing she can be doing. Her timelines, and the heights her shots
/// leave from, are named by them.
/// </summary>
public static class SamusLooks
{
    /// <summary>The look of the state <paramref name="samus"/> is in.</summary>
    public static string Of(Samus samus) => Of(samus, samus.State);

    /// <summary>Whether <paramref name="samus"/> is shown facing out of the screen: in the front view's own look, or in her appearance where a saved game is taken up.</summary>
    public static bool FacesOut(Samus samus) => Of(samus) is "facing_forward" or AppearingState.Appearance;

    /// <summary>The look of <paramref name="state"/>: a change of posture has the look of what it leads to, and the picture between plays as that look is taken up.</summary>
    public static string Of(Samus samus, PlayerState state) => state switch
    {
        // A wall jump shows its launch and then the spin it goes into, through the spin state it lets go into too.
        SpinJumpingState { IsOfWallJump: true } or WallJumpingState when samus.WallJumpLook is { } wallJump => wallJump,
        ISamusLook own => own.Look,
        ChangingPostureState changing => Of(samus, changing.Then),
        TurningState { Crouched: true } => "crouching",
        UsingStationState { FacingOut: true } => "facing_forward",
        StandingState => "standing",
        WallStoppedState => "wall_stopped",
        RunningState => "running",
        TurningState => "turning",
        CrouchingState => "crouching",
        JumpingState => "jumping",
        AirTurningState turning => turning.FromJump ? "air_turning_jump" : "air_turning_fall",
        FallingState => "falling",
        WallJumpingState => "wall_jumping",
        CrawlingState => "morph_ball",
        KnockedBackState => samus.Form == Player.Crawling ? "morph_ball" : "knocked_back",
        DyingState => samus.DeathLook,

        // Held, she goes on showing what she was doing as she was taken hold of, or what her holder says she is held as.
        HeldState held => Of(samus, held.Was),
        _ => samus.Form == Player.Crawling ? "morph_ball" : "standing",
    };
}
