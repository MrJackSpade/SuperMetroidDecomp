using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.NoobTubeProgram, SuperMetroid.Core.Rooms.Plms.NoobTubeDraw, SuperMetroid.Core.Rooms.Plms.NoobTubeCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's PLM programs (#1296), in place of its bank-$84 instruction lists.</summary>
internal static class NoobTubePlmProgramDefinitions
{
    /// <summary>How long the first broken image shows before the tube opens.</summary>
    private const int CrackedDuration = 48;
    /// <summary>How long the opened tube shows before the room's water is restored.</summary>
    private const int OpenedDuration = 96;
    /// <summary>The break sound's native queue cap.</summary>
    internal const byte BreakSoundMaximumQueued = 6;

    /// <summary>Every n00b-tube program.</summary>
    internal static AnimationProgramSet<NoobTubeProgram, NoobTubeDraw, NoobTubeCue> Programs { get; } = new(
        new Dictionary<NoobTubeProgram, Step[]>
        {
            [NoobTubeProgram.Main] =
            [
                Step.Publish(NoobTubeCue.SkipIfBroken),
                Step.Publish(NoobTubeCue.LinkInputWait),
                Step.Publish(NoobTubeCue.ArmPowerBombWake),
                Step.Show(NoobTubeDraw.Intact, 1),
                Step.Hold,
            ],
            [NoobTubeProgram.WaitForInput] =
            [
                Step.Publish(NoobTubeCue.LinkBreak),
                Step.Publish(NoobTubeCue.ArmInputWake),
                Step.Hold,
            ],
            [NoobTubeProgram.Break] =
            [
                Step.Publish(NoobTubeCue.ClearPreInstruction),
                Step.Publish(NoobTubeCue.LockSamus),
                Step.Publish(NoobTubeCue.SpawnCrack),
                Step.Show(NoobTubeDraw.Damaged, CrackedDuration),
                Step.Show(NoobTubeDraw.OpenedRows, 1),
                Step.Show(NoobTubeDraw.BrokenFull, 1),
                Step.Publish(NoobTubeCue.QueueBreakSound),
                Step.Publish(NoobTubeCue.SpawnShardsAndBubbles),
                Step.Publish(NoobTubeCue.TriggerEarthquake),
                Step.Show(NoobTubeDraw.Opened, OpenedDuration),
                Step.Publish(NoobTubeCue.SetBrokenEvent),
                Step.Publish(NoobTubeCue.EnableWaterPhysics),
                Step.Publish(NoobTubeCue.UnlockSamus),
                Step.Stop,
            ],
            [NoobTubeProgram.AlreadyBroken] =
            [
                Step.Publish(NoobTubeCue.EnableWaterPhysics),
                Step.Stop,
            ],
        });
}
