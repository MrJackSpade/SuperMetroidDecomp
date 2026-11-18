using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A door cap that plays authored animations of its images (#1554, <see cref="DoorCapSequenceDefinitions"/>): the blue,
/// grey and coloured caps. Each picture draws an image of the cap in its paint and facing, and each sound its animation
/// passes is requested just before the picture after it draws (<see cref="SequencedRoomObject{TSegment, TFrame, TEvent}"/>).
/// </summary>
/// <param name="orientation">The cap's facing.</param>
/// <param name="paint">The paint its closed images show.</param>
internal abstract class DoorCapObject(ColoredDoorOrientation orientation, DoorCapPaint paint)
    : SequencedRoomObject<DoorCapSegment, DoorCapFrame, SoundEffectId>(DoorCapSequenceDefinitions.Segments), IDoorBehindSamus
{
    /// <inheritdoc/>
    public abstract bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood);

    /// <inheritdoc/>
    public abstract void CloseBehindSamus();

    /// <summary>The cap's facing.</summary>
    internal ColoredDoorOrientation Orientation { get; } = orientation;

    /// <summary>The paint the cap's closed images show.</summary>
    internal DoorCapPaint Paint { get; } = paint;

    /// <inheritdoc/>
    private protected sealed override void Show(DoorCapFrame frame, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.Draw(place, DoorCaps.Image(Paint, Orientation, frame));

    /// <inheritdoc/>
    private protected sealed override void Raise(SoundEffectId raised, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.QueueSound(raised, DoorCapSequenceDefinitions.SoundPriority);
}
