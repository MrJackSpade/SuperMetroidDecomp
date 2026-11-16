namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a ninja Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum NinjaSpacePirateAction
{
    /// <summary>Stands still, its main AI idle, while its animation poses it: a claw throw, a kick, a flinch or a landing.</summary>
    StandingStill,
    /// <summary>Waits for Samus to enter activation range.</summary>
    Initial,
    /// <summary>Chooses flinch, kick, jump, or claw attacks.</summary>
    Active,
    /// <summary>Rises during a post-to-post spin jump its heading's way (<see cref="NinjaSpacePirateEnemyState.Heading"/>).</summary>
    SpinJumpRising,
    /// <summary>Falls during a post-to-post spin jump its heading's way.</summary>
    SpinJumpFalling,
    /// <summary>Waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick,
    /// <summary>Begins the divekick jump toward its heading.</summary>
    DivekickJump,
    /// <summary>Performs the collision-bearing dive its heading's way.</summary>
    DivekickDive,
    /// <summary>Walks its heading's way back to the authored post.</summary>
    DivekickWalkToPost,
}
