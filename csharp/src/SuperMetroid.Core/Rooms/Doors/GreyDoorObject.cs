using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A grey door cap (#1554), which replaces the grey caps' sequences, their condition and shot wakes and their hit
/// counter. On its first update the cap looks at its door: one opened for good turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>); otherwise it locks, shows its closed image and rests. While locked it tests its
/// condition on each update before its images count down: until the condition holds a shot pending at its block makes
/// the dud sound; once it holds (the room's enemy quota also wakes Zebes) the pending shot is consumed and the cap
/// flashes, again and again, until a shot opens it. The shot that opens it records the door opened for good, and the
/// cap leaves the room once open. A cap closing behind Samus plays the shared closing (Bomb Torizo's door stays open until
/// she has Bombs), then looks at its door again.
/// </summary>
internal sealed class GreyDoorObject : DoorCapObject, IClosingDoor
{
    /// <summary>A grey cap of <paramref name="variant"/>, locked by <paramref name="condition"/>, just placed.</summary>
    internal GreyDoorObject(GreyDoorVariant variant, GreyDoorCondition condition)
        : base(variant.Orientation, DoorCapPaint.Grey)
    {
        Variant = variant;
        Condition = condition;
    }

    /// <summary>Which grey cap this is.</summary>
    internal GreyDoorVariant Variant { get; }

    /// <summary>The condition that unlocks the cap.</summary>
    internal GreyDoorCondition Condition { get; }

    /// <summary>What the cap is doing.</summary>
    internal GreyDoorPhase Phase { get; private set; } = GreyDoorPhase.Locked;

    /// <summary>What the cap waits for on each update.</summary>
    internal GreyDoorWait Waits { get; private set; }

    /// <summary>The hits counted toward the grey count; the shot that reaches it opens the cap.</summary>
    internal int HitCounter { get; private set; }


    /// <summary>Whether the cap has yet to take its first look at its door.</summary>
    private bool _placed = true;

    /// <summary>Whether Bomb Torizo's door, closing, has seen Samus with Bombs and is closing for good.</summary>
    private bool _closingForGood;

    /// <inheritdoc/>
    public void CloseBehindSamus()
    {
        Phase = GreyDoorPhase.Closing;
        _closingForGood = !Variant.BombTorizo;
        PlayFromNextUpdate(Variant.BombTorizo ? GreyDoorDefinitions.BombTorizoHeldOpen : DoorCapSequenceDefinitions.CapClosing);
    }

    /// <summary>Reacts to what the object waits for, then, unless the reaction took this update, counts its steps down.</summary>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!React(place, context))
            base.Update(place, context);
    }

    /// <inheritdoc/>
    private bool React(RoomObjectSlot place, RoomObjectStepContext context)
    {
        GreyDoorObjects doors = context.RoomObjects.GreyDoors;
        switch (Waits)
        {
            case GreyDoorWait.Nothing:
                return false;
            case GreyDoorWait.Condition:
                if (!doors.IsUnlocked(Condition, context.Frame))
                {
                    if (place.ProjectileHit is not null)
                        doors.QueueDudSound();
                    place.ProjectileHit = null;
                    return false;
                }
                doors.RecordUnlocked(Condition);
                place.ProjectileHit = null;
                Waits = GreyDoorWait.Shot;
                Phase = GreyDoorPhase.Flashing;
                Play(GreyDoorDefinitions.Flash, place, context);
                return true;
            case GreyDoorWait.Shot:
                if (place.ProjectileHit is null)
                    return false;
                place.ProjectileHit = null;
                Open(place, context);
                return true;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorWait)} {Waits}.");
        }
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case GreyDoorPhase.Closing:
                if (_closingForGood)
                {
                    Look(place, context);
                    return;
                }
                // Bomb Torizo's door stays open, two updates at a time, until Samus has Bombs.
                _closingForGood = context.Frame.CollectedItems.HasAny(SamusEquipmentFlags.Bombs);
                Play(_closingForGood ? GreyDoorDefinitions.BombTorizoClosing : GreyDoorDefinitions.BombTorizoHeldOpen, place, context);
                return;
            case GreyDoorPhase.Locked:
                if (_placed)
                    Look(place, context);
                else
                    Rest();
                return;
            case GreyDoorPhase.Flashing:
                Play(GreyDoorDefinitions.Flash, place, context);
                return;
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
    private void Look(RoomObjectSlot place, RoomObjectStepContext context)
    {
        _placed = false;
        if (context.RoomObjects.DoorObjects.IsDoorOpened(place))
        {
            BlueDoorCapObject.TakeOver(place, context, Orientation);
            return;
        }
        Waits = GreyDoorWait.Condition;
        Phase = GreyDoorPhase.Locked;
        Play(GreyDoorDefinitions.Locked, place, context);
    }

    /// <summary>Counts the opening shot, which records the door opened for good, and opens the cap.</summary>
    private void Open(RoomObjectSlot place, RoomObjectStepContext context)
    {
        HitCounter++;
        if (HitCounter >= GreyDoorDefinitions.HitThreshold)
        {
            context.RoomObjects.DoorObjects.RecordDoorOpened(place);
            Waits = GreyDoorWait.Nothing;
        }
        Phase = GreyDoorPhase.Opening;
        Play(GreyDoorDefinitions.Opening, place, context);
    }
}
