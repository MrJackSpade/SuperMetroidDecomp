using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's body or bottom (#1554). On its first update the part looks
/// at its door: one opened for good leaves the room. Otherwise it looks for Samus each time its still image ends, standing still
/// (<see cref="EyeDoorSequenceDefinitions.Still"/>); once she is near (<see cref="EyeDoorSequenceDefinitions.PulseReach"/>)
/// it waits for its door to open and pulses while she stays near. Once it waits, the update its door has been opened for
/// good it leaves the room, before its images count down. The body (<see cref="EyeDoorBodyObject"/>) stands in the door
/// and closes behind Samus as she arrives through it by leaving the room on its next update; the bottom
/// (<see cref="EyeDoorBottomObject"/>) stands in no door (#2493). A part only draws its poses: its animations raise
/// nothing (#2481).
/// </summary>
internal abstract class EyeDoorPartObject
    : SilentSequencedRoomObject<EyeDoorPartSegment, EyeDoorPartPose>
{
    /// <summary>A part of <paramref name="part"/>'s kind and facing, just placed in <paramref name="door"/>.</summary>
    /// <param name="part">Which part this is, the body's middle or its bottom, and the way its door faces.</param>
    /// <param name="door">The door the part's placement named.</param>
    private protected EyeDoorPartObject(EyeDoorPart part, PlacedDoor door)
        : base(EyeDoorSequenceDefinitions.PartSegments)
    {
        Part = part;
        Door = Ensure.NotNull(door);
    }

    /// <summary>Which part this is and the way its door faces.</summary>
    private EyeDoorPart Part { get; }

    /// <summary>The eye door's facing.</summary>
    internal EyeDoorOrientation Orientation => Part.Orientation;

    /// <summary>The door the part stands in, as its placement named it.</summary>
    internal PlacedDoor Door { get; }

    /// <summary>What the part is doing (#2482).</summary>
    private EyeDoorPartPhase Phase { get; set; } = EyeDoorPartPhase.Placed;

    /// <summary>
    /// Whether the part waits for its door to be opened for good, which makes it leave the room. The wait starts the first
    /// time Samus comes near and stays when she leaves, so it is a fact of its own beside what the part shows.
    /// </summary>
    private bool WaitsForOpenedDoor { get; set; }

    /// <summary>Leaves the room on the part's next update, as the body closing behind Samus does.</summary>
    private protected void LeaveOnNextUpdate()
    {
        Phase = EyeDoorPartPhase.ClosingBehindSamus;
        EndOnNextUpdate();
    }

    /// <summary>Reacts to what the object waits for, then, unless the reaction took this update, counts its steps down.</summary>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!React(place, context))
            base.Update(place, context);
    }

    /// <summary>The part leaves once its door has been opened for good; returns whether it left.</summary>
    private bool React(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (!WaitsForOpenedDoor || !context.RoomObjects.DoorObjects.IsDoorOpened(Door))
            return false;
        WaitsForOpenedDoor = false;
        context.RoomObjects.DeleteObject(place);
        return true;
    }

    /// <inheritdoc/>
    private protected override void Show(EyeDoorPartPose frame, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.Draw(place, EyeDoorDraws.Of(Part, frame));

    /// <summary>What <see cref="SequenceEnded"/> does for each <see cref="EyeDoorPartPhase"/> member, each member's own arm.</summary>
    private IdentityTable<EyeDoorPartPhase, Action<RoomObjectPlace, RoomObjectStepContext>> SequenceEndedForEyeDoorPartPhase => field ??= new(new Dictionary<EyeDoorPartPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [EyeDoorPartPhase.ClosingBehindSamus] = (place, context) => context.RoomObjects.DeleteObject(place),
        [EyeDoorPartPhase.Placed] = (place, context) =>
        {
            if (context.RoomObjects.DoorObjects.IsDoorOpened(Door))
            {
                context.RoomObjects.DeleteObject(place);
                return;
            }
            Look(place, context);
        },
        [EyeDoorPartPhase.Pulsing] = (place, context) =>
        {
            if (IsSamusNear(place, context))
            {
                Play(EyeDoorSequenceDefinitions.Pulse[Orientation], place, context);
            }
            else
            {
                Look(place, context);
            }
        },
        [EyeDoorPartPhase.Still] = (place, context) => Look(place, context),
    });

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        SequenceEndedForEyeDoorPartPhase[Phase](place, context);
    }

    /// <summary>
    /// Looks for Samus: near, the part waits for its door to open and pulses; otherwise it stands still.
    /// </summary>
    private void Look(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (IsSamusNear(place, context))
        {
            WaitsForOpenedDoor = true;
            Phase = EyeDoorPartPhase.Pulsing;
            Play(EyeDoorSequenceDefinitions.Pulse[Orientation], place, context);
            return;
        }
        Phase = EyeDoorPartPhase.Still;
        Play(EyeDoorSequenceDefinitions.Still, place, context);
    }

    /// <summary>Whether Samus stands near enough to make the part pulse.</summary>
    private static bool IsSamusNear(RoomObjectPlace place, RoomObjectStepContext context) =>
        context.RoomObjects.EyeDoors.IsSamusNear(place, EyeDoorSequenceDefinitions.PulseReach);
}
