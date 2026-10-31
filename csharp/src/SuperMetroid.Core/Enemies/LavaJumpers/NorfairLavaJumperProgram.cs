namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="NorfairLavaJumperAnimations"/>.
/// </summary>
internal enum NorfairLavaJumperProgram
{
    /// <summary><c>AnimationScript_NorfairLavaJumper_Hidden</c>.</summary>
    Hidden,
    /// <summary><c>AnimationScript_NorfairLavaJumper_Jump</c>.</summary>
    Jump,
    /// <summary><c>AnimationScript_NorfairLavaJumper_Follower</c>.</summary>
    Follower,
    /// <summary>The loop.</summary>
    FollowerLoop,
}
