using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// An eye door's eye (#1554), which replaces the eye's sequences, its missile wake and its hit counter. On its first update
/// the eye looks at its door: one opened for good moves up a block and turns into its closed blue cap
/// (<see cref="BlueDoorCapObject"/>). Otherwise it stays closed, looking for Samus every four updates; once she is near
/// (<see cref="EyeDoorSequenceDefinitions.WakeReach"/>) it opens, takes missiles from then on, and watches her, firing its
/// shots, blinking while she stands right in front of it and closing again once she has left. A pending shot is
/// consumed before the eye's images count down: a missile counts a hit (a Super Missile first leaves it one hit short),
/// and any other shot makes the dud sound. A hit short of the threshold sounds, smokes and makes the eye flinch; the hit
/// that reaches it records the door opened for good, stops the shots, smokes, moves the eye up a block onto its blue cap,
/// flashes it ten times and turns it into the cap.
/// </summary>
/// <param name="orientation">The eye door's facing.</param>
internal sealed class EyeDoorEyeObject(EyeDoorOrientation orientation) : SequencedRoomObject<EyeDoorStep>
{
    /// <summary>The eye door's facing.</summary>
    internal EyeDoorOrientation Orientation { get; } = orientation;

    /// <summary>What the eye is doing.</summary>
    internal EyeDoorEyePhase Phase { get; private set; } = EyeDoorEyePhase.Closed;

    /// <summary>
    /// The hits counted toward the threshold. It only counts up, by one a hit or to one short of the threshold on a
    /// Super Missile, and the eye is destroyed once it reaches the threshold.
    /// </summary>
    internal int HitCounter { get; private set; }

    /// <summary>Whether a shot at the eye's block reaches it: from its opening until the hit that destroys it.</summary>
    internal bool TakesShots { get; private set; }

    /// <summary>Whether the eye has yet to take its first look at its door.</summary>
    private bool _placed = true;

    /// <summary>The flashes the destroyed eye has still to make before it turns into its blue cap.</summary>
    private int _flashesLeft;

    /// <summary>The blue cap the eye turns into: the one facing its own side.</summary>
    private ColoredDoorOrientation CapFacing => Orientation switch
    {
        EyeDoorOrientation.Left => ColoredDoorOrientation.Left,
        EyeDoorOrientation.Right => ColoredDoorOrientation.Right,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {Orientation}."),
    };

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
        if (family == SamusProjectileFamily.SuperMissile)
        {
            HitCounter = EyeDoorSequenceDefinitions.SuperMissileHitCounter;
        }
        else if (family != SamusProjectileFamily.Missile)
        {
            context.RoomObjects.EyeDoors.QueueRejectedShotSound();
            return false;
        }
        Hit(place, context);
        return true;
    }

    /// <inheritdoc/>
    private protected override int Show(EyeDoorStep step, RoomObjectSlot place, RoomObjectStepContext context)
    {
        EyeDoorObjects eyes = context.RoomObjects.EyeDoors;
        switch (step.Before)
        {
            case EyeDoorAction.None:
                break;
            case EyeDoorAction.Shoot:
                eyes.Shoot(place, Orientation);
                break;
            case EyeDoorAction.SpawnTwoSmoke:
                eyes.SpawnTwoSmoke(place, Orientation);
                break;
            case EyeDoorAction.SpawnSweat:
                eyes.SpawnSweat(place, Orientation);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorAction)} {step.Before}.");
        }
        context.Draw(place, EyeDoorDraws.DrawList(Orientation, EyeDoorComponent.Eye, step.Image));
        return step.Updates;
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case EyeDoorEyePhase.Closed:
                if (_placed)
                {
                    Look(place, context);
                    return;
                }
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
                _flashesLeft--;
                if (_flashesLeft != 0)
                    Play(EyeDoorSequenceDefinitions.OpeningFlash, place, context);
                else
                    BlueDoorCapObject.TakeOver(place, context, CapFacing);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EyeDoorEyePhase)} {Phase}.");
        }
    }

    /// <summary>
    /// The eye's first look at its door: one opened for good moves up onto its blue cap and turns into it; otherwise the
    /// eye stays closed.
    /// </summary>
    private void Look(RoomObjectSlot place, RoomObjectStepContext context)
    {
        _placed = false;
        if (context.RoomObjects.DoorObjects.IsDoorOpened(place))
        {
            EyeDoorObjects.ConvertEyeToBlueDoor(context.Level, place, Orientation);
            BlueDoorCapObject.TakeOver(place, context, CapFacing);
            return;
        }
        Play(EyeDoorSequenceDefinitions.Closed, place, context);
    }

    /// <summary>Samus came near: the eye takes missiles from now on and opens.</summary>
    private void Open(RoomObjectSlot place, RoomObjectStepContext context)
    {
        TakesShots = true;
        Phase = EyeDoorEyePhase.Opening;
        Play(EyeDoorSequenceDefinitions.Opening, place, context);
    }

    /// <summary>The open eye blinks while Samus stands right in front of it, and otherwise watches and shoots.</summary>
    private void Watch(RoomObjectSlot place, RoomObjectStepContext context)
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
    private void Hit(RoomObjectSlot place, RoomObjectStepContext context)
    {
        EyeDoorObjects eyes = context.RoomObjects.EyeDoors;
        context.QueueSound(EyeDoorSequenceDefinitions.HitSound, EyeDoorSequenceDefinitions.HitSoundPriority);
        eyes.SpawnTwoSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        HitCounter++;
        if (HitCounter < EyeDoorSequenceDefinitions.HitThreshold)
        {
            Phase = EyeDoorEyePhase.Flinching;
            Play(EyeDoorSequenceDefinitions.Flinching, place, context);
            return;
        }
        context.RoomObjects.DoorObjects.RecordDoorOpened(place);
        TakesShots = false;
        eyes.SpawnSmoke(place, Orientation);
        eyes.SpawnSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        eyes.SpawnTwoSmoke(place, Orientation);
        EyeDoorObjects.ConvertEyeToBlueDoor(context.Level, place, Orientation);
        Phase = EyeDoorEyePhase.Destroyed;
        _flashesLeft = EyeDoorSequenceDefinitions.OpeningFlashes;
        Play(EyeDoorSequenceDefinitions.OpeningFlash, place, context);
    }

    /// <summary>Whether Samus stands within <paramref name="reach"/> of the eye.</summary>
    private static bool IsSamusNear(RoomObjectSlot place, RoomObjectStepContext context, Extent16<RoomBlocks> reach) =>
        context.RoomObjects.EyeDoors.IsSamusNear(place, reach);
}
