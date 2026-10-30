using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockProgram, SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockFrame, SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockCue>;
using static SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockPlmProgramDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The crumble programs of the Speed Booster blocks.</summary>
internal static class SpeedBoosterBlockPrograms
{
    /// <summary>Every Speed Booster block program.</summary>
    internal static AnimationProgramSet<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue> Programs { get; } = new(
        new Dictionary<SpeedBoosterBlockProgram, Step[]>
        {
            [SpeedBoosterBlockProgram.BrinstarSlowRespawning] = Crumble(SlowCrumbleDuration, respawn: SpeedBoosterBlockRespawn.Respawns),
            [SpeedBoosterBlockProgram.Respawning] = Crumble(CrumbleDuration, respawn: SpeedBoosterBlockRespawn.Respawns),
            [SpeedBoosterBlockProgram.DachoraRespawning] = Crumble(CrumbleDuration, respawn: SpeedBoosterBlockRespawn.Respawns),
            [SpeedBoosterBlockProgram.BrinstarSlowPermanent] = Crumble(SlowCrumbleDuration, respawn: SpeedBoosterBlockRespawn.Permanent),
            [SpeedBoosterBlockProgram.Permanent] = Crumble(CrumbleDuration, respawn: SpeedBoosterBlockRespawn.Permanent),
            [SpeedBoosterBlockProgram.BombReveal] = [Step.Show(SpeedBoosterBlockFrame.BombReveal, 1), Step.Stop],
        });

    /// <summary>The break sound and the crumble; a respawning block grows back and restores its block.</summary>
    private static Step[] Crumble(int crumbleDuration, SpeedBoosterBlockRespawn respawn)
    {
        List<Step> steps =
        [
            Step.Publish(SpeedBoosterBlockCue.QueueBreakSound),
            Step.Show(SpeedBoosterBlockFrame.Crumble0, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble1, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble2, crumbleDuration),
            Step.Show(SpeedBoosterBlockFrame.Crumble3, respawn == SpeedBoosterBlockRespawn.Respawns ? GoneDuration : 1),
        ];
        if (respawn == SpeedBoosterBlockRespawn.Respawns)
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
