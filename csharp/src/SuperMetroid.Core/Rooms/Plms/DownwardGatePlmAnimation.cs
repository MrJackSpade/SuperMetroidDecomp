using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A downward gate's or trigger's typed program (#1296).</summary>
internal sealed class DownwardGatePlmAnimation(DownwardGateProgram program)
    : PlmProgramAnimation<DownwardGateProgram, DownwardGateDraw, DownwardGateCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DownwardGateProgram, DownwardGateDraw, DownwardGateCue> Programs =>
        DownwardGatePlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(DownwardGateDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override DownwardGateProgram? ProgramAfterSleep(DownwardGateProgram program) =>
        DownwardGatePlmProgramDefinitions.ProgramAfterSleep(program);

    /// <inheritdoc/>
    private protected override DownwardGateProgram? Publish(DownwardGateCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case DownwardGateCue.ClearTrigger:
                slot.LoopTimer = 0;
                break;
            case DownwardGateCue.ArmWakeIfTriggered:
                slot.PreInstruction = (ushort)DownwardGatePreInstruction.WakeIfTriggered;
                break;
            case DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow:
                slot.PreInstruction = (ushort)DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow;
                break;
            case DownwardGateCue.SpawnMovingGate:
                context.Plms.Gates.SpawnMovingGateProjectile(slot);
                break;
            case DownwardGateCue.WakeGateProjectile:
                context.Plms.Gates.WakeGateProjectile(slot);
                break;
            case DownwardGateCue.QueueMovementSound:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, DownwardGatePlmRomData.MovementSound),
                    DownwardGatePlmProgramDefinitions.MovementSoundMaximumQueued);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGateCue)} {cue}.");
        }
        return null;
    }
}
