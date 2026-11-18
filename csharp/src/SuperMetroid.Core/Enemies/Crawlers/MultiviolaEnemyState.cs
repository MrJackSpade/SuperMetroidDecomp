using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The state of the Multiviola. It sets its velocity from its heading and speed each update, and turning a speed left or
/// up is not that speed's exact opposite (<see cref="MultiviolaLeftOrUpSpeed"/>).
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MultiviolaSegment> Playback { get; set; }

    /// <summary>The Multiviola's velocity in room pixels per update, positive right and down.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The flight heading, from its spawn settings, bounced into the neighbouring quarter turn at each wall or floor
    /// (<see cref="MultiviolaQuarterBounce"/>); its velocity is rebuilt from it every update.
    /// </summary>
    public TrigonometryAngle Heading { get; internal set; }
}
