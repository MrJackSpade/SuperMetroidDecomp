using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
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
    private protected override MotherBrainGlassPlmProgram? Publish(MotherBrainGlassPlmCue cue, PlmSlot slot, PlmAnimationContext context)
    {
        MotherBrainGlassPlms glass = context.Plms.MotherBrainGlass;
        switch (cue.Kind)
        {
            case MotherBrainGlassPlmCueKind.SkipIfBossDead:
                return glass.IsBossDead() ? MotherBrainGlassPlmProgram.BossDead : null;
            case MotherBrainGlassPlmCueKind.SkipIfGlassDestroyed:
                return glass.IsGlassDestroyed() ? MotherBrainGlassPlmProgram.NoGlass : null;
            case MotherBrainGlassPlmCueKind.ArmHitWake:
                slot.PreInstruction = MotherBrainGlassPlmProgramDefinitions.HitPreInstruction;
                return null;
            case MotherBrainGlassPlmCueKind.RepeatUntilHits:
                return slot.RoomArgument < cue.Hits ? Cursor.Program : null;
            case MotherBrainGlassPlmCueKind.SpawnShards:
                glass.SpawnShards(slot, cue.Shards);
                return null;
            case MotherBrainGlassPlmCueKind.SetGlassDestroyed:
                glass.SetGlassDestroyed();
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassPlmCueKind)} {cue.Kind}.");
        }
    }
}
