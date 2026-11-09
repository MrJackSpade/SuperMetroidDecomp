using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A block-reaction PLM's typed program (#1296).</summary>
internal sealed class BlockReactionObjectAnimation(BlockReactionObjectProgram program)
    : RoomObjectProgramAnimation<BlockReactionObjectProgram, BlockReactionObjectFrame, BlockReactionObjectCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BlockReactionObjectProgram, BlockReactionObjectFrame, BlockReactionObjectCue> Programs =>
        BlockReactionObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(BlockReactionObjectFrame frame) =>
        BlockReactionObjectDrawDefinitions.DrawLists.TryGetValue(frame, out RoomObjectDraw? drawList)
            ? drawList
            : throw new InvalidOperationException($"Block-reaction frame {frame} draws the saved block, not a draw list.");

    /// <inheritdoc/>
    private protected override void Draw(BlockReactionObjectFrame frame, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (frame == BlockReactionObjectFrame.RestoredBlock)
            context.DrawRestoredBlock(slot);
        else
            base.Draw(frame, slot, context);
    }

    /// <inheritdoc/>
    private protected override BlockReactionObjectProgram? Publish(BlockReactionObjectCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue.Kind)
        {
            case BlockReactionObjectCueKind.QueueSoundWhenIdle:
                context.QueueSound(cue.QueuedSound, SoundRequestPriority.WhenIdle);
                break;
            case BlockReactionObjectCueKind.QueueSoundAtLowPriority:
                context.QueueSound(cue.QueuedSound, SoundRequestPriority.Low);
                break;
            case BlockReactionObjectCueKind.QueueSoundAtNormalPriority:
                context.QueueSound(cue.QueuedSound, SoundRequestPriority.Normal);
                break;
            case BlockReactionObjectCueKind.RestoreRespawningGrappleBehavior:
                context.Level.SetBehavior(slot.BlockIndex, BlockReactionObjectDefinitions.RespawningGrappleBehavior);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BlockReactionObjectCueKind)} {cue.Kind}.");
        }
        return null;
    }
}
