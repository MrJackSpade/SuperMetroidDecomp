using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A door cap that plays authored sequences of its images (#1554): the blue, grey and coloured caps. Each step draws an
/// image of the cap in its paint and facing, requesting the step's sound as it draws (<see cref="SequencedRoomObject{TStep}"/>).
/// </summary>
/// <param name="orientation">The cap's facing.</param>
/// <param name="paint">The paint its closed images show.</param>
/// <param name="door">The door the cap's placement named, or the one the door it took over from named; null for none.</param>
internal abstract class DoorCapObject(ColoredDoorOrientation orientation, DoorCapPaint paint, PlacedDoor? door)
    : SequencedRoomObject<DoorCapStep>, IDoorKeptOpen
{
    /// <summary>The door the cap stands in, as its placement or the door it took over from named it; null for none.</summary>
    internal PlacedDoor? Door { get; } = door;

    /// <summary>A cap whose door has been opened for good stays open behind Samus.</summary>
    public virtual bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood, ShotOpenedCapClosing<LockedDoor> shotCapClosing) =>
        Door?.IsOpenedForGood(openedForGood) == true;

    /// <summary>The cap's facing.</summary>
    internal ColoredDoorOrientation Orientation { get; } = orientation;

    /// <summary>The paint the cap's closed images show.</summary>
    internal DoorCapPaint Paint { get; } = paint;

    /// <inheritdoc/>
    private protected sealed override int Show(DoorCapStep step, RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (step.SoundBefore is { } sound)
            context.QueueSound(sound, DoorCapSequenceDefinitions.SoundPriority);
        context.Draw(place, DoorCaps.Image(Paint, Orientation, step.Image));
        return step.Updates;
    }
}
