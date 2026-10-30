using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A block-reaction PLM's typed program (#1296).</summary>
internal sealed class BlockReactionPlmAnimation(BlockReactionPlmProgram program)
    : PlmProgramAnimation<BlockReactionPlmProgram, BlockReactionPlmFrame, BlockReactionPlmCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<BlockReactionPlmProgram, BlockReactionPlmFrame, BlockReactionPlmCue> Programs =>
        BlockReactionPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(BlockReactionPlmFrame frame) =>
        BlockReactionPlmDrawDefinitions.DrawLists.TryGetValue(frame, out PlmDraw? drawList)
            ? drawList
            : throw new InvalidOperationException($"Block-reaction frame {frame} draws the saved block, not a draw list.");

    /// <inheritdoc/>
    private protected override void Draw(BlockReactionPlmFrame frame, PlmSlot slot, PlmStepContext context)
    {
        if (frame == BlockReactionPlmFrame.RestoredBlock)
            context.DrawRestoredBlock(slot);
        else
            base.Draw(frame, slot, context);
    }

    /// <inheritdoc/>
    private protected override BlockReactionPlmProgram? Publish(BlockReactionPlmCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue.Kind)
        {
            case BlockReactionPlmCueKind.QueueSoundLibrary2Maximum1:
                context.QueueSound(cue.QueuedSound, SoundQueueLimit.Max1);
                break;
            case BlockReactionPlmCueKind.QueueSoundLibrary2Maximum3:
                context.QueueSound(cue.QueuedSound, SoundQueueLimit.Max3);
                break;
            case BlockReactionPlmCueKind.QueueSoundLibrary2Maximum6:
                context.QueueSound(cue.QueuedSound, SoundQueueLimit.Max6);
                break;
            case BlockReactionPlmCueKind.RestoreRespawningGrappleBehavior:
                context.Level.SetBehavior(slot.BlockIndex, BlockReactionPlmDefinitions.RespawningGrappleBehavior);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BlockReactionPlmCueKind)} {cue.Kind}.");
        }
        return null;
    }
}
