using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's typed program (#1296).</summary>
internal sealed class NoobTubePlmAnimation(NoobTubeProgram program)
    : PlmProgramAnimation<NoobTubeProgram, NoobTubeDraw, NoobTubeCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<NoobTubeProgram, NoobTubeDraw, NoobTubeCue> Programs =>
        NoobTubePlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(NoobTubeDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override NoobTubeProgram? Publish(NoobTubeCue cue, PlmSlot slot, PlmStepContext context)
    {
        NoobTubePlms tube = context.Plms.NoobTube;
        switch (cue)
        {
            case NoobTubeCue.SkipIfBroken:
                return tube.IsBroken() ? NoobTubeProgram.AlreadyBroken : null;
            case NoobTubeCue.LinkInputWait:
                SetLink(NoobTubeProgram.WaitForInput);
                break;
            case NoobTubeCue.ArmPowerBombWake:
                slot.PreInstruction = NoobTubePreInstructions.WakeOnPowerBomb;
                break;
            case NoobTubeCue.LinkBreak:
                SetLink(NoobTubeProgram.Break);
                break;
            case NoobTubeCue.ArmInputWake:
                slot.PreInstruction = NoobTubePreInstructions.WakeOnAcceptedInput;
                break;
            case NoobTubeCue.ClearPreInstruction:
                slot.PreInstruction = NoobTubePreInstructions.Inactive;
                break;
            case NoobTubeCue.LockSamus:
                tube.SetSamusLocked(true);
                break;
            case NoobTubeCue.SpawnCrack:
                tube.SpawnCrack(slot);
                break;
            case NoobTubeCue.QueueBreakSound:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, NoobTubePlmRomData.BreakSound),
                    NoobTubePlmProgramDefinitions.BreakSoundMaximumQueued);
                break;
            case NoobTubeCue.SpawnShardsAndBubbles:
                tube.SpawnShardsAndBubbles(slot);
                break;
            case NoobTubeCue.TriggerEarthquake:
                tube.TriggerEarthquake();
                break;
            case NoobTubeCue.SetBrokenEvent:
                tube.SetBrokenEvent();
                break;
            case NoobTubeCue.EnableWaterPhysics:
                tube.EnableWaterPhysics();
                break;
            case NoobTubeCue.UnlockSamus:
                tube.SetSamusLocked(false);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubeCue)} {cue}.");
        }
        return null;
    }
}
