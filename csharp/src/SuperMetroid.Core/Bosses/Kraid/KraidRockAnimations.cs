using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Kraid.KraidRockProgram, SuperMetroid.Core.Bosses.Kraid.KraidRockFrame, SuperMetroid.Core.Bosses.Kraid.KraidRockCue>;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's typed rock programs (#641), in place of their bank-$86 instruction lists.</summary>
internal static class KraidRockAnimations
{
    /// <summary>Frames in the spat rock's burst.</summary>
    internal const int ShotPhaseCount = 5;

    /// <summary>The still poses last the native maximum, so the rock holds them for good in play.</summary>
    private const int StillFrameDuration = 32767;

    /// <summary>
    /// Each rock program: the shared and right rising rocks hold their still pose; the spat rock's burst takes the
    /// palette pre-instruction, shows five four-update frames, then deletes itself.
    /// </summary>
    internal static AnimationProgramSet<KraidRockProgram, KraidRockFrame, KraidRockCue> Programs { get; } = new(
        new Dictionary<KraidRockProgram, Step[]>
        {
            [KraidRockProgram.SharedPose] = [Step.Show(new(KraidRockProgram.SharedPose, 0), StillFrameDuration), Step.Hold],
            [KraidRockProgram.RisingRightPose] = [Step.Show(new(KraidRockProgram.RisingRightPose, 0), StillFrameDuration), Step.Hold],
            [KraidRockProgram.SpitRockShot] =
            [
                Step.Publish(KraidRockCue.UsePaletteZero),
                .. Enumerable.Range(0, ShotPhaseCount)
                    .Select(phase => Step.Show(new(KraidRockProgram.SpitRockShot, phase), 4)),
                Step.Publish(KraidRockCue.Delete),
                Step.Hold,
            ],
        });
}
