using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One Tourian entrance statue's own state (#1476). Its setup starts the animation every time, so nothing it holds is
/// inherited from an earlier occupant of its slot.
/// </summary>
/// <param name="playback">The statue's animation, before its first play.</param>
internal sealed class TourianEntranceStatueState(AnimationPlayback<TourianEntranceStatueSegment> playback)
{
    /// <summary>The statue's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<TourianEntranceStatueSegment> Playback { get; set; } = playback;
}
