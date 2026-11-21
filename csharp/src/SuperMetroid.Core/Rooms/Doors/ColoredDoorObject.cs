using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A yellow, green or red door cap (#1554).
/// On its first update the cap looks at its door: one opened for good turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>); otherwise it starts taking shots, shows its closed image and rests. While it takes
/// shots, a shot pending at its block is consumed before its images count down: the colour's projectile counts a hit
/// (a Super Missile reaches the count at once), and any other shot makes the dud sound. A hit
/// short of the colour's count flashes the cap, which then rests again; the hit that reaches it records the door
/// opened for good, stops the shots and opens the cap, which then leaves the room. A cap closing behind Samus plays the
/// shared closing, then looks at its door again.
/// </summary>
internal sealed partial class ColoredDoorObject : DoorCapObject, IShootableRoomObject
{
    /// <summary>A cap of <paramref name="variant"/>'s colour and facing, just placed in <paramref name="door"/>.</summary>
    internal ColoredDoorObject(ColoredDoorVariant variant, PlacedDoor door)
        : base(variant.Orientation, ColoredDoorDefinitions.Colors[variant.Color].Paint)
    {
        Door = Ensure.NotNull(door);
        Rules = ColoredDoorDefinitions.Colors[variant.Color];
        _sequences = ColoredDoorDefinitions.Sequences[variant];
    }

    /// <summary>The door the cap stands in, as its placement named it.</summary>
    internal PlacedDoor Door { get; }

    /// <summary>A cap whose door has been opened for good stays open behind Samus.</summary>
    public override bool StaysOpenBehindSamus(Func<LockedDoor, bool> openedForGood) =>
        Door.IsOpenedForGood(openedForGood);


    /// <summary>What the cap is doing.</summary>
    internal ColoredDoorPhase Phase { get; set; } = ColoredDoorPhase.Placed;

    /// <summary>
    /// The hits counted toward the colour's count. It only counts up, by one a hit or straight to the count on a Super
    /// Missile, and the cap opens once it reaches the count.
    /// </summary>
    internal int HitCounter { get; set; }

    /// <summary>
    /// Whether a shot at the cap's block reaches it: from its first look until the hit that opens it. This is what the cap
    /// waits for, apart from what it plays: a closing behind Samus keeps it, so a cap that has looked still takes shots while
    /// it closes, and one just placed takes none.
    /// </summary>
    internal bool TakesShots { get; set; }

    /// <summary>The colour's rules: the shots it counts and the hits it takes.</summary>
    internal ColoredDoorColorDefinition Rules { get; }

    /// <summary>The colour and facing's sequences.</summary>
    private readonly ColoredDoorSequences _sequences;

    /// <summary>
    /// The shot at the cap's block it has yet to take (#2484, #2562). A shot that arrives before the cap takes shots waits for
    /// it; a later shot before the cap takes it replaces it.
    /// </summary>
    private readonly StrikeAwaitingRoomObject<ResidentObjectProjectileHit> _strike = new();

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => _strike.Land(hit);

    /// <inheritdoc/>
    public override void CloseBehindSamus()
    {
        Phase = ColoredDoorPhase.Closing;
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
        if (!TakesShots || _strike.Take() is not { Family: var family })
            return false;
        if (!Rules.Counts(family))
        {
            context.RoomObjects.DoorObjects.QueueDudSound();
            return false;
        }
        CountHit(place, context, opensAtOnce: family == SamusProjectileFamily.SuperMissile);
        return true;
    }

    /// <summary>What <see cref="SequenceEnded"/> does for each <see cref="ColoredDoorPhase"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<ColoredDoorPhase, Action<RoomObjectPlace, RoomObjectStepContext>> SequenceEndedForColoredDoorPhase => field ??= new(new Dictionary<ColoredDoorPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [ColoredDoorPhase.Placed] = (place, context) => Look(place, context),
        [ColoredDoorPhase.Closing] = (place, context) => Look(place, context),
        // Once it has looked, the cap rests on its closed image until a hit wakes it.
        [ColoredDoorPhase.Waiting] = (place, context) => { },
        [ColoredDoorPhase.Flashing] = (place, context) =>
        {
            // The cap rests on its closed image again until the next hit wakes it.
            Phase = ColoredDoorPhase.Waiting;
        },
        [ColoredDoorPhase.Opening] = (place, context) => context.RoomObjects.DeleteObject(place),
    });

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        SequenceEndedForColoredDoorPhase[Phase](place, context);
    }

    /// <summary>
    /// Looks at the cap's door: one opened for good turns into its blue cap; otherwise the cap starts taking shots and
    /// shows its closed image.
    /// </summary>
    private void Look(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (context.RoomObjects.DoorObjects.IsDoorOpened(Door))
        {
            BlueDoorCapObject.TakeOver(place, context, Orientation, Door);
            return;
        }
        TakesShots = true;
        Phase = ColoredDoorPhase.Waiting;
        Play(_sequences.Waiting, place, context);
    }

    /// <summary>
    /// Counts a hit: short of the colour's count the cap flashes; at it, the door opens for good. A hit that
    /// <paramref name="opensAtOnce"/> brings the count straight to the colour's.
    /// </summary>
    private void CountHit(RoomObjectPlace place, RoomObjectStepContext context, bool opensAtOnce)
    {
        int threshold = Rules.HitsToOpen;
        HitCounter = opensAtOnce ? threshold : HitCounter + 1;
        if (HitCounter < threshold)
        {
            Phase = ColoredDoorPhase.Flashing;
            Play(_sequences.Flash, place, context);
            return;
        }
        context.RoomObjects.DoorObjects.RecordDoorOpened(Door);
        TakesShots = false;
        Phase = ColoredDoorPhase.Opening;
        Play(_sequences.Opening, place, context);
    }
}
