using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The state of the Multiviola. Retail recomputes its velocity from the population angle and speed every active frame,
/// with two deliberately imperfect reflection negations.
/// </summary>
public sealed class MultiviolaEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MultiviolaSegment> Playback { get; set; }

    /// <summary>Signed 16.16 velocity in room pixels per update.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The flight heading, from its spawn settings, bounced into the neighbouring quarter turn at each wall or floor
    /// (<see cref="MultiviolaQuarterBounce"/>); its velocity is rebuilt from it every update.
    /// </summary>
    public TrigonometryAngle Heading { get; internal set; }

    /// <summary>The flight speed in whole pixels per update, from its spawn settings, split by the heading's sine and cosine.</summary>
    public int Speed { get; internal set; }
}
