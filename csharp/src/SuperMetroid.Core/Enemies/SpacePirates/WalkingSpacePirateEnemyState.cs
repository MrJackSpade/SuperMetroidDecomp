using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one walking Space Pirate. The patrol posts are room positions that wrap at sixteen bits, because
/// its patrol reach is added and subtracted without clamping by the initializer; the walk compares the Pirate
/// with them by the wrapped spatial difference.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal WalkingSpacePirateEnemyState(WalkingSpacePirateAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WalkingSpacePirateSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WalkingSpacePirateAction Action { get; internal set; }

    /// <summary>The left patrol post's room column: a leftward walk turns back once the Pirate passes it.</summary>
    public int LeftPostX { get; internal set; }

    /// <summary>The right patrol post's room column: a rightward walk turns back once the Pirate reaches it.</summary>
    public int RightPostX { get; internal set; }
}
