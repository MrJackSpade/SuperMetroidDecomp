using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The state of a Squeept's cosmetic follower, the second half its room lists right after the jumping parent: only its
/// animation. It shows while its parent rises and reads everything else from the parent.
/// </summary>
public sealed class NorfairLavaJumperFollowerState
{
    /// <summary>Creates the follower's state, following the parent it found as it was placed.</summary>
    /// <param name="parent">The parent's state.</param>
    /// <param name="parentPlace">The parent's place.</param>
    internal NorfairLavaJumperFollowerState(SqueeptState parent, RoomEnemyPlace parentPlace)
    {
        Parent = parent;
        ParentPlace = Ensure.NotNull(parentPlace);
    }

    /// <summary>Where the follower's looping animation resumes.</summary>
    internal AnimationPlayback<NorfairLavaJumperSegment> Playback { get; set; }

    /// <summary>The state of the parent the follower found as it was placed, which it holds from its setup (#2554).</summary>
    internal SqueeptState Parent { get; }

    /// <summary>
    /// The place of the parent the follower found as it was placed, the enemy its room lists directly before it, whatever
    /// that place holds now.
    /// </summary>
    internal RoomEnemyPlace ParentPlace { get; }
}
