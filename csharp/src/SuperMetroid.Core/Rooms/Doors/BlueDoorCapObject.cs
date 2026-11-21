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
        new(orientation, BlueDoorCapPhase.Opening, DoorCapSequenceDefinitions.BlueOpening,
            new CapOrigin.ShotOpenedCap(Ensure.NotNull(shotCapClosing)));

    /// <summary>A cap closing behind Samus, from its next update.</summary>
    /// <param name="orientation">The cap's facing.</param>
    internal static BlueDoorCapObject Closing(ColoredDoorOrientation orientation) =>
        new(orientation, BlueDoorCapPhase.Closing, DoorCapSequenceDefinitions.BlueClosing, CapOrigin.ClosingCap.Instance);

    /// <summary>A cap that plays <paramref name="sequence"/> from its next update, standing in no door of its own.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="phase">Whether it opens or closes.</param>
    /// <param name="sequence">The opening or the closing it plays.</param>
    /// <param name="origin">How the cap came to be: opened by a shot, or closing behind Samus.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, BlueDoorCapPhase phase, Animation<DoorCapSegment> sequence,
        CapOrigin origin)
        : base(orientation, DoorCapPaint.Blue)
    {
        Phase = phase;
        Origin = origin;
        PlayFromNextUpdate(sequence);
    }

    /// <summary>A cap another door turns into, which its takeover closes at once, standing in that door's door.</summary>
    /// <param name="orientation">The cap's facing.</param>
    /// <param name="door">The door the door it takes over from was placed in.</param>
    private BlueDoorCapObject(ColoredDoorOrientation orientation, PlacedDoor door)
        : base(orientation, DoorCapPaint.Blue)
    {
        Phase = BlueDoorCapPhase.Closed;
        Origin = new CapOrigin.TakenOverCap(Ensure.NotNull(door));
    }

    /// <summary>How the cap came to be, which decides whether it stays open behind Samus (#2560).</summary>
    private CapOrigin Origin { get; }

    /// <summary>What the cap is doing.</summary>
    private BlueDoorCapPhase Phase { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// A cap a shot opened stays open as the room's <see cref="ShotOpenedCapClosing{TDoor}"/> rule says; a cap another door
    /// turns into stays open once that door has been opened for good; a closing cap never does.
    /// </remarks>
    public override bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) => Origin.StaysOpen(openedForGood);

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

    /// <summary>What <see cref="SequenceEnded"/> does for each <see cref="BlueDoorCapPhase"/> member, each member's own arm.</summary>
    private IdentityTable<BlueDoorCapPhase, Action<RoomObjectPlace, RoomObjectStepContext>> SequenceEndedForBlueDoorCapPhase => field ??= new(new Dictionary<BlueDoorCapPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [BlueDoorCapPhase.Closing] = (place, context) => Close(place, context),
        [BlueDoorCapPhase.Opening] = (place, context) => context.RoomObjects.DeleteObject(place),
        [BlueDoorCapPhase.Closed] = (place, context) => context.RoomObjects.DeleteObject(place),
    });

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        SequenceEndedForBlueDoorCapPhase[Phase](place, context);
    }

    /// <summary>Makes the cap's block shootable again and shows its closed image.</summary>
    private void Close(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Phase = BlueDoorCapPhase.Closed;
        context.Level.SetBehavior(place.Block,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(Orientation)));
        Play(DoorCapSequenceDefinitions.BlueClosed, place, context);
    }

    /// <summary>
    /// How a blue cap came to be (#2560): a closed choice of a cap a shot opened, which carries the room's shot-cap rule, a cap
    /// closing behind Samus, and a cap another door turned into, which carries the door it stands in.
    /// </summary>
    internal abstract record CapOrigin
    {
        /// <summary>Closes the hierarchy to the origins declared here.</summary>
        private CapOrigin()
        {
        }

        /// <summary>Whether the cap stays open behind Samus, given which doors are opened for good.</summary>
        internal abstract bool StaysOpen(Func<LockedDoor, bool> openedForGood);

        /// <summary>A cap a shot opened, which names no door of its own.</summary>
        /// <param name="Rule">The room's rule for whether a cap a shot opened closes behind Samus (#1906).</param>
        internal sealed record ShotOpenedCap(ShotOpenedCapClosing<LockedDoor> Rule) : CapOrigin
        {
            /// <inheritdoc/>
            internal override bool StaysOpen(Func<LockedDoor, bool> openedForGood) => Rule.StaysOpen(openedForGood);
        }

        /// <summary>A cap closing behind Samus, which names no door of its own and never stays open.</summary>
        internal sealed record ClosingCap : CapOrigin
        {
            /// <summary>The one such origin.</summary>
            internal static ClosingCap Instance { get; } = new();

            /// <summary>Only <see cref="Instance"/> exists.</summary>
            private ClosingCap()
            {
            }

            /// <inheritdoc/>
            internal override bool StaysOpen(Func<LockedDoor, bool> openedForGood) => false;
        }

        /// <summary>A cap another door turned into once its door had been opened for good.</summary>
        /// <param name="Door">The door the cap stands in, as the door it took over from named it.</param>
        internal sealed record TakenOverCap(PlacedDoor Door) : CapOrigin
        {
            /// <inheritdoc/>
            internal override bool StaysOpen(Func<LockedDoor, bool> openedForGood) => Door.IsOpenedForGood(openedForGood);
        }
    }
}
