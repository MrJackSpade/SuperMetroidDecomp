namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which branch a <see cref="TorizoCue.Branch"/> cue is. A branch, call or link to another body animation.</summary>
internal enum TorizoBranchKind
{
    /// <summary>Saves the target as the return point.</summary>
    Link,
    /// <summary>Continues at the target while rising.</summary>
    GoToIfRising,
    /// <summary>Calls the target when Samus is close behind.</summary>
    CallIfSamusCloseBehind,
    /// <summary>Jumps back to the target when Samus is close in front.</summary>
    JumpBackIfSamusCloseInFront,
    /// <summary>Continues at the target until the body reaches the ground.</summary>
    GoToIfAirborne,
    /// <summary>Continues at the target while an eye beam is active.</summary>
    GoToIfEyeBeamActive,
    /// <summary>Continues at the target when Samus is morphed behind the body.</summary>
    GoToIfSamusMorphedBehind,
    /// <summary>Calls the target, at random, when Samus is morphed in front.</summary>
    CallSometimesIfSamusMorphedInFront,
    /// <summary>Calls the target, at random, when health is low.</summary>
    CallSometimesIfHealthLow,
    /// <summary>Calls the target when the stun health is high.</summary>
    CallIfStunHealthHigh,
    /// <summary>Jumps forward to the target when Samus is far.</summary>
    JumpForwardIfSamusFar,
    /// <summary>Jumps back to the target when Samus is close in front of Golden Torizo.</summary>
    JumpBackIfSamusCloseInFrontGolden,
}
