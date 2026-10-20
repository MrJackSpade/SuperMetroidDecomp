using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockProgram, SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockFrame, SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The Speed Booster blocks' PLM programs (#1296), in place of their bank-$84 instruction lists. A block crumbles through
/// four images; a respawning block holds the last, plays them back and restores its saved level word.
/// </summary>
internal static class SpeedBoosterBlockPlmProgramDefinitions
{
    /// <summary>The break sound, library 2.</summary>
    internal const byte BreakSoundId = 0x06;
    /// <summary>The break sound's native queue cap.</summary>
    internal const byte BreakSoundMaximumQueued = 1;
    /// <summary>How long each crumble image shows on Brinstar's slow blocks.</summary>
    private const int SlowCrumbleDuration = 2;
    /// <summary>How long each crumble image shows on the other blocks.</summary>
    private const int CrumbleDuration = 1;
    /// <summary>How long a respawning block stays gone.</summary>
    private const int GoneDuration = 48;
    /// <summary>How long each image shows as a respawning block grows back.</summary>
    private const int RegrowDuration = 4;

    /// <summary>Every Speed Booster block program.</summary>
    internal static AnimationProgramSet<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue> Programs { get; } = new(
        new Dictionary<SpeedBoosterBlockProgram, Step[]>
        {
            [SpeedBoosterBlockProgram.BrinstarSlowRespawning] = Crumble(SlowCrumbleDuration, respawns: true),
            [SpeedBoosterBlockProgram.Respawning] = Crumble(CrumbleDuration, respawns: true),
            [SpeedBoosterBlockProgram.DachoraRespawning] = Crumble(CrumbleDuration, respawns: true),
            [SpeedBoosterBlockProgram.BrinstarSlowPermanent] = Crumble(SlowCrumbleDuration, respawns: false),
            [SpeedBoosterBlockProgram.Permanent] = Crumble(CrumbleDuration, respawns: false),
            [SpeedBoosterBlockProgram.BombReveal] = [Step.Show(SpeedBoosterBlockFrame.BombReveal, 1), Step.Stop],
        });

    /// <summary>The break sound and the crumble; a respawning block grows back and restores its level word.</summary>
    private static Step[] Crumble(int crumbleDuration, bool respawns)
    {
        List<Step> steps =
        [
            Step.Publish(SpeedBoosterBlockCue.QueueBreakSound),
            Step.Show(SpeedBoosterBlockFrame.Crumble0, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble1, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble2, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble3, respawns ? GoneDuration : 1),
        ];
        if (respawns)
        {
            steps.Add(Step.Show(SpeedBoosterBlockFrame.Crumble2, RegrowDuration));
            steps.Add(Step.Show(SpeedBoosterBlockFrame.Crumble1, RegrowDuration));
            steps.Add(Step.Show(SpeedBoosterBlockFrame.Crumble0, RegrowDuration));
            steps.Add(Step.Show(SpeedBoosterBlockFrame.RestoredBlock, 1));
        }
        steps.Add(Step.Stop);
        return [.. steps];
    }
}
