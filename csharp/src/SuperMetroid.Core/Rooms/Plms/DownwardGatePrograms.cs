using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.DownwardGateProgram, SuperMetroid.Core.Rooms.Plms.DownwardGateDraw, SuperMetroid.Core.Rooms.Plms.DownwardGateCue>;
using static SuperMetroid.Core.Rooms.Plms.DownwardGatePlmProgramDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's programs and the program that follows its sleep.</summary>
internal static class DownwardGatePrograms
{
    /// <summary>Every downward gate program.</summary>
    internal static AnimationProgramSet<DownwardGateProgram, DownwardGateDraw, DownwardGateCue> Programs { get; } = new(
        new Dictionary<DownwardGateProgram, Step[]>
        {
            [DownwardGateProgram.Open] =
            [
                Step.Show(DownwardGateDraw.Open, 1),
                Step.Publish(DownwardGateCue.ClearTrigger),
                Step.Publish(DownwardGateCue.ArmWakeIfTriggered),
                Step.Hold,
            ],
            [DownwardGateProgram.Close] =
            [
                Step.Show(DownwardGateDraw.Open, CloseDelay),
                Step.Publish(DownwardGateCue.SpawnMovingGate),
                Step.Publish(DownwardGateCue.QueueMovementSound),
                Step.Show(DownwardGateDraw.Lowered1, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered2, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered3, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered4, LastMovementFrameDuration),
                Step.FallInto(DownwardGateProgram.Closed),
            ],
            [DownwardGateProgram.Closed] =
            [
                Step.Show(DownwardGateDraw.Closed, 1),
                Step.Publish(DownwardGateCue.ClearTrigger),
                Step.Publish(DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow),
                Step.Hold,
            ],
            [DownwardGateProgram.Reopen] =
            [
                Step.Publish(DownwardGateCue.WakeGateProjectile),
                Step.Publish(DownwardGateCue.QueueMovementSound),
                Step.Show(DownwardGateDraw.Lowered4, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered3, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered2, MovementFrameDuration),
                Step.Show(DownwardGateDraw.Lowered1, LastMovementFrameDuration),
                Step.GoTo(DownwardGateProgram.Open),
            ],
            [DownwardGateProgram.TriggerBlueLeft] = Trigger(DownwardGateDraw.TriggerBlueLeft),
            [DownwardGateProgram.TriggerBlueRight] = Trigger(DownwardGateDraw.TriggerBlueRight),
            [DownwardGateProgram.TriggerRedLeft] = Trigger(DownwardGateDraw.TriggerRedLeft),
            [DownwardGateProgram.TriggerRedRight] = Trigger(DownwardGateDraw.TriggerRedRight),
            [DownwardGateProgram.TriggerGreenLeft] = Trigger(DownwardGateDraw.TriggerGreenLeft),
            [DownwardGateProgram.TriggerGreenRight] = Trigger(DownwardGateDraw.TriggerGreenRight),
            [DownwardGateProgram.TriggerYellowLeft] = Trigger(DownwardGateDraw.TriggerYellowLeft),
            [DownwardGateProgram.TriggerYellowRight] = Trigger(DownwardGateDraw.TriggerYellowRight),
        });

    /// <summary>The program a wake past <paramref name="program"/>'s sleep continues with.</summary>
    internal static DownwardGateProgram? ProgramAfterSleep(DownwardGateProgram program) => program switch
    {
        DownwardGateProgram.Open => DownwardGateProgram.Close,
        DownwardGateProgram.Closed => DownwardGateProgram.Reopen,
        DownwardGateProgram.Close or DownwardGateProgram.Reopen
            or DownwardGateProgram.TriggerBlueLeft or DownwardGateProgram.TriggerBlueRight
            or DownwardGateProgram.TriggerRedLeft or DownwardGateProgram.TriggerRedRight
            or DownwardGateProgram.TriggerGreenLeft or DownwardGateProgram.TriggerGreenRight
            or DownwardGateProgram.TriggerYellowLeft or DownwardGateProgram.TriggerYellowRight => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGateProgram)} {program}."),
    };

    /// <summary>Draws a trigger's shot block for one update, then deletes the PLM.</summary>
    private static Step[] Trigger(DownwardGateDraw draw) => [Step.Show(draw, 1), Step.Stop];
}
