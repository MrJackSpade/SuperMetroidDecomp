namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="NorfairLavaJumperAnimations"/>.
/// </summary>
internal enum NorfairLavaJumperSegment
{
    /// <summary>The hidden animation.</summary>
    Hidden,
    /// <summary>The jump animation.</summary>
    Jump,
    /// <summary>The follower animation.</summary>
    Follower,
    /// <summary>The loop.</summary>
    FollowerLoop,
}
