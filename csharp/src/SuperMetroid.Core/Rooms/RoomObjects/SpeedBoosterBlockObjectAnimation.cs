using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A Speed Booster block's typed program (#1296).</summary>
internal sealed class SpeedBoosterBlockObjectAnimation(SpeedBoosterBlockProgram program)
    : RoomObjectProgramAnimation<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterBlockProgram, SpeedBoosterBlockFrame, SpeedBoosterBlockCue> Programs =>
        SpeedBoosterBlockPrograms.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(SpeedBoosterBlockFrame frame) => frame switch
    {
        SpeedBoosterBlockFrame.Crumble0 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(0),
        SpeedBoosterBlockFrame.Crumble1 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(1),
        SpeedBoosterBlockFrame.Crumble2 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(2),
        SpeedBoosterBlockFrame.Crumble3 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(3),
        SpeedBoosterBlockFrame.BombReveal => SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw,
        SpeedBoosterBlockFrame.RestoredBlock => throw new InvalidOperationException("The restored block draws its saved level word, not a draw list."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockFrame)} {frame}."),
    };

    /// <inheritdoc/>
    private protected override void Draw(SpeedBoosterBlockFrame frame, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (frame == SpeedBoosterBlockFrame.RestoredBlock)
            context.DrawRestoredBlock(slot);
        else
            base.Draw(frame, slot, context);
    }

    /// <inheritdoc/>
    private protected override SpeedBoosterBlockProgram? Publish(SpeedBoosterBlockCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (cue != SpeedBoosterBlockCue.QueueBreakSound)
            throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockCue)} {cue}.");
        context.QueueSound(SpeedBoosterBlockObjectProgramDefinitions.BreakSoundId,
            SpeedBoosterBlockObjectProgramDefinitions.BreakSoundPriority);
        return null;
    }
}
