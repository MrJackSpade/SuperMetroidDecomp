namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which branch a <see cref="TorizoCue.Branch"/> cue is. A branch, call or link to another body program.</summary>
internal enum TorizoBranchKind
{
    /// <summary>Saves the target as the return point (<c>Instruction_Torizo_LinkInstructionInY</c>, $AA:C2ED).</summary>
    Link,
    /// <summary>Continues at the target while rising (<c>Instruction_Torizo_GotoY_IfRising</c>, $AA:C55A).</summary>
    GoToIfRising,
    /// <summary>Calls the target when Samus is close behind (<c>Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront</c>, $AA:C567).</summary>
    CallIfSamusCloseBehind,
    /// <summary>Jumps back to the target when Samus is close in front (<c>Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels</c>, $AA:C58B).</summary>
    JumpBackIfSamusCloseInFront,
    /// <summary>Continues at the target until the body reaches the ground (<c>Instruction_Torizo_GotoY_IfNotHitGround</c>, $AA:CACE).</summary>
    GoToIfAirborne,
    /// <summary>Continues at the target while an eye beam is active (<c>Instruction_GoldenTorizo_EyeBeamAttack_0</c>, $AA:D0F3).</summary>
    GoToIfEyeBeamActive,
    /// <summary>Continues at the target when Samus is morphed behind the body (<c>Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo</c>, $AA:D3EA).</summary>
    GoToIfSamusMorphedBehind,
    /// <summary>Calls the target, at random, when Samus is morphed in front (<c>Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo</c>, $AA:D445).</summary>
    CallSometimesIfSamusMorphedInFront,
    /// <summary>Calls the target, at random, when health is low (<c>Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789</c>, $AA:D474).</summary>
    CallSometimesIfHealthLow,
    /// <summary>Calls the target when the stun health is high (<c>Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31</c>, $AA:D49B).</summary>
    CallIfStunHealthHigh,
    /// <summary>Jumps forward to the target when Samus is far (<c>Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel</c>, $AA:D4BA).</summary>
    JumpForwardIfSamusFar,
    /// <summary>Jumps back to the target when Samus is close in front of Golden Torizo (<c>Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels</c>, $AA:D4FD).</summary>
    JumpBackIfSamusCloseInFrontGolden,
}
