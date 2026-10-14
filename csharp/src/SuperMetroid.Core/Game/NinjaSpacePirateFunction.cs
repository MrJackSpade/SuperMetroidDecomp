namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B2 function pointers stored in native enemy variable A. These values are
/// intentionally addresses, so debugger state can be compared directly with WRAM and the
/// disassembly instead of being hidden behind a host-only state number.
/// </summary>
public enum NinjaSpacePirateFunction : ushort
{
    /// <summary>$B2:804B, shared return stub used while animation bytecode owns the actor.</summary>
    NoOperation = 0x804b,
    /// <summary>$B2:F6A9, waits for Samus to enter activation range.</summary>
    Initial = 0xf6a9,
    /// <summary>$B2:F6E4, chooses flinch, kick, jump, or claw attacks.</summary>
    Active = 0xf6e4,
    /// <summary>$B2:F817, rises during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftRising = 0xf817,
    /// <summary>$B2:F84C, falls during a leftward post-to-post spin jump.</summary>
    SpinJumpLeftFalling = 0xf84c,
    /// <summary>$B2:F890, rises during a rightward post-to-post spin jump.</summary>
    SpinJumpRightRising = 0xf890,
    /// <summary>$B2:F8C5, falls during a rightward post-to-post spin jump.</summary>
    SpinJumpRightFalling = 0xf8c5,
    /// <summary>$B2:F909, waits at the jump peak for a divekick opportunity.</summary>
    ReadyToDivekick = 0xf909,
    /// <summary>$B2:F985, begins the leftward divekick jump.</summary>
    DivekickLeftJump = 0xf985,
    /// <summary>$B2:F9C1, performs the leftward collision-bearing dive.</summary>
    DivekickLeftDive = 0xf9c1,
    /// <summary>$B2:FA15, walks leftward back to the authored post.</summary>
    DivekickLeftWalkToPost = 0xfa15,
    /// <summary>$B2:FA59, begins the rightward divekick jump.</summary>
    DivekickRightJump = 0xfa59,
    /// <summary>$B2:FA95, performs the rightward collision-bearing dive.</summary>
    DivekickRightDive = 0xfa95,
    /// <summary>$B2:FAE9, walks rightward back to the authored post.</summary>
    DivekickRightWalkToPost = 0xfae9,
}
