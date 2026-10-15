using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Fx.AnimationProgramStep<SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingProgram, SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingDraw, SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingCue>;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's ceiling PLM programs (#1296), in place of their bank-$84 instruction lists: the crumble plays its
/// sound and three images and runs into the clear.
/// </summary>
internal static class SporeSpawnCeilingPlmProgramDefinitions
{
    /// <summary>The crumble sound, library 2.</summary>
    internal const byte CrumbleSoundId = 0x0a;
    /// <summary>The crumble sound's native queue cap.</summary>
    internal const byte CrumbleSoundMaximumQueued = 6;
    /// <summary>How long each image shows.</summary>
    internal const ushort FrameDuration = 4;

    /// <summary>Both ceiling programs.</summary>
    internal static AnimationProgramSet<SporeSpawnCeilingProgram, SporeSpawnCeilingDraw, SporeSpawnCeilingCue> Programs { get; } = new(
        new Dictionary<SporeSpawnCeilingProgram, Step[]>
        {
            [SporeSpawnCeilingProgram.Crumble] =
            [
                Step.Publish(SporeSpawnCeilingCue.QueueCrumbleSound),
                Step.Show(SporeSpawnCeilingDraw.CrumbleFirst, FrameDuration),
                Step.Show(SporeSpawnCeilingDraw.CrumbleSecond, FrameDuration),
                Step.Show(SporeSpawnCeilingDraw.CrumbleThird, FrameDuration),
                Step.FallInto(SporeSpawnCeilingProgram.Clear),
            ],
            [SporeSpawnCeilingProgram.Clear] = [Step.Show(SporeSpawnCeilingDraw.Clear, FrameDuration), Step.Stop],
        });
}
