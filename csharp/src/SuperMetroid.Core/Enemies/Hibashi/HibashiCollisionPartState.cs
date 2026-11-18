using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// A Hibashi's invisible collision part, placed right after its flame: it takes no action of its own and holds its
/// one-frame hitbox animation; the flame moves and sizes it.
/// </summary>
public sealed class HibashiCollisionPartState
{
    /// <summary>The hitbox animation the part holds.</summary>
    internal AnimationPlayback<HibashiSegment> Playback { get; set; }
}
