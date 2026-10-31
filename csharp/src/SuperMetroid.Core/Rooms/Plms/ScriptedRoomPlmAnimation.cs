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
                context.QueueSound(queueSoundLibrary2Maximum3.Sound, SoundRequestPriority.Low);
                return null;
            case ScriptedRoomPlmCue.QueueSoundLibrary2Maximum6 queueSoundLibrary2Maximum6:
                context.QueueSound(queueSoundLibrary2Maximum6.Sound, SoundRequestPriority.Normal);
                return null;
            case ScriptedRoomPlmCue.InstallFrameBehavior installFrameBehavior:
                slot.FrameBehavior = context.Plms.ScriptedHooks.FrameBehavior(installFrameBehavior.FrameBehavior ?? throw new InvalidOperationException(
                    $"Undefined {nameof(ScriptedRoomPlmFrameBehavior)} in {installFrameBehavior}."));
                return null;
            case ScriptedRoomPlmCue.Command { Kind: ScriptedRoomPlmCueKind.ClearFrameBehavior }:
                slot.FrameBehavior = PlmFrameBehaviors.Inert;
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
