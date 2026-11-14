using static SuperMetroid.Core.Frontend.CeresCinematics.CeresExplosionDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Schedules the Ceres destruction blasts' waves on their host frames; each wave's blasts are
/// <see cref="CeresExplosionDefinitions"/>.
/// </summary>
internal static class CeresExplosions
{
    /// <summary>Spawner events, with the repeating frame behaviour before the final wave.</summary>
    internal static CeresExplosionSpawnEvents EventsAtFrame(int frame, bool repeatingEnabled, ref int countdown)
    {
        bool repeat = false;
        if (repeatingEnabled && frame >= RepeatingFirstFrame && frame <= SpawnerFinalFrame)
        {
            if (--countdown <= 0)
            {
                repeat = true;
                countdown = RepeatingPeriodFrames;
            }
        }
        return new(frame == FrameOf(SpawnerMilestone.InitialWave), repeat,
            frame == FrameOf(SpawnerMilestone.FinalWave));
    }

    /// <summary>Spawner-schedule milestones that each fire a wave on exactly one host frame.</summary>
    private enum SpawnerMilestone
    {
        /// <summary>The first wave spawns the initial square-and-center wave.</summary>
        InitialWave,
        /// <summary>The final wave spawns the large bursts.</summary>
        FinalWave,
    }

    /// <summary>Maps the initial and final cue milestones to their exact host-frame numbers.</summary>
    private static int FrameOf(SpawnerMilestone milestone) => milestone switch
    {
        SpawnerMilestone.InitialWave => InitialSpawnFrame,
        SpawnerMilestone.FinalWave => SpawnerFinalFrame,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpawnerMilestone)} {milestone}."),
    };
}
