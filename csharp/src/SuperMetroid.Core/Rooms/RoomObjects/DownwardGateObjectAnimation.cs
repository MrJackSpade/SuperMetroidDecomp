using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A downward gate's or trigger's typed program (#1296).</summary>
internal sealed class DownwardGateObjectAnimation(DownwardGateProgram program)
    : RoomObjectProgramAnimation<DownwardGateProgram, DownwardGateDraw, DownwardGateCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DownwardGateProgram, DownwardGateDraw, DownwardGateCue> Programs =>
        DownwardGatePrograms.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(DownwardGateDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override DownwardGateProgram? ProgramAfterSleep(DownwardGateProgram program) =>
        DownwardGatePrograms.ProgramAfterSleep(program);

    /// <inheritdoc/>
    private protected override DownwardGateProgram? Publish(DownwardGateCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case DownwardGateCue.ClearTrigger:
                slot.LoopTimer = WrappingCountdown.Start(0);
                break;
            case DownwardGateCue.ArmWakeIfTriggered:
                slot.FrameBehavior = DownwardGateFrameBehaviors.WakeIfTriggered;
                break;
            case DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow:
                slot.FrameBehavior = DownwardGateFrameBehaviors.WakeIfTriggeredOrSamusBelow;
                break;
            case DownwardGateCue.SpawnMovingGate:
                context.RoomObjects.Gates.SpawnMovingGateProjectile(slot);
                break;
            case DownwardGateCue.WakeGateProjectile:
                context.RoomObjects.Gates.WakeGateProjectile(slot);
                break;
            case DownwardGateCue.QueueMovementSound:
                context.QueueSound(DownwardGateDefinitions.MovementSound,
                    DownwardGateObjectProgramDefinitions.MovementSoundPriority);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGateCue)} {cue}.");
        }
        return null;
    }
}
