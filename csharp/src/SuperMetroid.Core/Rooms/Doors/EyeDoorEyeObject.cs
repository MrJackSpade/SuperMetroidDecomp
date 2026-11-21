using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's eye (#1554). On its first update
/// the eye looks at its door: one opened for good moves up a block and turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>). Otherwise it stays closed, looking for Samus each time its closed image ends; once she is near
/// (<see cref="EyeDoorSequenceDefinitions.WakeReach"/>) it opens, takes missiles from then on, and watches her, firing its
/// shots, blinking while she stands right in front of it and closing again once she has left. A pending shot is
/// consumed before the eye's images count down: a missile counts a hit (a Super Missile reaches the threshold at once),
/// and any other shot makes the dud sound. A hit short of the threshold sounds, smokes and makes the eye flinch; the hit
/// that reaches it records the door opened for good, stops the shots, smokes, moves the eye up a block onto its blue cap,
/// flashes it (<see cref="EyeDoorSequenceDefinitions.OpeningFlashes"/>) and turns it into the cap.
/// </summary>
/// <param name="orientation">The eye door's facing.</param>
/// <param name="door">The door the eye's placement named, which its attacks keep to burst once it opens.</param>
internal sealed partial class EyeDoorEyeObject(EyeDoorOrientation orientation, PlacedDoor door) : SequencedRoomObject<EyeDoorSegment, EyeDoorObjectFrame, EyeDoorAction>(EyeDoorSequenceDefinitions.Segments), IShootableRoomObject
{
    /// <summary>The eye door's facing.</summary>
    internal EyeDoorOrientation Orientation { get; } = orientation;

    /// <summary>The door the eye stands in, as its placement named it.</summary>
    internal PlacedDoor Door { get; } = Ensure.NotNull(door);

    /// <summary>What the eye is doing.</summary>
    internal EyeDoorEyePhase Phase { get; private set; } = EyeDoorEyePhase.Placed;

    /// <summary>
    /// The hits counted toward the threshold. It only counts up, by one a hit or straight to the threshold on a Super
    /// Missile, and the eye is destroyed once it reaches the threshold.
    /// </summary>
    internal int HitCounter { get; set; }

    /// <summary>Whether a shot at the eye's block reaches it: from its opening until the hit that destroys it.</summary>
    internal bool TakesShots { get; set; }

    /// <summary>
    /// The shot at the eye's block it has yet to take (#2484, #2562). A shot that arrives before the eye takes shots waits for
    /// it; a later shot before the eye takes it replaces it.
    /// </summary>
    private readonly StrikeAwaitingRoomObject<ResidentObjectProjectileHit> _strike = new();

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => _strike.Land(hit);

    /// <summary>The blue cap the eye turns into: the one facing its own side.</summary>
    private ColoredDoorOrientation CapFacing => EyeDoorDraws.BlueCapFacing(Orientation);

