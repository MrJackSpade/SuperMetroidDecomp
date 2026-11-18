using SuperMetroid.Core.Animation;
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
    /// <param name="shotCapClosing">The room's rule for whether a cap a shot opened closes behind Samus (#1906).</param>
    internal static BlueDoorCapObject Opening(ColoredDoorOrientation orientation, ShotOpenedCapClosing<LockedDoor> shotCapClosing) =>
        new(orientation, BlueDoorCapPhase.Opening, DoorCapSequenceDefinitions.BlueOpening, Ensure.NotNull(shotCapClosing));

    /// <summary>A cap closing behind Samus, from its next update.</summary>
    /// <param name="orientation">The cap's facing.</param>
    internal static BlueDoorCapObject Closing(ColoredDoorOrientation orientation) =>
        new(orientation, BlueDoorCapPhase.Closing, DoorCapSequenceDefinitions.BlueClosing, shotCapClosing: null);

    /// <summary>A cap that plays <paramref name="sequence"/> from its next update, standing in no door of its own.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="phase">Whether it opens or closes.</param>
    /// <param name="sequence">The opening or the closing it plays.</param>
    /// <param name="shotCapClosing">The room's shot-cap rule for a cap a shot opens; null for a closing cap.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, BlueDoorCapPhase phase, Animation<DoorCapSegment> sequence,
        ShotOpenedCapClosing<LockedDoor>? shotCapClosing)
        : base(orientation, DoorCapPaint.Blue)
    {
        Phase = phase;
        _shotCapClosing = shotCapClosing;
        PlayFromNextUpdate(sequence);
    }

    /// <summary>A cap another door turns into, which its takeover closes at once, standing in that door's door.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="door">The door the door it takes over from was placed in.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, PlacedDoor door)
        : base(orientation, DoorCapPaint.Blue)
    {
        Phase = BlueDoorCapPhase.Closed;
        Door = Ensure.NotNull(door);
    }

    /// <summary>
    /// The door the cap stands in, as the door it took over from named it; null for a cap a shot opens or one closing
    /// behind Samus, which stand in no door of their own.
    /// </summary>
    internal PlacedDoor? Door { get; }

    /// <summary>What the cap is doing.</summary>
    internal BlueDoorCapPhase Phase { get; private set; }

    /// <summary>
    /// The room's rule for whether the cap closes behind Samus when a shot opened it (#1906): such a cap names no door of its
    /// own. Null for a cap no shot opened.
    /// </summary>
    private readonly ShotOpenedCapClosing<LockedDoor>? _shotCapClosing;

    /// <summary>Whether a shot opened the cap, so whether it closes behind Samus is the room's shot-cap rule's (#1906).</summary>
    internal bool OpenedByShot => _shotCapClosing is not null;

    /// <inheritdoc/>
    /// <remarks>
    /// A cap a shot opened stays open as the room's <see cref="ShotOpenedCapClosing{TDoor}"/> rule says; a cap another door
    /// turns into stays open once that door has been opened for good; a closing cap never does.
    /// </remarks>
    public override bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        _shotCapClosing is { } rule
            ? rule.StaysOpen(openedForGood)
            : Door?.IsOpenedForGood(openedForGood) == true;

    /// <inheritdoc/>
    /// <remarks>
    /// The cap plays the blue closing from its next update, then makes its block shootable again, shows its closed image and
    /// leaves the room, as a cap closing behind Samus does (#2493): a cap a shot opened closes as the door's own closer does.
    /// </remarks>
    public override void CloseBehindSamus()
    {
        Phase = BlueDoorCapPhase.Closing;
        PlayFromNextUpdate(DoorCapSequenceDefinitions.BlueClosing);
    }

    /// <summary>
    /// Turns the door at <paramref name="place"/>, placed in <paramref name="door"/>, into its closed blue cap facing
    /// <paramref name="orientation"/>, which makes its block shootable and draws its closed image on this same update.
    /// </summary>
    internal static void TakeOver(RoomObjectPlace place, RoomObjectStepContext context, ColoredDoorOrientation orientation,
        PlacedDoor door)
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
