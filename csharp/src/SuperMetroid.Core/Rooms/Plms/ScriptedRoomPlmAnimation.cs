using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A scripted room PLM's typed program (#1296).</summary>
internal sealed class ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram program)
    : PlmProgramAnimation<ScriptedRoomPlmProgram, ScriptedRoomPlmFrame, ScriptedRoomPlmCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ScriptedRoomPlmProgram, ScriptedRoomPlmFrame, ScriptedRoomPlmCue> Programs =>
        ScriptedRoomPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(ScriptedRoomPlmFrame frame) => ScriptedRoomPlmDrawDefinitions.DrawLists[frame];

    /// <inheritdoc/>
    private protected override ScriptedRoomPlmProgram? ProgramAfterSleep(ScriptedRoomPlmProgram program) =>
        program == ScriptedRoomPlmProgram.OldTourianWallWait ? ScriptedRoomPlmProgram.OldTourianWallOpen : null;

    /// <inheritdoc/>
    private protected override ScriptedRoomPlmProgram? Publish(ScriptedRoomPlmCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue)
        {
            case ScriptedRoomPlmCue.QueueSoundLibrary2Maximum3 queueSoundLibrary2Maximum3:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, checked((byte)queueSoundLibrary2Maximum3.SoundId)), 3);
                return null;
            case ScriptedRoomPlmCue.QueueSoundLibrary2Maximum6 queueSoundLibrary2Maximum6:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, checked((byte)queueSoundLibrary2Maximum6.SoundId)), 6);
                return null;
            case ScriptedRoomPlmCue.InstallPreInstruction installPreInstruction:
                slot.PreInstruction = context.Plms.ScriptedHooks.PreInstruction(installPreInstruction.PreInstruction ?? throw new InvalidOperationException(
                    $"Undefined {nameof(ScriptedRoomPlmPreInstruction)} in {installPreInstruction}."));
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.ClearPreInstruction }:
                slot.PreInstruction = PlmPreInstructions.Inert;
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.SetAnimalsEscapedEvent }:
                (context.Plms.Events ?? throw new InvalidOperationException("Animal rescue requires the room event owner."))
                    .Set(EventNumber.CrittersEscaped);
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.MoveRightFourBlocks }:
                context.MoveOrigin(slot, 4, 0);
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.ContinueIfLowerNorfairAcidLowered }:
                return context.Plms.ScriptedHooks.HasLoweredLowerNorfairAcid ? ScriptedRoomPlmProgram.LowerNorfairChozoHandLowered : null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.SetLoweredAcidHeight }:
                context.Plms.ScriptedHooks.SetLoweredAcidHeight();
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.TransformSpikesToSlopes }:
                context.Plms.ScriptedHooks.TransformSpikesToSlopes(context.Level);
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.RevertSlopesToSpikes }:
                context.Plms.ScriptedHooks.RevertSlopesToSpikes(context.Level);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ScriptedRoomPlmCueKind)} {cue}.");
        }
    }
}
