using Engine.Actors;

namespace Game;

/// <summary>
/// The names of the ways Samus looks, one for each thing she can be doing. Her timelines, and the heights her shots
/// leave from, are named by them.
/// </summary>
public static class SamusLooks
{
    /// <summary>The look of the state <paramref name="samus"/> is in.</summary>
    public static string Of(Samus samus) => samus.State switch
    {
        ISamusLook own => own.Look,
        StandingState => "standing",
        RunningState => "running",
        TurningState => "turning",
        CrouchingState => "crouching",
        JumpingState => "jumping",
        FallingState => "falling",
        WallJumpingState => "wall_jumping",
        CrawlingState => "morph_ball",
        KnockedBackState => samus.Form == Player.Crawling ? "morph_ball" : "knocked_back",
        DyingState => "dying",
        CarriedState => "carried",
        _ => samus.Form == Player.Crawling ? "morph_ball" : "standing",
    };
}
