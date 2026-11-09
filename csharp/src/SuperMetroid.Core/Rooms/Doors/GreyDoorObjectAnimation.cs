using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A grey door cap's typed program (#1296).</summary>
internal sealed class GreyDoorObjectAnimation(GreyDoorVariant variant, GreyDoorProgram program)
    : RoomObjectProgramAnimation<GreyDoorProgram, DoorCapFrame, GreyDoorCue>(program), IResidentDoorAnimation
{
    /// <summary>Which grey cap this is.</summary>
    internal GreyDoorVariant Variant { get; } = variant;

    /// <inheritdoc/>
    private protected override AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue> Programs =>
        GreyDoorObjectProgramDefinitions.Sets[Variant];

    /// <inheritdoc/>
    public void StartClosing() => Restart(GreyDoorProgram.Closing);

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorCaps.Clear(Variant.Orientation),
        DoorCapFrame.Closed0 => DoorCaps.Grey(Variant.Orientation, 0),
        DoorCapFrame.Closed1 => DoorCaps.Grey(Variant.Orientation, 1),
        DoorCapFrame.Closed2 => DoorCaps.Grey(Variant.Orientation, 2),
        DoorCapFrame.Closed3 => DoorCaps.Grey(Variant.Orientation, 3),
        DoorCapFrame.BlueFlash => DoorCaps.Blue(Variant.Orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override GreyDoorProgram? Publish(GreyDoorCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case GreyDoorCue.BranchIfOpened:
                if (context.RoomObjects.DoorObjects.IsDoorOpened(slot))
                {
                    slot.Family = null;
                    slot.StartAnimation(new BlueDoorObjectAnimation(BlueDoorObjectProgramDefinitions.ClosedPrograms[Variant.Orientation]));
                }
                return null;
            case GreyDoorCue.LinkActivate:
                SetLink(GreyDoorProgram.Activate);
                return null;
            case GreyDoorCue.ArmCondition:
                slot.FrameBehavior = GreyDoorFrameBehaviors.Of(GreyDoorObjectProgramDefinitions.UnlockChecks[
                    ((slot.Family as GreyDoorObjectState) ?? throw new InvalidOperationException("A grey cap's program ran without its door state.")).Condition]);
                return null;
            case GreyDoorCue.LinkHit:
                SetLink(GreyDoorProgram.Hit);
                return null;
            case GreyDoorCue.ArmShotWake:
                slot.FrameBehavior = GreyDoorFrameBehaviors.Of(GreyDoorFrameBehavior.WakeWhenShot);
                return null;
            case GreyDoorCue.CountHit:
                return context.RoomObjects.GreyDoors.CountGreyDoorHit(slot) ? GreyDoorProgram.Open : null;
            case GreyDoorCue.QueueOpenSound:
                QueueDoorSound(context, BlueDoorObjectProgramDefinitions.OpenSound);
                return null;
            case GreyDoorCue.QueueCloseSound:
                QueueDoorSound(context, BlueDoorObjectProgramDefinitions.CloseSound);
                return null;
            case GreyDoorCue.RepeatUntilSamusHasBombs:
                return context.Frame.CollectedItems.HasAny(SamusEquipmentFlags.Bombs) ? null : GreyDoorProgram.Closing;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorCue)} {cue}.");
        }
    }

    /// <summary>Queues a room-library door sound under the door sounds' cap.</summary>
    private static void QueueDoorSound(RoomObjectStepContext context, SoundEffectId sound) =>
        context.QueueSound(sound, BlueDoorObjectProgramDefinitions.SoundPriority);
}
