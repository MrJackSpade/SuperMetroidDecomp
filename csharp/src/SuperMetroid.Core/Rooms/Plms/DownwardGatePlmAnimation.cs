using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A downward gate's or trigger's typed program (#1296).</summary>
internal sealed class DownwardGatePlmAnimation(DownwardGateProgram program)
    : PlmProgramAnimation<DownwardGateProgram, DownwardGateDraw, DownwardGateCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DownwardGateProgram, DownwardGateDraw, DownwardGateCue> Programs =>
        DownwardGatePrograms.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(DownwardGateDraw frame) => PlmDraw.Of(frame);

    /// <inheritdoc/>
    private protected override DownwardGateProgram? ProgramAfterSleep(DownwardGateProgram program) =>
        DownwardGatePrograms.ProgramAfterSleep(program);

    /// <inheritdoc/>
    private protected override DownwardGateProgram? Publish(DownwardGateCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case DownwardGateCue.ClearTrigger:
                slot.LoopTimer = 0;
                break;
            case DownwardGateCue.ArmWakeIfTriggered:
                slot.PreInstruction = DownwardGatePreInstructions.WakeIfTriggered;
                break;
            case DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow:
                slot.PreInstruction = DownwardGatePreInstructions.WakeIfTriggeredOrSamusBelow;
                break;
            case DownwardGateCue.SpawnMovingGate:
                context.Plms.Gates.SpawnMovingGateProjectile(slot);
                break;
            case DownwardGateCue.WakeGateProjectile:
                context.Plms.Gates.WakeGateProjectile(slot);
                break;
            case DownwardGateCue.QueueMovementSound:
                context.QueueSound(DownwardGateDefinitions.MovementSound,
                    DownwardGatePlmProgramDefinitions.MovementSoundMaximumQueued);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGateCue)} {cue}.");
        }
        return null;
    }
}
