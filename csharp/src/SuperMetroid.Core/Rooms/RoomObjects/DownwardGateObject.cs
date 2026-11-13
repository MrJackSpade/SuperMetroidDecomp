namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A downward gate (#1554). It shows the closed gate on its first update; after showing the closed or open gate for one
/// update it clears its trigger and rests. Resting closed, it wakes on the first update, before its timing runs, on which
/// it is triggered or Samus is inside its column, and rises; resting open, it wakes once triggered and lowers. A trigger
/// a shot sets while the gate moves stays until the gate next rests.
/// </summary>
/// <param name="owner">The room's gates, which reach Samus and the gate projectiles.</param>
[RoomLifetime]
internal sealed class DownwardGateObject(GateObjects owner) : RoomObjectBehaviour
{
    /// <summary>The room's gates.</summary>
    private readonly GateObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the gate is in its course.</summary>
    internal DownwardGatePhase Phase { get; private set; } = DownwardGatePhase.Starting;

    /// <summary>The image of its closing or reopening the gate shows; none until its movement shows its first image (#1627).</summary>
    internal int? Frame { get; private set; }

    /// <summary>Whether an accepted shot on one of its triggers has set the gate off since it last rested.</summary>
    internal bool Triggered { get; private set; }

    /// <summary>Whether the gate wakes for a trigger (or, closed, for Samus in its column): from when it rests until it wakes.</summary>
    internal bool WakeArmed { get; private set; }

    /// <summary>The updates left before the gate's next step.</summary>
    private int _updatesBeforeNextStep = 1;

    /// <summary>An accepted shot on one of the gate's triggers sets it off; further shots before it rests change nothing.</summary>
    internal void Trigger() => Triggered = true;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && (Triggered || (Phase == DownwardGatePhase.RestingClosed && _owner.IsSamusInsideDownwardGateColumn(place))))
        {
            WakeArmed = false;
            Phase = Phase == DownwardGatePhase.RestingOpen ? DownwardGatePhase.Closing : DownwardGatePhase.Reopening;
            Frame = null;
            _updatesBeforeNextStep = 1;
        }
        if (--_updatesBeforeNextStep != 0)
            return;
        switch (Phase)
        {
            case DownwardGatePhase.Starting:
                Show(place, context, DownwardGateMovementDefinitions.ClosedImage, 1, DownwardGatePhase.ShowingClosed);
                return;
            case DownwardGatePhase.ShowingClosed:
                Rest(DownwardGatePhase.RestingClosed);
                return;
            case DownwardGatePhase.ShowingOpen:
                Rest(DownwardGatePhase.RestingOpen);
                return;
            case DownwardGatePhase.RestingClosed:
            case DownwardGatePhase.RestingOpen:
                _updatesBeforeNextStep = 1;
                return;
            case DownwardGatePhase.Closing:
                int closingImage = NextImage();
                Frame = closingImage;
                if (closingImage == DownwardGateMovementDefinitions.Closing.Count)
                {
                    Show(place, context, DownwardGateMovementDefinitions.ClosedImage, 1, DownwardGatePhase.ShowingClosed);
                    return;
                }
                if (closingImage == DownwardGateMovementDefinitions.ClosingMovementStart)
                {
                    _owner.SpawnMovingGateProjectile(place);
                    context.QueueSound(DownwardGateDefinitions.MovementSound, DownwardGateMovementDefinitions.MovementSoundPriority);
                }
                ShowFrame(place, context, DownwardGateMovementDefinitions.Closing[closingImage]);
                return;
            case DownwardGatePhase.Reopening:
                int reopeningImage = NextImage();
                Frame = reopeningImage;
                if (reopeningImage == DownwardGateMovementDefinitions.Reopening.Count)
                {
                    Show(place, context, DownwardGateMovementDefinitions.OpenImage, 1, DownwardGatePhase.ShowingOpen);
                    return;
                }
                if (reopeningImage == 0)
                {
                    _owner.WakeGateProjectile(place);
                    context.QueueSound(DownwardGateDefinitions.MovementSound, DownwardGateMovementDefinitions.MovementSoundPriority);
                }
                ShowFrame(place, context, DownwardGateMovementDefinitions.Reopening[reopeningImage]);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGatePhase)} {Phase}.");
        }
    }

    /// <summary>The image after the one the gate shows: its movement's first when it has shown none.</summary>
    private int NextImage() => Frame is int shown ? shown + 1 : 0;

    /// <summary>Clears the gate's trigger and lets it rest in <paramref name="phase"/>, its wake armed from the next update.</summary>
    private void Rest(DownwardGatePhase phase)
    {
        Triggered = false;
        WakeArmed = true;
        Phase = phase;
        _updatesBeforeNextStep = 1;
    }

    /// <summary>Draws one image of the gate's movement and holds it for its updates.</summary>
    private void ShowFrame(RoomObjectSlot place, RoomObjectStepContext context, TerrainChangeFrame frame)
    {
        context.Draw(place, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
    }

    /// <summary>Draws <paramref name="image"/> and holds it for <paramref name="updates"/> in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectSlot place, RoomObjectStepContext context, RoomObjectDraw image, int updates, DownwardGatePhase phase)
    {
        context.Draw(place, image);
        _updatesBeforeNextStep = updates;
        Phase = phase;
    }
}
