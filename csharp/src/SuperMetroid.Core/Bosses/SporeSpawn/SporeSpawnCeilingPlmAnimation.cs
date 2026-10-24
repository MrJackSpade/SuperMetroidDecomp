using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn ceiling PLM's typed program (#1296).</summary>
internal sealed class SporeSpawnCeilingPlmAnimation(SporeSpawnCeilingProgram program)
    : PlmProgramAnimation<SporeSpawnCeilingProgram, SporeSpawnCeilingDraw, SporeSpawnCeilingCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SporeSpawnCeilingProgram, SporeSpawnCeilingDraw, SporeSpawnCeilingCue> Programs =>
        SporeSpawnCeilingPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(SporeSpawnCeilingDraw frame) => PlmDraw.Of(frame);

    /// <inheritdoc/>
    private protected override SporeSpawnCeilingProgram? Publish(SporeSpawnCeilingCue cue, PlmSlot slot, PlmStepContext context)
    {
        if (cue != SporeSpawnCeilingCue.QueueCrumbleSound)
            throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCeilingCue)} {cue}.");
        context.QueueSound(SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId,
            SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundMaximumQueued);
        return null;
    }
}
