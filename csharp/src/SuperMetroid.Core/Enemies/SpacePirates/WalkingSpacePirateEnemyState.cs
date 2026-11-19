using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one walking Space Pirate. The patrol posts are its spawn position plus and less its patrol reach,
/// unclamped, so a post may lie past the room's edge; the walk compares the Pirate with them as
/// <see cref="DistanceAcrossRoomEdge.LiesBefore"/> compares columns.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal WalkingSpacePirateEnemyState(WalkingSpacePirateAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WalkingSpacePirateSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WalkingSpacePirateAction Action { get; internal set; }

    /// <summary>The way the Pirate walks (#2165).</summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>The left patrol post's room column: a leftward walk turns back once the Pirate passes it.</summary>
    public int LeftPostX { get; internal set; }

    /// <summary>The right patrol post's room column: a rightward walk turns back once the Pirate reaches it.</summary>
    public int RightPostX { get; internal set; }
}
