using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A sequenced room object whose animations raise no event (#2553): <see cref="NoAnimationCue"/> has no member, so no event
/// ever reaches it to carry out.
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

    /// <inheritdoc/>
    /// <remarks><see cref="NoAnimationCue"/> has no member, so there is no event to carry out.</remarks>
    private protected sealed override void Raise(NoAnimationCue raised, RoomObjectPlace place, RoomObjectStepContext context)
    {
    }
}
