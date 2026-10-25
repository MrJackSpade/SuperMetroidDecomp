namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The routine a ninja Space Pirate's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum NinjaSpacePirateFunction : ushort
{
    /// <summary>$B2:804B, shared return stub used while animation bytecode owns the actor.</summary>
    NoOperation = 1,
    /// <summary>$B2:F6A9, waits for Samus to enter activation range.</summary>
    Initial,
    /// <summary>$B2:F6E4, chooses flinch, kick, jump, or claw attacks.</summary>
    Active,
    /// <summary>$B2:F817, rises during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftRising,
    /// <summary>$B2:F84C, falls during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftFalling,
    /// <summary>$B2:F890, rises during a rightward post-to-post spin jump.</summary>
    SpinJumpRightRising,
    /// <summary>$B2:F8C5, falls during a rightward post-to-post spin jump.</summary>
    SpinJumpRightFalling,
    /// <summary>$B2:F909, waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick,
    /// <summary>$B2:F985, begins the leftward divekick jump.</summary>
    DivekickLeftJump,
    /// <summary>$B2:F9C1, performs the leftward collision-bearing dive.</summary>
    DivekickLeftDive,
    /// <summary>$B2:FA15, walks leftward back to the authored post.</summary>
    DivekickLeftWalkToPost,
    /// <summary>$B2:FA59, begins the rightward divekick jump.</summary>
    DivekickRightJump,
    /// <summary>$B2:FA95, performs the rightward collision-bearing dive.</summary>
    DivekickRightDive,
    /// <summary>$B2:FAE9, walks rightward back to the authored post.</summary>
    DivekickRightWalkToPost,
}
