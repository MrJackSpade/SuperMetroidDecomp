namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="NorfairLavaJumperAnimations"/>.
/// </summary>
internal enum NorfairLavaJumperProgram
{
    /// <summary><c>InstructionList_NorfairLavaJumper_Hidden</c> at $A2:BE3C.</summary>
    Hidden,
    /// <summary><c>InstructionList_NorfairLavaJumper_Jump</c> at $A2:BE42.</summary>
    Jump,
    /// <summary><c>InstructionList_NorfairLavaJumper_Follower</c> at $A2:BE62.</summary>
    Follower,
    /// <summary>The loop at $A2:BE6E.</summary>
    FollowerLoop,
}
