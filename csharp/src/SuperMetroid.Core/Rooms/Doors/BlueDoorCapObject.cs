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
    /// <summary>A cap that a shot opens or that closes behind Samus, from its next update.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="phase">Whether it opens or closes.</param>
    internal BlueDoorCapObject(ColoredDoorOrientation orientation, BlueDoorCapPhase phase)
        : base(orientation, DoorCapPaint.Blue)
    {
        Phase = phase;
        PlayFromNextUpdate(phase switch
        {
            BlueDoorCapPhase.Opening => DoorCapSequenceDefinitions.BlueOpening,
            BlueDoorCapPhase.Closing => DoorCapSequenceDefinitions.BlueClosing,
            BlueDoorCapPhase.Closed => throw new ArgumentOutOfRangeException(nameof(phase), phase,
                "A closed blue cap only takes over from a door that turns blue."),
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined blue door cap phase."),
        });
    }

    /// <summary>A cap another door turns into, which its takeover closes at once.</summary>
    /// <param name="orientation">The cap's facing.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation)
        : base(orientation, DoorCapPaint.Blue) => Phase = BlueDoorCapPhase.Closed;

    /// <summary>What the cap is doing.</summary>
    internal BlueDoorCapPhase Phase { get; private set; }

    /// <summary>
    /// Turns the door at <paramref name="place"/> into its closed blue cap facing <paramref name="orientation"/>, which
    /// makes its block shootable and draws its closed image on this same update.
    /// </summary>
    internal static void TakeOver(RoomObjectSlot place, RoomObjectStepContext context, ColoredDoorOrientation orientation)
    {
        Ensure.NotNull(place);
        var cap = new BlueDoorCapObject(orientation);
        place.Replace(cap);
        cap.Close(place, context);
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context)
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
    private void Close(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Phase = BlueDoorCapPhase.Closed;
        context.Level.SetBehavior(place.Block,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(Orientation)));
        Play(DoorCapSequenceDefinitions.BlueClosed, place, context);
    }
}
