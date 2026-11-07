using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass's typed program (#1296).</summary>
internal sealed class MotherBrainGlassObjectAnimation()
    : RoomObjectProgramAnimation<MotherBrainGlassObjectProgram, MotherBrainGlassDraw, MotherBrainGlassObjectCue>(MotherBrainGlassObjectProgram.Entry)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainGlassObjectProgram, MotherBrainGlassDraw, MotherBrainGlassObjectCue> Programs =>
        MotherBrainGlassObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(MotherBrainGlassDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override MotherBrainGlassObjectProgram? Publish(MotherBrainGlassObjectCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        MotherBrainGlassObjects glass = context.RoomObjects.Bosses().MotherBrainGlass;
        switch (cue)
        {
            case MotherBrainGlassObjectCue.Command { Kind: MotherBrainGlassObjectCueKind.SkipIfBossDead }:
                return glass.IsBossDead() ? MotherBrainGlassObjectProgram.BossDead : null;
            case MotherBrainGlassObjectCue.Command { Kind: MotherBrainGlassObjectCueKind.SkipIfGlassDestroyed }:
                return glass.IsGlassDestroyed() ? MotherBrainGlassObjectProgram.NoGlass : null;
            case MotherBrainGlassObjectCue.Command { Kind: MotherBrainGlassObjectCueKind.ArmHitWake }:
                slot.FrameBehavior = MotherBrainGlassFrameBehaviors.Hit;
                return null;
            case MotherBrainGlassObjectCue.RepeatUntilHits repeatUntilHits:
                return slot.HitCounter < repeatUntilHits.Hits ? Cursor.Program : null;
            case MotherBrainGlassObjectCue.SpawnShards spawnShards:
                glass.SpawnShards(slot, spawnShards.Shards);
                return null;
            case MotherBrainGlassObjectCue.Command { Kind: MotherBrainGlassObjectCueKind.SetGlassDestroyed }:
                glass.SetGlassDestroyed();
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassObjectCueKind)} {cue}.");
        }
    }
}
