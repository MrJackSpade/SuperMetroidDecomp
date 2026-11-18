using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A grey door cap (#1554).
/// On its first update the cap looks at its door: one opened for good turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>); otherwise it locks, shows its closed image and rests. While locked it tests its
/// condition on each update before its images count down: until the condition holds a shot pending at its block makes
/// the dud sound; once it holds (the room's enemy quota also wakes Zebes) the pending shot is consumed and the cap
/// flashes, again and again, until a shot opens it. The shot that opens it records the door opened for good, and the
/// cap leaves the room once open. A cap closing behind Samus plays the shared closing (Bomb Torizo's door stays open until
/// she has Bombs), then looks at its door again. The cap lives in its room's object places, with the lock its setup resolved
/// for that room (#2486).
/// </summary>
[RoomLifetime]
internal sealed class GreyDoorObject : DoorCapObject, IShootableRoomObject
{
    /// <summary>A grey cap of <paramref name="variant"/>, locked by <paramref name="doorLock"/>, just placed in <paramref name="door"/>.</summary>
    internal GreyDoorObject(GreyDoorVariant variant, GreyDoorLock doorLock, PlacedDoor door)
        : base(variant.Orientation, DoorCapPaint.Grey)
    {
        Variant = variant;
        Lock = Ensure.NotNull(doorLock);
        Door = Ensure.NotNull(door);
    }

    /// <summary>The door the cap stands in, as its placement named it.</summary>
    internal PlacedDoor Door { get; }

    /// <summary>A cap whose door has been opened for good stays open behind Samus.</summary>
    public override bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        Door.IsOpenedForGood(openedForGood);

    /// <summary>Which grey cap this is.</summary>
    internal GreyDoorVariant Variant { get; }

    /// <summary>What holds the cap locked: the condition it was placed with and what that condition reads (#2486).</summary>
    internal GreyDoorLock Lock { get; }

    /// <summary>What the cap is doing.</summary>
    internal GreyDoorPhase Phase { get; private set; } = GreyDoorPhase.Locked;

    /// <summary>
    /// What the cap waits for on each update: nothing until its first look locks it (a locked cap waiting for nothing has
    /// yet to take that look), and nothing once it opens.
    /// </summary>
    internal GreyDoorWait Waits { get; private set; }

    /// <summary>
    /// The shot at the cap's block it has yet to take, or null for none (#2484). A shot that arrives before the cap waits for anything waits for
    /// it; a later shot before the cap takes it replaces it.
    /// </summary>
    internal ResidentObjectProjectileHit? PendingHit { get; private set; }

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => PendingHit = hit;

    /// <inheritdoc/>
    /// <remarks>Bomb Torizo's door is held open until Samus has Bombs; every other cap closes at once.</remarks>
    public override void CloseBehindSamus()
    {
        if (Variant.Closing == GreyDoorClosing.AfterBombs)
        {
            Phase = GreyDoorPhase.HeldOpen;
            PlayFromNextUpdate(GreyDoorDefinitions.BombTorizoHeldOpen);
            return;
        }
        Phase = GreyDoorPhase.Closing;
        PlayFromNextUpdate(DoorCapSequenceDefinitions.CapClosing);
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
        GreyDoorObjects doors = context.RoomObjects.GreyDoors;
        switch (Waits)
        {
            case GreyDoorWait.Nothing:
                return false;
            case GreyDoorWait.Condition:
                if (!Lock.IsUnlocked(context.Frame))
                {
                    if (PendingHit is not null)
                        doors.QueueDudSound();
                    PendingHit = null;
                    return false;
                }
                Lock.RecordUnlocked();
                PendingHit = null;
                Waits = GreyDoorWait.Shot;
                Phase = GreyDoorPhase.Flashing;
                Play(GreyDoorDefinitions.Flash, place, context);
                return true;
            case GreyDoorWait.Shot:
                if (PendingHit is null)
                    return false;
                PendingHit = null;
                Open(place, context);
                return true;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorWait)} {Waits}.");
        }
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case GreyDoorPhase.Closing:
                Look(place, context);
                return;
            case GreyDoorPhase.HeldOpen:
                // Bomb Torizo's door stays open, two updates at a time, until Samus has Bombs.
                if (!context.Frame.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
                {
                    Play(GreyDoorDefinitions.BombTorizoHeldOpen, place, context);
                    return;
                }
                Phase = GreyDoorPhase.Closing;
                Play(GreyDoorDefinitions.BombTorizoClosing, place, context);
                return;
            case GreyDoorPhase.Locked:
                // Once it has looked, the cap rests on its closed image until its condition holds.
                if (Waits == GreyDoorWait.Nothing)
                    Look(place, context);
                return;
            case GreyDoorPhase.Flashing:
                throw new InvalidOperationException("An unlocked grey cap flashes until a shot opens it; its flash never ends.");
            case GreyDoorPhase.Opening:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorPhase)} {Phase}.");
        }
    }

    /// <summary>
    /// Looks at the cap's door: one opened for good turns into its blue cap; otherwise the cap locks and shows its closed
    /// image.
    /// </summary>
    private void Look(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (context.RoomObjects.DoorObjects.IsDoorOpened(Door))
        {
            BlueDoorCapObject.TakeOver(place, context, Orientation, Door);
            return;
        }
        Waits = GreyDoorWait.Condition;
        Phase = GreyDoorPhase.Locked;
        Play(GreyDoorDefinitions.Locked, place, context);
    }

    /// <summary>Opens the cap at the opening shot, which records the door opened for good.</summary>
    private void Open(RoomObjectPlace place, RoomObjectStepContext context)
    {
        context.RoomObjects.DoorObjects.RecordDoorOpened(Door);
        Waits = GreyDoorWait.Nothing;
        Phase = GreyDoorPhase.Opening;
        Play(GreyDoorDefinitions.Opening, place, context);
    }
}
