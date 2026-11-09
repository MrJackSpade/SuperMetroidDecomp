using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Maridia's glass tube (#1554). On its first update, a tube broken on an earlier visit restores the room's water and
/// leaves; otherwise it shows the intact tube. From its second update on, before its timing runs, a Power Bomb that
/// struck its block wakes it to wait for input, and any other strike plays the ineffective-shot sound; every strike is
/// then read. While it waits, a newly pressed accepted button breaks it on that update: Samus is locked, the crack
/// appears and the tube cracks image by image, then shatters with its sound, shards, bubbles and earthquake, and once
/// the open tube's time is up the broken event is set, the water returns, Samus is released and the tube leaves.
/// </summary>
/// <param name="owner">The room's tube owner, which reaches Samus, the events, the water, the earthquake and the debris.</param>
internal sealed class NoobTubeObject(NoobTubeObjects owner) : RoomObjectBehaviour, IShootableRoomObject
{
    /// <summary>The room's tube owner.</summary>
    private readonly NoobTubeObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the tube is in its course.</summary>
    internal NoobTubePhase Phase { get; private set; } = NoobTubePhase.Starting;

    /// <summary>The strike on the tube's block it has not yet read, or null for none.</summary>
    internal ResidentObjectProjectileHit? PendingHit { get; private set; }

    /// <summary>Which cracking image the tube shows.</summary>
    private int _frame;

    /// <summary>The updates left before the tube's next step.</summary>
    private int _updatesBeforeNextStep = 1;

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => PendingHit = hit;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        Wake(context);
        if (--_updatesBeforeNextStep != 0)
            return;
        switch (Phase)
        {
            case NoobTubePhase.Starting when _owner.IsBroken():
                _owner.EnableWaterPhysics();
                context.RoomObjects.DeleteObject(place);
                return;
            case NoobTubePhase.Starting:
                context.Draw(place, RoomObjectDraw.Of(NoobTubeObjectDefinitions.IntactImage));
                _updatesBeforeNextStep = 1;
                Phase = NoobTubePhase.WaitingForPowerBomb;
                return;
            case NoobTubePhase.WaitingForPowerBomb:
            case NoobTubePhase.WaitingForInput:
                // The tube rests until a wake starts its next step.
                _updatesBeforeNextStep = 1;
                return;
            case NoobTubePhase.Breaking:
                _owner.LockSamus();
                _owner.SpawnCrack(place);
                _frame = 0;
                Show(place, context, NoobTubeObjectDefinitions.Cracking[0], NoobTubePhase.Cracking);
                return;
            case NoobTubePhase.Cracking when _frame + 1 < NoobTubeObjectDefinitions.Cracking.Count:
                _frame++;
                Show(place, context, NoobTubeObjectDefinitions.Cracking[_frame], NoobTubePhase.Cracking);
                return;
            case NoobTubePhase.Cracking:
                context.QueueSound(NoobTubeObjectDefinitions.BreakSound, NoobTubeObjectDefinitions.BreakSoundPriority);
                _owner.SpawnShardsAndBubbles(place);
                _owner.TriggerEarthquake();
                Show(place, context, NoobTubeObjectDefinitions.Shattered, NoobTubePhase.Shattered);
                return;
            case NoobTubePhase.Shattered:
                _owner.SetBrokenEvent();
                _owner.EnableWaterPhysics();
                _owner.UnlockSamus();
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubePhase)} {Phase}.");
        }
    }

    /// <summary>Reads the update's strike or input while the tube waits for one, and starts the step it wakes.</summary>
    private void Wake(RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case NoobTubePhase.WaitingForPowerBomb:
                if (PendingHit?.Family == SamusProjectileFamily.PowerBomb)
                {
                    Phase = NoobTubePhase.WaitingForInput;
                    _updatesBeforeNextStep = 1;
                }
                else if (PendingHit is not null)
                {
                    context.QueueSound(NoobTubeObjectDefinitions.IneffectiveShotSound, SoundRequestPriority.Normal);
                }
                PendingHit = null;
                return;
            case NoobTubePhase.WaitingForInput when (context.Frame.ControllerNewInput & NoobTubeObjectDefinitions.AcceptedWakeInputMask) != ControllerButton.None:
                // The break starts on this update.
                Phase = NoobTubePhase.Breaking;
                _updatesBeforeNextStep = 1;
                return;
            case NoobTubePhase.Starting:
            case NoobTubePhase.WaitingForInput:
            case NoobTubePhase.Breaking:
            case NoobTubePhase.Cracking:
            case NoobTubePhase.Shattered:
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubePhase)} {Phase}.");
        }
    }

    /// <summary>Draws <paramref name="frame"/> and holds it for its updates in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectSlot place, RoomObjectStepContext context, TerrainChangeFrame frame, NoobTubePhase phase)
    {
        context.Draw(place, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
        Phase = phase;
    }
}
