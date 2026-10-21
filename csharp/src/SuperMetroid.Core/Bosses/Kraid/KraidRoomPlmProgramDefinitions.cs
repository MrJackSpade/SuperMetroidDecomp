using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Kraid.KraidRoomProgram, SuperMetroid.Core.Rooms.Plms.KraidRoomDraw, SuperMetroid.Core.Bosses.Kraid.KraidRoomCue>;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The Kraid room's ceiling, platform and spike PLM programs (#1296), in place of their bank-$84 instruction lists
/// at $84:AB6D..ABE2. A crumble shows three crumble frames for three updates each before its final block; the spike
/// program crumbles eleven pairs of columns.
/// </summary>
internal static class KraidRoomPlmProgramDefinitions
{
    /// <summary>Updates each crumble or final block frame lasts.</summary>
    internal const int CrumbleFrameDuration = 3;
    /// <summary>Two-column spike passes the spike program runs.</summary>
    internal const int SpikePassCount = 11;

    /// <summary>Every Kraid room program.</summary>
    internal static AnimationProgramSet<KraidRoomProgram, KraidRoomDraw, KraidRoomCue> Programs { get; } = new(
        new Dictionary<KraidRoomProgram, Step[]>
        {
            [KraidRoomProgram.CrumbleCeilingIntoBackground1] = [.. Crumble(), Step.FallInto(KraidRoomProgram.SetCeilingToBackground1)],
            [KraidRoomProgram.SetCeilingToBackground1] = [Step.Show(KraidRoomDraw.CeilingBackground1, CrumbleFrameDuration), Step.Stop],
            [KraidRoomProgram.CrumbleCeilingIntoBackground2] = [.. Crumble(), Step.FallInto(KraidRoomProgram.SetCeilingToBackground2)],
            [KraidRoomProgram.SetCeilingToBackground2] = [Step.Show(KraidRoomDraw.CeilingBackground2, CrumbleFrameDuration), Step.Stop],
            [KraidRoomProgram.CrumbleCeilingIntoBackground3] = [.. Crumble(), Step.FallInto(KraidRoomProgram.SetCeilingToBackground3)],
            [KraidRoomProgram.SetCeilingToBackground3] = [Step.Show(KraidRoomDraw.CeilingBackground3, CrumbleFrameDuration), Step.Stop],
            [KraidRoomProgram.ClearCeiling] = [Step.Show(KraidRoomDraw.ClearCeiling, 1), Step.Stop],
            [KraidRoomProgram.CrumbleSpikes] = [Step.SetCounter(SpikePassCount), Step.FallInto(KraidRoomProgram.CrumbleSpikesPass)],
            [KraidRoomProgram.CrumbleSpikesPass] =
            [
                .. Crumble(), Step.Show(KraidRoomDraw.SpikeFirst, CrumbleFrameDuration), Step.Publish(KraidRoomCue.MoveRightOneBlock),
                .. Crumble(), Step.Show(KraidRoomDraw.SpikeSecond, CrumbleFrameDuration), Step.Publish(KraidRoomCue.MoveRightOneBlock),
                Step.RepeatWhileCounting(KraidRoomProgram.CrumbleSpikesPass),
                Step.Stop,
            ],
            [KraidRoomProgram.ClearSpikes] = [Step.Show(KraidRoomDraw.ClearSpikes, 1), Step.Stop],
        });

    /// <summary>The three crumble frames every ceiling block and spike column shows first.</summary>
    private static Step[] Crumble() =>
    [
        Step.Show(KraidRoomDraw.CrumbleFirst, CrumbleFrameDuration),
        Step.Show(KraidRoomDraw.CrumbleSecond, CrumbleFrameDuration),
        Step.Show(KraidRoomDraw.CrumbleThird, CrumbleFrameDuration),
    ];
}
