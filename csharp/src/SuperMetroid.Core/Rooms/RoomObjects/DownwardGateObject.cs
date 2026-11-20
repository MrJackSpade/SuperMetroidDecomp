using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A downward gate (#1554). It shows the closed gate on its first update; after showing the closed or open gate for one
/// update it clears its trigger and rests. Resting closed, it wakes on the first update, before its timing runs, on which
/// it is triggered or Samus is inside its column, and rises; resting open, it wakes once triggered and lowers. The
/// movement's first image shows on the waking update, and each image starts the movement it names
/// (<see cref="DownwardGateImage.Starts"/>). A trigger a shot sets while the gate moves stays until the gate next rests.
/// </summary>
/// <param name="owner">The room's gates, which reach Samus and the gate projectiles.</param>
[RoomLifetime]
internal sealed class DownwardGateObject(GateObjects owner) : SequencedRoomObject<DownwardGateSegment, DownwardGateImage, NoAnimationCue>(DownwardGateAnimations.Segments)
{
    /// <summary>The room's gates.</summary>
    private readonly GateObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the gate is in its course.</summary>
    private DownwardGatePhase Phase { get; set; } = DownwardGatePhase.Starting;

    /// <summary>
    /// Whether an accepted shot on one of its triggers has set the gate off since it last rested. A shot can come at any
    /// point of the gate's course and is kept until the gate next rests, so it is a fact of its own beside the phase.
    /// </summary>
    private bool Triggered { get; set; }

    /// <summary>An accepted shot on one of the gate's triggers sets it off; further shots before it rests change nothing.</summary>
    internal void Trigger() => Triggered = true;

    /// <inheritdoc/>
    /// <remarks>Only a resting gate wakes: for a trigger, or, closed, for Samus in its column.</remarks>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (Phase is DownwardGatePhase.RestingClosed or DownwardGatePhase.RestingOpen &&
            (Triggered || (Phase == DownwardGatePhase.RestingClosed && _owner.IsSamusInsideDownwardGateColumn(place))))
        {
            bool open = Phase == DownwardGatePhase.RestingOpen;
            Phase = open ? DownwardGatePhase.Closing : DownwardGatePhase.Reopening;
            // The movement's first image shows as the sequence runs below, on this update.
            PlayFromNextUpdate(open ? DownwardGateAnimations.Closing : DownwardGateAnimations.Reopening);
        }
        base.Update(place, context);
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case DownwardGatePhase.Starting:
            case DownwardGatePhase.Closing:
                Phase = DownwardGatePhase.ShowingClosed;
                Play(DownwardGateAnimations.Closed, place, context);
                return;
            case DownwardGatePhase.Reopening:
                Phase = DownwardGatePhase.ShowingOpen;
                Play(DownwardGateAnimations.Open, place, context);
                return;
            case DownwardGatePhase.ShowingClosed:
                RestIn(DownwardGatePhase.RestingClosed);
                return;
            case DownwardGatePhase.ShowingOpen:
                RestIn(DownwardGatePhase.RestingOpen);
                return;
            case DownwardGatePhase.RestingClosed:
            case DownwardGatePhase.RestingOpen:
            default:
                throw new InvalidOperationException($"The downward gate's sequence ended in {nameof(DownwardGatePhase)} {Phase}.");
        }
    }

    /// <summary>Starts the movement <paramref name="frame"/> names, then draws it.</summary>
    /// <exception cref="InvalidOperationException">The image names an undefined movement.</exception>
    private protected override void Show(DownwardGateImage frame, RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (frame.Starts)
        {
            case DownwardGateMovement.None:
                break;
            case DownwardGateMovement.Lowering:
                _owner.SpawnMovingGateProjectile(place);
                context.QueueSound(DownwardGateDefinitions.MovementSound, DownwardGateMovementDefinitions.MovementSoundPriority);
                break;
            case DownwardGateMovement.Rising:
                _owner.WakeGateProjectile(place);
                context.QueueSound(DownwardGateDefinitions.MovementSound, DownwardGateMovementDefinitions.MovementSoundPriority);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGateMovement)} {frame.Starts}.");
        }
        context.Draw(place, frame.Draw);
    }

    /// <summary>
    /// Clears the gate's trigger and lets it rest in <paramref name="phase"/> on the image it shows, waking from the
    /// next update on.
    /// </summary>
    private void RestIn(DownwardGatePhase phase)
    {
        Triggered = false;
        Phase = phase;
    }
}
