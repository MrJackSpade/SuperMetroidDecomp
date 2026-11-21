using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A sequenced room object whose animations raise no event (#2553): <see cref="NoAnimationCue"/> has no member, so it plays
/// them silently, with nothing to carry an event out.
/// </summary>
/// <typeparam name="TSegment">The segments its animations play.</typeparam>
/// <typeparam name="TFrame">The frames its animations show.</typeparam>
internal abstract class SilentSequencedRoomObject<TSegment, TFrame> : SequencedRoomObject<TSegment, TFrame, NoAnimationCue>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>Plays its animations from <paramref name="segments"/>.</summary>
    private protected SilentSequencedRoomObject(AnimationSegments<TSegment, TFrame, NoAnimationCue> segments)
        : base(segments)
    {
    }
}
