using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn ceiling PLM's typed program (#1296).</summary>
internal sealed class SporeSpawnCeilingObjectAnimation(SporeSpawnCeilingProgram program)
    : RoomObjectProgramAnimation<SporeSpawnCeilingProgram, SporeSpawnCeilingDraw, SporeSpawnCeilingCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SporeSpawnCeilingProgram, SporeSpawnCeilingDraw, SporeSpawnCeilingCue> Programs =>
        SporeSpawnCeilingObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(SporeSpawnCeilingDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override SporeSpawnCeilingProgram? Publish(SporeSpawnCeilingCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (cue != SporeSpawnCeilingCue.QueueCrumbleSound)
            throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCeilingCue)} {cue}.");
        context.QueueSound(SporeSpawnCeilingObjectProgramDefinitions.CrumbleSoundId,
            SporeSpawnCeilingObjectProgramDefinitions.CrumbleSoundPriority);
        return null;
    }
}
