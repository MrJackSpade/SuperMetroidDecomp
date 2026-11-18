using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The state of one Ripper, grapple Ripper or jet Ripper: its flying animation and the way it flies. Each reads its speed,
/// and the grapple Ripper its patrol bounds, from its spawn settings where it uses them (#2450).
/// </summary>
public sealed class RipperEnemyState
{
    /// <summary>The Ripper-family typed flying loop (#641), started by setup and restarted at each reversal.</summary>
    internal AnimationPlayback<RipperSegment> Playback { get; set; }

    /// <summary>The way the enemy flies (#2278).</summary>
    public HorizontalDirection Heading { get; internal set; }
}
