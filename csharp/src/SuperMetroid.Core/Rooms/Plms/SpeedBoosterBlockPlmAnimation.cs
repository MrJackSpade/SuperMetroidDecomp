using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A Speed Booster block's typed program (#1296).</summary>
internal sealed class SpeedBoosterBlockPlmAnimation(SpeedBoosterBlockProgram program)
    : PlmProgramAnimation<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue> Programs =>
        SpeedBoosterBlockPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(SpeedBoosterBlockFrame frame) => frame switch
    {
        SpeedBoosterBlockFrame.Crumble0 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(0),
        SpeedBoosterBlockFrame.Crumble1 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(1),
        SpeedBoosterBlockFrame.Crumble2 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(2),
        SpeedBoosterBlockFrame.Crumble3 => SpeedBoosterBlockPlmDrawDefinitions.CrumbleDrawList(3),
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
        context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, SpeedBoosterBlockPlmProgramDefinitions.BreakSoundId),
            SpeedBoosterBlockPlmProgramDefinitions.BreakSoundMaximumQueued);
        return null;
    }
}
