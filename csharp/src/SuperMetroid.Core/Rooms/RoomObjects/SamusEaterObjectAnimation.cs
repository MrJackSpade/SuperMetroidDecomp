using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A Brinstar plant's typed program (#1296).</summary>
internal sealed class SamusEaterObjectAnimation(SamusEaterProgram program)
    : RoomObjectProgramAnimation<SamusEaterProgram, SamusEaterDraw, SamusEaterCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SamusEaterProgram, SamusEaterDraw, SamusEaterCue> Programs =>
        SamusEaterObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(SamusEaterDraw frame) => RoomObjectDraw.Of(frame);

    /// <inheritdoc/>
    private protected override SamusEaterProgram? Publish(SamusEaterCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case SamusEaterCue.HoldSamus:
                slot.FrameBehavior = SamusEaterFrameBehaviors.Hold;
                break;
            case SamusEaterCue.QueueChewSound:
                context.QueueSound(SamusEaterObjectDefinitions.ChewSound,
                    SamusEaterObjectProgramDefinitions.ChewSoundPriority);
                break;
            case SamusEaterCue.DamageSamus:
                context.RoomObjects.SamusEater.DamageSamus();
                break;
            case SamusEaterCue.ReleaseSamus:
                context.RoomObjects.SamusEater.ReleaseSamus();
                break;
            case SamusEaterCue.ClearFrameBehavior:
                slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterCue)} {cue}.");
        }
        return null;
    }
}
