using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A grey door cap's typed program (#1296).</summary>
internal sealed class GreyDoorPlmAnimation(GreyDoorVariant variant, GreyDoorProgram program)
    : PlmProgramAnimation<GreyDoorProgram, DoorCapFrame, GreyDoorCue>(program), IResidentDoorAnimation
{
    /// <summary>Which grey cap this is.</summary>
    internal GreyDoorVariant Variant { get; } = variant;

    /// <inheritdoc/>
    private protected override AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue> Programs =>
        GreyDoorPlmProgramDefinitions.ProgramsFor(Variant);

    /// <inheritdoc/>
    public void StartClosing() => Restart(GreyDoorProgram.Closing);

    /// <inheritdoc/>
    private protected override ushort DrawList(DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorPlmDrawListDefinitions.Clear(Variant.Orientation),
        DoorCapFrame.Closed0 => DoorPlmDrawListDefinitions.Grey(Variant.Orientation, 0),
        DoorCapFrame.Closed1 => DoorPlmDrawListDefinitions.Grey(Variant.Orientation, 1),
        DoorCapFrame.Closed2 => DoorPlmDrawListDefinitions.Grey(Variant.Orientation, 2),
        DoorCapFrame.Closed3 => DoorPlmDrawListDefinitions.Grey(Variant.Orientation, 3),
        DoorCapFrame.BlueFlash => DoorPlmDrawListDefinitions.Blue(Variant.Orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override GreyDoorProgram? Publish(GreyDoorCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case GreyDoorCue.BranchIfOpened:
                if (context.Plms.DoorPlms.IsDoorOpened(slot))
                {
                    slot.Family = null;
                    slot.StartAnimation(new BlueDoorPlmAnimation(BlueDoorPlmProgramDefinitions.Closed(Variant.Orientation)));
                }
                return null;
            case GreyDoorCue.LinkActivate:
                SetLink(GreyDoorProgram.Activate);
                return null;
            case GreyDoorCue.ArmCondition:
                slot.PreInstruction = GreyDoorPreInstructions.Of(GreyDoorPlmProgramDefinitions.UnlockCheckOf(
                    ((slot.Family as GreyDoorPlmState) ?? throw new InvalidOperationException("A grey cap's program ran without its door state.")).Condition));
                return null;
            case GreyDoorCue.LinkHit:
                SetLink(GreyDoorProgram.Hit);
                return null;
            case GreyDoorCue.ArmShotWake:
                slot.PreInstruction = GreyDoorPreInstructions.Of(GreyDoorPreInstruction.WakeWhenShot);
                return null;
            case GreyDoorCue.CountHit:
                return context.Plms.GreyDoors.CountGreyDoorHit(slot) ? GreyDoorProgram.Open : null;
            case GreyDoorCue.QueueOpenSound:
                QueueDoorSound(context, BlueDoorPlmProgramDefinitions.OpenSound);
                return null;
            case GreyDoorCue.QueueCloseSound:
                QueueDoorSound(context, BlueDoorPlmProgramDefinitions.CloseSound);
                return null;
            case GreyDoorCue.RepeatUntilSamusHasBombs:
                return context.Frame.CollectedItems.HasAny(SamusEquipmentFlags.Bombs) ? null : GreyDoorProgram.Closing;
            default:
                throw new InvalidOperationException($"Undefined {nameof(GreyDoorCue)} {cue}.");
        }
    }

    /// <summary>Queues a library-3 door sound under the door sounds' cap.</summary>
    private static void QueueDoorSound(PlmStepContext context, SoundEffectId sound) =>
        context.QueueSound(sound, BlueDoorPlmProgramDefinitions.SoundMaximumQueued);
}
