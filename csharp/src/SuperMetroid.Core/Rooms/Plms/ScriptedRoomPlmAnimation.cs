using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;
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
    private protected override ushort DrawList(ScriptedRoomPlmFrame frame) => ScriptedRoomPlmDrawDefinitions.DrawLists[frame];

    /// <inheritdoc/>
    private protected override ScriptedRoomPlmProgram? ProgramAfterSleep(ScriptedRoomPlmProgram program) =>
        program == ScriptedRoomPlmProgram.OldTourianWallWait ? ScriptedRoomPlmProgram.OldTourianWallOpen : null;

    /// <inheritdoc/>
    private protected override ScriptedRoomPlmProgram? Publish(ScriptedRoomPlmCue cue, PlmSlot slot, PlmStepContext context)
    {
        switch (cue.Kind)
        {
            case ScriptedRoomPlmCueKind.QueueSoundLibrary2Maximum3:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, checked((byte)cue.SoundId)), 3);
                return null;
            case ScriptedRoomPlmCueKind.QueueSoundLibrary2Maximum6:
                context.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, checked((byte)cue.SoundId)), 6);
                return null;
            case ScriptedRoomPlmCueKind.InstallPreInstruction:
                slot.PreInstruction = cue.PreInstruction switch
                {
                    ScriptedRoomPlmPreInstruction.ShaktoolRoomController => ShaktoolRoomPreInstructions.Controller,
                    ScriptedRoomPlmPreInstruction.OldTourianWallWaitForSamus => TourianPreInstructions.OldTourianEscapeShaftWallWaitForSamus,
                    ScriptedRoomPlmPreInstruction.LowerNorfairChozoHandWait => ChozoStatuePreInstructions.WaitForLowerNorfairHand,
                    _ => throw new InvalidOperationException($"Undefined {nameof(ScriptedRoomPlmPreInstruction)} {cue.PreInstruction}."),
                };
                return null;
            case ScriptedRoomPlmCueKind.ClearPreInstruction:
                slot.PreInstruction = PlmPreInstructions.Inert;
                return null;
            case ScriptedRoomPlmCueKind.SetAnimalsEscapedEvent:
                (context.Plms.Bindings.Events ?? throw new InvalidOperationException("Animal rescue requires the room event owner."))
                    .Set(EventNumber.CrittersEscaped);
                return null;
            case ScriptedRoomPlmCueKind.MoveRightFourBlocks:
                context.MoveOrigin(slot, 4, 0);
                return null;
            case ScriptedRoomPlmCueKind.ContinueIfLowerNorfairAcidLowered:
                return context.Plms.ChozoStatue.HasLoweredLowerNorfairAcid ? ScriptedRoomPlmProgram.LowerNorfairChozoHandLowered : null;
            case ScriptedRoomPlmCueKind.SetLoweredAcidHeight:
                context.Plms.ChozoStatue.SetLoweredAcidHeight();
                return null;
            case ScriptedRoomPlmCueKind.TransformSpikesToSlopes:
                ChozoStatuePlms.TransformSpikesToSlopes(context.Level);
                return null;
            case ScriptedRoomPlmCueKind.RevertSlopesToSpikes:
                ChozoStatuePlms.RevertSlopesToSpikes(context.Level);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ScriptedRoomPlmCueKind)} {cue.Kind}.");
        }
    }
}
