using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass's typed program (#1296).</summary>
internal sealed class MotherBrainGlassPlmAnimation()
    : PlmProgramAnimation<MotherBrainGlassPlmProgram, MotherBrainGlassDraw, MotherBrainGlassPlmCue>(MotherBrainGlassPlmProgram.Entry)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainGlassPlmProgram, MotherBrainGlassDraw, MotherBrainGlassPlmCue> Programs =>
        MotherBrainGlassPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(MotherBrainGlassDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override MotherBrainGlassPlmProgram? Publish(MotherBrainGlassPlmCue cue, PlmSlot slot, PlmStepContext context)
    {
        MotherBrainGlassPlms glass = context.Plms.Bosses().MotherBrainGlass;
        switch (cue)
        {
            case MotherBrainGlassPlmCue.Command { Kind: MotherBrainGlassPlmCueKind.SkipIfBossDead }:
                return glass.IsBossDead() ? MotherBrainGlassPlmProgram.BossDead : null;
            case MotherBrainGlassPlmCue.Command { Kind: MotherBrainGlassPlmCueKind.SkipIfGlassDestroyed }:
                return glass.IsGlassDestroyed() ? MotherBrainGlassPlmProgram.NoGlass : null;
            case MotherBrainGlassPlmCue.Command { Kind: MotherBrainGlassPlmCueKind.ArmHitWake }:
                slot.PreInstruction = MotherBrainGlassPreInstructions.Hit;
                return null;
            case MotherBrainGlassPlmCue.RepeatUntilHits repeatUntilHits:
                return slot.RoomArgument < repeatUntilHits.Hits ? Cursor.Program : null;
            case MotherBrainGlassPlmCue.SpawnShards spawnShards:
                glass.SpawnShards(slot, spawnShards.Shards);
                return null;
            case MotherBrainGlassPlmCue.Command { Kind: MotherBrainGlassPlmCueKind.SetGlassDestroyed }:
                glass.SetGlassDestroyed();
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassPlmCueKind)} {cue}.");
        }
    }
}
