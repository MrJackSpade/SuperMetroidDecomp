namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a ninja Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum NinjaSpacePirateFunction
{
    /// <summary>Shared return stub used while the animation owns the actor.</summary>
    NoOperation = 1,
    /// <summary>Waits for Samus to enter activation range.</summary>
    Initial,
    /// <summary>Chooses flinch, kick, jump, or claw attacks.</summary>
    Active,
    /// <summary>Rises during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftRising,
    /// <summary>Falls during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftFalling,
    /// <summary>Rises during a rightward post-to-post spin jump.</summary>
    SpinJumpRightRising,
    /// <summary>Falls during a rightward post-to-post spin jump.</summary>
    SpinJumpRightFalling,
    /// <summary>Waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick,
    /// <summary>Begins the leftward divekick jump.</summary>
    DivekickLeftJump,
    /// <summary>Performs the leftward collision-bearing dive.</summary>
    DivekickLeftDive,
    /// <summary>Walks leftward back to the authored post.</summary>
    DivekickLeftWalkToPost,
    /// <summary>Begins the rightward divekick jump.</summary>
    DivekickRightJump,
    /// <summary>Performs the rightward collision-bearing dive.</summary>
    DivekickRightDive,
    /// <summary>Walks rightward back to the authored post.</summary>
    DivekickRightWalkToPost,
}
