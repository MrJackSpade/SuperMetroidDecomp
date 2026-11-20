namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations (#641).
/// Their steps are <see cref="NorfairLavaJumperAnimations"/>.
/// </summary>
internal enum NorfairLavaJumperSegment
{
    /// <summary>Its claws curled down, held while it hides in the lava.</summary>
    Hidden,
    /// <summary>
    /// Leaping from the lava: its claws curled down, spreading, spread flat, raising and folding, before the leap's
    /// pictures end.
    /// </summary>
    Jump,
    /// <summary>
    /// A trailing flame flaring for an update and vanishing for one, before it flickers; its animation repeats both.
    /// </summary>
    Follower,
    /// <summary>A trailing flame flickering small and then tall, each picture an update with an update of nothing after it.</summary>
    FollowerFlickering,
}
