using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A yellow, green or red door cap (#1554), which replaces the coloured caps' sequences, their shot wake and their hit
/// counter. On its first update the cap looks at its door: one opened for good turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>); otherwise it starts taking shots, shows its closed image and rests. While it takes
/// shots, a shot pending at its block is consumed before its images count down: the colour's projectile counts a hit
/// (a Super Missile first leaves a red cap one hit short of any count), and any other shot makes the dud sound. A hit
/// short of the colour's count flashes the cap, which then rests again; the hit that reaches it records the door
/// opened for good, stops the shots and opens the cap, which then leaves the room. A cap closing behind Samus plays the
/// shared closing, then looks at its door again.
/// </summary>
internal sealed class ColoredDoorObject : DoorCapObject, IClosingDoor
{
    /// <summary>A cap of <paramref name="variant"/>'s colour and facing, just placed.</summary>
    internal ColoredDoorObject(ColoredDoorVariant variant)
        : base(variant.Orientation, DoorCaps.PaintOf(variant.Color))
    {
        Color = variant.Color;
        _sequences = ColoredDoorDefinitions.Sequences[variant];
    }

    /// <summary>The cap's colour, which selects the projectile that opens it.</summary>
    internal ColoredDoorColor Color { get; }

    /// <summary>What the cap is doing.</summary>
    internal ColoredDoorPhase Phase { get; private set; } = ColoredDoorPhase.Waiting;

    /// <summary>
    /// The hits counted toward the colour's count. It only counts up, by one a hit or to one short of any count on a
    /// red cap's Super Missile, and the cap opens once it reaches the count.
    /// </summary>
    internal int HitCounter { get; private set; }

    /// <summary>Whether a shot at the cap's block reaches it: from its first look until the hit that opens it.</summary>
    internal bool TakesShots { get; private set; }

    /// <summary>The colour and facing's sequences.</summary>
    private readonly ColoredDoorSequences _sequences;

    /// <summary>Whether the cap has yet to take its first look at its door.</summary>
    private bool _placed = true;

    /// <inheritdoc/>
    public void CloseBehindSamus()
    {
        Phase = ColoredDoorPhase.Closing;
        PlayFromNextUpdate(DoorCapSequenceDefinitions.CapClosing);
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
        if (!TakesShots || place.ProjectileHit is not { Family: var family })
            return false;
        place.ProjectileHit = null;
        if (Color == ColoredDoorColor.Red && family == SamusProjectileFamily.SuperMissile)
            HitCounter = ColoredDoorDefinitions.RedSuperMissileHitCounter;
        if (!Accepts(family))
        {
            context.RoomObjects.DoorObjects.QueueDudSound();
            return false;
        }
        CountHit(place, context);
        return true;
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case ColoredDoorPhase.Closing:
                Look(place, context);
                return;
            case ColoredDoorPhase.Waiting:
                if (_placed)
                    Look(place, context);
                else
                    Rest();
                return;
            case ColoredDoorPhase.Flashing:
                Phase = ColoredDoorPhase.Waiting;
                Rest();
                return;
            case ColoredDoorPhase.Opening:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ColoredDoorPhase)} {Phase}.");
        }
    }

    /// <summary>Whether a shot of <paramref name="family"/> counts as a hit on a cap of this colour.</summary>
    private bool Accepts(SamusProjectileFamily family) => Color switch
    {
        ColoredDoorColor.Yellow => family == SamusProjectileFamily.PowerBomb,
        ColoredDoorColor.Green => family == SamusProjectileFamily.SuperMissile,
        ColoredDoorColor.Red => family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {Color}."),
    };

    /// <summary>
    /// Looks at the cap's door: one opened for good turns into its blue cap; otherwise the cap starts taking shots and
    /// shows its closed image.
    /// </summary>
    private void Look(RoomObjectSlot place, RoomObjectStepContext context)
    {
        _placed = false;
        if (context.RoomObjects.DoorObjects.IsDoorOpened(place))
        {
            BlueDoorCapObject.TakeOver(place, context, Orientation);
            return;
        }
        TakesShots = true;
        Phase = ColoredDoorPhase.Waiting;
        Play(_sequences.Waiting, place, context);
    }

    /// <summary>Counts a hit: short of the colour's count the cap flashes; at it, the door opens for good.</summary>
    private void CountHit(RoomObjectSlot place, RoomObjectStepContext context)
    {
        HitCounter++;
        if (HitCounter < ColoredDoorDefinitions.HitThresholds[Color])
        {
            Phase = ColoredDoorPhase.Flashing;
            Play(_sequences.Flash, place, context);
            return;
        }
        context.RoomObjects.DoorObjects.RecordDoorOpened(place);
        TakesShots = false;
        Phase = ColoredDoorPhase.Opening;
        Play(_sequences.Opening, place, context);
    }
}
