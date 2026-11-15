using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Maridia's glass tube (#1554). On its first update, a tube broken on an earlier visit restores the room's water and
/// leaves; otherwise it shows the intact tube. From its second update on, before its timing runs, a Power Bomb that
/// struck its block wakes it to wait for input, and any other strike plays the ineffective-shot sound; every strike is
/// then read. While it waits, a newly pressed accepted button breaks it on that update: Samus is locked, the crack
/// appears and the tube plays its cracking (#1994), then shatters with its sound, shards, bubbles and earthquake, and
/// once the open tube's time is up the broken event is set, the water returns, Samus is released and the tube leaves.
/// </summary>
/// <param name="owner">The room's tube owner, which reaches Samus, the events, the water, the earthquake and the debris.</param>
[RoomLifetime]
internal sealed class NoobTubeObject(NoobTubeObjects owner) : SequencedRoomObject<TerrainChangeFrame>, IShootableRoomObject
{
    /// <summary>The room's tube owner.</summary>
    private readonly NoobTubeObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the tube is in its course.</summary>
    internal NoobTubePhase Phase { get; private set; } = NoobTubePhase.Starting;

    /// <summary>The strike on the tube's block it has not yet read, or null for none.</summary>
    internal ResidentObjectProjectileHit? PendingHit { get; private set; }

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => PendingHit = hit;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        Wake(place, context);
        base.Update(place, context);
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case NoobTubePhase.Starting when _owner.IsBroken():
                _owner.EnableWaterPhysics();
                context.RoomObjects.DeleteObject(place);
                return;
            case NoobTubePhase.Starting:
                Phase = NoobTubePhase.WaitingForPowerBomb;
                Play([new(RoomObjectDraw.Of(NoobTubeObjectDefinitions.IntactImage), 1)], place, context);
                return;
            case NoobTubePhase.WaitingForPowerBomb:
            case NoobTubePhase.WaitingForInput:
                // The tube rests until a wake starts its next step.
                Rest();
                return;
            case NoobTubePhase.Cracking:
                context.QueueSound(NoobTubeObjectDefinitions.BreakSound, NoobTubeObjectDefinitions.BreakSoundPriority);
                _owner.SpawnShardsAndBubbles(place);
                _owner.TriggerEarthquake();
                Phase = NoobTubePhase.Shattered;
                Play([NoobTubeObjectDefinitions.Shattered], place, context);
                return;
            case NoobTubePhase.Shattered:
                _owner.SetBrokenEvent();
                _owner.EnableWaterPhysics();
                _owner.UnlockSamus();
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"The n00b tube's sequence ended in {nameof(NoobTubePhase)} {Phase}.");
        }
    }

    /// <inheritdoc/>
    private protected override int Show(TerrainChangeFrame step, RoomObjectPlace place, RoomObjectStepContext context)
    {
        context.Draw(place, step.Draw);
        return step.Updates;
    }

    /// <summary>
    /// Reads the update's strike or input while the tube waits for one, and starts the step it wakes: a Power Bomb's
    /// strike sets it waiting for input, and accepted input breaks it, locking Samus and spawning the crack before the
    /// cracking's first image shows on this same update.
    /// </summary>
    private void Wake(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case NoobTubePhase.WaitingForPowerBomb:
                if (PendingHit?.Family == SamusProjectileFamily.PowerBomb)
                {
                    Phase = NoobTubePhase.WaitingForInput;
                    EndOnNextUpdate();
                }
                else if (PendingHit is not null)
                {
                    context.QueueSound(NoobTubeObjectDefinitions.IneffectiveShotSound, SoundRequestPriority.Normal);
                }
                PendingHit = null;
                return;
            case NoobTubePhase.WaitingForInput when (context.Frame.ControllerNewInput & NoobTubeObjectDefinitions.AcceptedWakeInputMask) != ControllerButton.None:
                // The break starts on this update.
                _owner.LockSamus();
                _owner.SpawnCrack(place);
                Phase = NoobTubePhase.Cracking;
                PlayFromNextUpdate(NoobTubeObjectDefinitions.Cracking);
                return;
            case NoobTubePhase.Starting:
            case NoobTubePhase.WaitingForInput:
            case NoobTubePhase.Cracking:
            case NoobTubePhase.Shattered:
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubePhase)} {Phase}.");
        }
    }
}
