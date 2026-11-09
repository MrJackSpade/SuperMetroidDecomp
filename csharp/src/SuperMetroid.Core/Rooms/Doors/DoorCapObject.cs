using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A door cap that plays authored sequences of its images (#1554): the blue, grey and coloured caps. Each step draws an
/// image of the cap in its paint and facing, requesting the step's sound as it draws (<see cref="SequencedRoomObject{TStep}"/>).
/// </summary>
/// <param name="orientation">The cap's facing.</param>
/// <param name="paint">The paint its closed images show.</param>
internal abstract class DoorCapObject(ColoredDoorOrientation orientation, DoorCapPaint paint) : SequencedRoomObject<DoorCapStep>
{
    /// <summary>The cap's facing.</summary>
    internal ColoredDoorOrientation Orientation { get; } = orientation;

    /// <summary>The paint the cap's closed images show.</summary>
    internal DoorCapPaint Paint { get; } = paint;

    /// <inheritdoc/>
    private protected sealed override int Show(DoorCapStep step, RoomObjectSlot place, RoomObjectStepContext context)
    {
        if (step.SoundBefore is { } sound)
            context.QueueSound(sound, DoorCapSequenceDefinitions.SoundPriority);
        context.Draw(place, DoorCaps.Image(Paint, Orientation, step.Image));
        return step.Updates;
    }
}
