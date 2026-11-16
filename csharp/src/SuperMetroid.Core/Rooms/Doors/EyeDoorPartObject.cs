using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's body or bottom (#1554). On its first update the part looks
/// at its door: one opened for good leaves the room. Otherwise it looks for Samus every eight updates, standing still
/// (<see cref="EyeDoorSequenceDefinitions.Still"/>); once she is near (<see cref="EyeDoorSequenceDefinitions.PulseReach"/>)
/// it waits for its door to open and pulses while she stays near. Once it waits, the update its door has been opened for
/// good it leaves the room, before its images count down. The body at the door Samus arrives through closes behind her
/// by leaving the room on its next update.
/// </summary>
/// <param name="orientation">The eye door's facing.</param>
/// <param name="component">Which part this is: the body or the bottom.</param>
/// <param name="door">The door the part's placement named; null for none.</param>
internal sealed class EyeDoorPartObject(EyeDoorOrientation orientation, EyeDoorComponent component, PlacedDoor? door)
    : SequencedRoomObject<EyeDoorStep>, IClosingDoor, IDoorKeptOpen
{
    /// <summary>The eye door's facing.</summary>
    internal EyeDoorOrientation Orientation { get; } = orientation;

    /// <summary>The door the part stands in, as its placement named it; null for none.</summary>
    internal PlacedDoor? Door { get; } = door;

    /// <summary>A part whose door has been opened for good stays open behind Samus.</summary>
    public bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood, ShotOpenedCapClosing<LockedDoor> shotCapClosing) =>
        Door?.IsOpenedForGood(openedForGood) == true;

    /// <summary>Which part this is: the body or the bottom.</summary>
    internal EyeDoorComponent Component { get; } = component is EyeDoorComponent.Door or EyeDoorComponent.Bottom
        ? component
        : throw new ArgumentOutOfRangeException(nameof(component), component, "An eye door's part is its body or its bottom.");

    /// <summary>Whether the part pulses while Samus stays near.</summary>
    internal bool Pulsing { get; private set; }

    /// <summary>Whether the part waits for its door to be opened for good, which makes it leave the room.</summary>
    internal bool WaitsForOpenedDoor { get; private set; }

    /// <summary>Whether the part has yet to take its first look at its door.</summary>
    private bool _placed = true;

    /// <summary>Whether the part closes behind Samus, leaving the room on its next update.</summary>
    internal bool ClosesBehindSamus { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The part is the bottom, which no door closes.</exception>
    public void CloseBehindSamus()
    {
        if (Component != EyeDoorComponent.Door)
            throw new InvalidDataException($"An eye door's {Component} has no closing behind Samus.");
        ClosesBehindSamus = true;
        EndOnNextUpdate();
    }

    /// <summary>Reacts to what the object waits for, then, unless the reaction took this update, counts its steps down.</summary>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!React(place, context))
            base.Update(place, context);
    }

    /// <inheritdoc/>
    private bool React(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (!WaitsForOpenedDoor || !context.RoomObjects.DoorObjects.IsDoorOpened(Door))
            return false;
        WaitsForOpenedDoor = false;
        context.RoomObjects.DeleteObject(place);
        return true;
    }

    /// <inheritdoc/>
    private protected override int Show(EyeDoorStep step, RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (step.Before != EyeDoorAction.None)
            throw new InvalidOperationException($"An eye door's {Component} does nothing as it draws; got {step.Before}.");
        context.Draw(place, EyeDoorDraws.Of(Orientation, Component, step.Image));
        return step.Updates;
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (ClosesBehindSamus)
        {
            context.RoomObjects.DeleteObject(place);
            return;
        }
        if (_placed)
        {
            _placed = false;
            if (context.RoomObjects.DoorObjects.IsDoorOpened(Door))
            {
                context.RoomObjects.DeleteObject(place);
                return;
            }
            Look(place, context);
            return;
        }
        if (Pulsing && IsSamusNear(place, context))
        {
            Play(EyeDoorSequenceDefinitions.Pulse[Orientation], place, context);
            return;
        }
        Look(place, context);
    }

    /// <summary>
    /// Looks for Samus: near, the part waits for its door to open and pulses; otherwise it stands still.
    /// </summary>
    private void Look(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (IsSamusNear(place, context))
        {
            WaitsForOpenedDoor = true;
            Pulsing = true;
            Play(EyeDoorSequenceDefinitions.Pulse[Orientation], place, context);
            return;
        }
        Pulsing = false;
        Play(EyeDoorSequenceDefinitions.Still, place, context);
    }

    /// <summary>Whether Samus stands near enough to make the part pulse.</summary>
    private static bool IsSamusNear(RoomObjectPlace place, RoomObjectStepContext context) =>
        context.RoomObjects.EyeDoors.IsSamusNear(place, EyeDoorSequenceDefinitions.PulseReach);
}
