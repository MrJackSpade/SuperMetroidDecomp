using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A blue door cap (#1554): one a shot opens, one closing behind Samus, or one
/// a coloured, grey or eye door turns into once its door has been opened for good. A shot cap opens
/// (<see cref="DoorCapSequenceDefinitions.BlueOpening"/>) and leaves the room. A closing cap closes
/// (<see cref="DoorCapSequenceDefinitions.BlueClosing"/>), then, like a cap another door turns into, makes its block
/// shootable again, shows its closed image (<see cref="DoorCapSequenceDefinitions.BlueClosed"/>) and leaves the room.
/// </summary>
internal sealed class BlueDoorCapObject : DoorCapObject
{
    /// <summary>A cap a shot opens, from its next update.</summary>
    /// <param name="orientation">The cap's facing.</param>
    internal static BlueDoorCapObject Opening(ColoredDoorOrientation orientation) =>
        new(orientation, BlueDoorCapPhase.Opening, DoorCapSequenceDefinitions.BlueOpening);

    /// <summary>A cap closing behind Samus, from its next update.</summary>
    /// <param name="orientation">The cap's facing.</param>
    internal static BlueDoorCapObject Closing(ColoredDoorOrientation orientation) =>
        new(orientation, BlueDoorCapPhase.Closing, DoorCapSequenceDefinitions.BlueClosing);

    /// <summary>A cap that plays <paramref name="sequence"/> from its next update, standing in no door of its own.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="phase">Whether it opens or closes.</param>
    /// <param name="sequence">The opening or the closing it plays.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, BlueDoorCapPhase phase, IReadOnlyList<DoorCapStep> sequence)
        : base(orientation, DoorCapPaint.Blue, door: null)
    {
        Phase = phase;
        OpenedByShot = phase == BlueDoorCapPhase.Opening;
        PlayFromNextUpdate(sequence);
    }

    /// <summary>A cap another door turns into, which its takeover closes at once, standing in that door's door.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="door">The door the door it takes over from was placed in; null for none.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, PlacedDoor? door)
        : base(orientation, DoorCapPaint.Blue, door) => Phase = BlueDoorCapPhase.Closed;

    /// <summary>What the cap is doing.</summary>
    internal BlueDoorCapPhase Phase { get; private set; }

    /// <summary>
    /// Whether a shot opened the cap: it names no door of its own, and whether it closes behind Samus is the room's
    /// <see cref="ShotOpenedCapClosing{TDoor}"/> rule's (#1906).
    /// </summary>
    internal bool OpenedByShot { get; }

    /// <inheritdoc/>
    public override bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood, ShotOpenedCapClosing<LockedDoor> shotCapClosing) =>
        OpenedByShot
            ? Ensure.NotNull(shotCapClosing).StaysOpen(openedForGood)
            : base.StaysOpenBehindSamus(openedForGood, shotCapClosing);

    /// <summary>
    /// Turns the door at <paramref name="place"/>, placed in <paramref name="door"/>, into its closed blue cap facing
    /// <paramref name="orientation"/>, which makes its block shootable and draws its closed image on this same update.
    /// </summary>
    internal static void TakeOver(RoomObjectPlace place, RoomObjectStepContext context, ColoredDoorOrientation orientation,
        PlacedDoor? door)
    {
        Ensure.NotNull(place);
        var cap = new BlueDoorCapObject(orientation, door);
        place.Replace(cap);
        cap.Close(place, context);
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case BlueDoorCapPhase.Closing:
                Close(place, context);
                return;
            case BlueDoorCapPhase.Opening:
            case BlueDoorCapPhase.Closed:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined blue door cap phase {Phase}.");
        }
    }

    /// <summary>Makes the cap's block shootable again and shows its closed image.</summary>
    private void Close(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Phase = BlueDoorCapPhase.Closed;
        context.Level.SetBehavior(place.Block,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(Orientation)));
        Play(DoorCapSequenceDefinitions.BlueClosed, place, context);
    }
}
