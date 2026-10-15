using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Fx;
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
    private protected override ushort DrawList(SporeSpawnCeilingDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override SporeSpawnCeilingProgram? Publish(SporeSpawnCeilingCue cue, PlmSlot slot, PlmAnimationContext context)
    {
        if (cue != SporeSpawnCeilingCue.QueueCrumbleSound)
            throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCeilingCue)} {cue}.");
        context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId),
            SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundMaximumQueued);
        return null;
    }
}