    /// <summary>Reacts to what the object waits for, then, unless the reaction took this update, counts its steps down.</summary>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!React(place, context))
            base.Update(place, context);
    }

    /// <summary>
    /// What a pending shot does to the eye while it takes shots: a missile or Super Missile hits it, a Super Missile
    /// destroying it at once, and any other shot is rejected with its sound. Returns whether the eye was hit.
    /// </summary>
    private bool React(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (!TakesShots || _strike.Take() is not { Family: var family })
            return false;
        if (!EyeDoorShotDefinitions.Hits.TryGetValue(family, out EyeDoorHit hit))
        {
            context.RoomObjects.EyeDoors.QueueRejectedShotSound();
            return false;
        }
        Hit(place, context, hit);
        return true;
    }

    /// <inheritdoc/>
    private protected override void Show(EyeDoorObjectFrame frame, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.Draw(place, EyeDoorDraws.Of(Orientation, frame));

    /// <inheritdoc/>
    private protected override void Raise(EyeDoorAction raised, RoomObjectPlace place, RoomObjectStepContext context)
    {
        EyeDoorObjects eyes = context.RoomObjects.EyeDoors;
        switch (raised)
        {
            case EyeDoorAction.Shoot:
                eyes.Shoot(place, Orientation, Door.Placed.Door);
                return;
            case EyeDoorAction.SpawnTwoSmoke:
                eyes.SpawnTwoSmoke(place, Orientation);
                return;
            case EyeDoorAction.SpawnSweat:
                eyes.SpawnSweat(place, Orientation);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorAction)} {raised}.");
        }
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case EyeDoorEyePhase.Placed:
                Look(place, context);
                return;
            case EyeDoorEyePhase.Closed:
                if (IsSamusNear(place, context, EyeDoorSequenceDefinitions.WakeReach))
                    Open(place, context);
                else
                    Play(EyeDoorSequenceDefinitions.Closed, place, context);
                return;
            case EyeDoorEyePhase.Watching:
                if (IsSamusNear(place, context, EyeDoorSequenceDefinitions.WakeReach))
                {
                    Watch(place, context);
                    return;
                }
                Phase = EyeDoorEyePhase.Closed;
                Play(EyeDoorSequenceDefinitions.Closed, place, context);
                return;
            case EyeDoorEyePhase.Opening:
            case EyeDoorEyePhase.Blinking:
            case EyeDoorEyePhase.Flinching:
                Watch(place, context);
                return;
            case EyeDoorEyePhase.Destroyed:
                BlueDoorCapObject.TakeOver(place, context, CapFacing, Door);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorEyePhase)} {Phase}.");
        }
    }

    /// <summary>
    /// The eye's first look at its door: one opened for good moves up onto its blue cap and turns into it; otherwise the
    /// eye stays closed.
    /// </summary>
    private void Look(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (context.RoomObjects.DoorObjects.IsDoorOpened(Door))
        {
            EyeDoorObjects.ConvertEyeToBlueDoor(context.Level, place, Orientation);
            BlueDoorCapObject.TakeOver(place, context, CapFacing, Door);
            return;
        }
        Phase = EyeDoorEyePhase.Closed;
        Play(EyeDoorSequenceDefinitions.Closed, place, context);
    }

    /// <summary>Samus came near: the eye takes missiles from now on and opens.</summary>
    private void Open(RoomObjectPlace place, RoomObjectStepContext context)
    {
        TakesShots = true;
        Phase = EyeDoorEyePhase.Opening;
        Play(EyeDoorSequenceDefinitions.Opening, place, context);
    }

    /// <summary>The open eye blinks while Samus stands right in front of it, and otherwise watches and shoots.</summary>
    private void Watch(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (IsSamusNear(place, context, EyeDoorSequenceDefinitions.InFrontReach))
        {
            Phase = EyeDoorEyePhase.Blinking;
            Play(EyeDoorSequenceDefinitions.Blinking, place, context);
            return;
        }
        Phase = EyeDoorEyePhase.Watching;
        Play(EyeDoorSequenceDefinitions.Watching, place, context);
    }

    /// <summary>
    /// A missile hit: it sounds and smokes, then counts; short of the threshold the eye flinches, and at it the eye is
    /// destroyed.
    /// </summary>
    private void Hit(RoomObjectPlace place, RoomObjectStepContext context, EyeDoorHit hit)
    {
        EyeDoorObjects eyes = context.RoomObjects.EyeDoors;
        context.QueueSound(EyeDoorSequenceDefinitions.HitSound, EyeDoorSequenceDefinitions.HitSoundPriority);
        eyes.SpawnTwoSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        HitCounter = hit switch
        {
            EyeDoorHit.DestroysAtOnce => EyeDoorSequenceDefinitions.HitThreshold,
            EyeDoorHit.Counts => HitCounter + 1,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorHit)} {hit}."),
        };
        if (HitCounter < EyeDoorSequenceDefinitions.HitThreshold)
        {
            Phase = EyeDoorEyePhase.Flinching;
            Play(EyeDoorSequenceDefinitions.Flinching, place, context);
            return;
        }
        context.RoomObjects.DoorObjects.RecordDoorOpened(Door);
        TakesShots = false;
        eyes.SpawnSmoke(place, Orientation);
        eyes.SpawnSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        EyeDoorObjects.ConvertEyeToBlueDoor(context.Level, place, Orientation);
        Phase = EyeDoorEyePhase.Destroyed;
        Play(EyeDoorSequenceDefinitions.Destroyed, place, context);
    }

    /// <summary>Whether Samus stands within <paramref name="reach"/> of the eye.</summary>
    private static bool IsSamusNear(RoomObjectPlace place, RoomObjectStepContext context, Extent16<RoomBlocks> reach) =>
        context.RoomObjects.EyeDoors.IsSamusNear(place, reach);
}
