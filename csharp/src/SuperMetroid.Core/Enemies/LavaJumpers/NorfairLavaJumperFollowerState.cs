using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The state of a Squeept's cosmetic follower, the second half its room lists right after the jumping parent: only its
/// animation. It shows while its parent rises and reads everything else from the parent.
/// </summary>
public sealed class NorfairLavaJumperFollowerState
{
    /// <summary>Starts the follower's state with the parent it found when it was placed.</summary>
    /// <param name="parent">The parent's state.</param>
    internal NorfairLavaJumperFollowerState(SqueeptState parent) => Parent = Ensure.NotNull(parent);

    /// <summary>Where the follower's looping animation resumes.</summary>
    internal AnimationPlayback<NorfairLavaJumperSegment> Playback { get; set; }

    /// <summary>
    /// The parent the follower found when it was placed, the lava jumper its room lists right before it (#2554): its
    /// state as it moves on, and as a deletion without a kill left it.
    /// </summary>
    internal SqueeptState Parent { get; }
}
