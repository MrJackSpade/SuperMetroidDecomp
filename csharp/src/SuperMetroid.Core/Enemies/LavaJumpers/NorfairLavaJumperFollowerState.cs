using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The state of a Squeept's cosmetic follower, the second half its room lists right after the jumping parent: only its
/// animation. It shows while its parent rises and reads everything else from the parent.
/// </summary>
public sealed partial class NorfairLavaJumperFollowerState
{
    /// <summary>Where the follower's looping animation resumes.</summary>
    internal AnimationPlayback<NorfairLavaJumperSegment> Playback { get; set; }
}
