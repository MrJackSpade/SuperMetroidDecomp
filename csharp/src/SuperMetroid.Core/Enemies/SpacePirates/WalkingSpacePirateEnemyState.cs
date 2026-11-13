using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one walking Space Pirate. The patrol posts are room positions that wrap at sixteen bits, because
/// population parameter two is added and subtracted without clamping by the initializer; the walk compares the Pirate
/// with them by the wrapped spatial difference.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WalkingSpacePirateSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WalkingSpacePirateFunction? Function { get; internal set; }

    /// <summary>
    /// Left patrol post. Only the X axis is stored and meaningful (Y reads zero): a leftward walk turns back once the
    /// Pirate passes it.
    /// </summary>
    public Position16<RoomPixels> LeftPost
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>
    /// Right patrol post. Only the X axis is stored and meaningful (Y reads zero): a rightward walk turns back once the
    /// Pirate reaches it.
    /// </summary>
    public Position16<RoomPixels> RightPost
    {
        get;
        internal set => field = new(value.X, 0);
    }
}
