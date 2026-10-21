using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A Brinstar plant's typed program (#1296).</summary>
internal sealed class SamusEaterPlmAnimation(SamusEaterProgram program)
    : PlmProgramAnimation<SamusEaterProgram, SamusEaterDraw, SamusEaterCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SamusEaterProgram, SamusEaterDraw, SamusEaterCue> Programs =>
        SamusEaterPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(SamusEaterDraw frame) => PlmDraw.Of(frame);

    /// <inheritdoc/>
    private protected override SamusEaterProgram? Publish(SamusEaterCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case SamusEaterCue.HoldSamus:
                slot.PreInstruction = SamusEaterPreInstructions.Hold;
                break;
            case SamusEaterCue.QueueChewSound:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, SamusEaterPlmRomData.ChewSound),
                    SamusEaterPlmProgramDefinitions.ChewSoundMaximumQueued);
                break;
            case SamusEaterCue.DamageSamus:
                context.Plms.SamusEater.DamageSamus();
                break;
            case SamusEaterCue.ReleaseSamus:
                context.Plms.SamusEater.ReleaseSamus();
                break;
            case SamusEaterCue.ClearPreInstruction:
                slot.PreInstruction = PlmPreInstructions.Inert;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterCue)} {cue}.");
        }
        return null;
    }
}
