using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A Speed Booster block's typed program (#1296).</summary>
internal sealed class SpeedBoosterBlockPlmAnimation(SpeedBoosterBlockProgram program)
    : PlmProgramAnimation<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue> Programs =>
        SpeedBoosterBlockPrograms.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(SpeedBoosterBlockFrame frame) => frame switch
    {
        SpeedBoosterBlockFrame.Crumble0 => SpeedBoosterBlockPlmDraws.CrumbleDrawList(0),
        SpeedBoosterBlockFrame.Crumble1 => SpeedBoosterBlockPlmDraws.CrumbleDrawList(1),
        SpeedBoosterBlockFrame.Crumble2 => SpeedBoosterBlockPlmDraws.CrumbleDrawList(2),
        SpeedBoosterBlockFrame.Crumble3 => SpeedBoosterBlockPlmDraws.CrumbleDrawList(3),
        SpeedBoosterBlockFrame.BombReveal => SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw,
        SpeedBoosterBlockFrame.RestoredBlock => throw new InvalidOperationException("The restored block draws its saved level word, not a draw list."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override void Draw(SpeedBoosterBlockFrame frame, PlmSlot slot, PlmStepContext context)
    {
        if (frame == SpeedBoosterBlockFrame.RestoredBlock)
            context.DrawRestoredBlock(slot);
        else
            base.Draw(frame, slot, context);
    }

    /// <inheritdoc/>
    private protected override SpeedBoosterBlockProgram? Publish(SpeedBoosterBlockCue cue, PlmSlot slot, PlmStepContext context)
    {
        if (cue != SpeedBoosterBlockCue.QueueBreakSound)
            throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockCue)} {cue}.");
        context.QueueSound(SpeedBoosterBlockPlmProgramDefinitions.BreakSoundId,
            SpeedBoosterBlockPlmProgramDefinitions.BreakSoundMaximumQueued);
        return null;
    }
}
