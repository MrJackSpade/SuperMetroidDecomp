using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A scripted room PLM's typed program (#1296).</summary>
internal sealed class ScriptedRoomObjectAnimation(ScriptedRoomObjectProgram program)
    : RoomObjectProgramAnimation<ScriptedRoomObjectProgram, ScriptedRoomObjectFrame, ScriptedRoomObjectCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ScriptedRoomObjectProgram, ScriptedRoomObjectFrame, ScriptedRoomObjectCue> Programs =>
        ScriptedRoomObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(ScriptedRoomObjectFrame frame) => ScriptedRoomObjectDrawDefinitions.DrawLists[frame];

    /// <inheritdoc/>
    private protected override ScriptedRoomObjectProgram? ProgramAfterSleep(ScriptedRoomObjectProgram program) =>
        program == ScriptedRoomObjectProgram.OldTourianWallWait ? ScriptedRoomObjectProgram.OldTourianWallOpen : null;

    /// <inheritdoc/>
    private protected override ScriptedRoomObjectProgram? Publish(ScriptedRoomObjectCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        switch (cue)
        {
            case ScriptedRoomObjectCue.InstallFrameBehavior installFrameBehavior:
                slot.FrameBehavior = context.RoomObjects.ScriptedHooks.FrameBehavior(installFrameBehavior.FrameBehavior ?? throw new InvalidOperationException(
                    $"Undefined {nameof(ScriptedRoomObjectFrameBehavior)} in {installFrameBehavior}."));
                return null;
            case ScriptedRoomObjectCue.Command { Kind: ScriptedRoomObjectCueKind.ClearFrameBehavior }:
                slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
                return null;
            case ScriptedRoomObjectCue.Command { Kind: ScriptedRoomObjectCueKind.ContinueIfLowerNorfairAcidLowered }:
                return context.RoomObjects.ScriptedHooks.HasLoweredLowerNorfairAcid ? ScriptedRoomObjectProgram.LowerNorfairChozoHandLowered : null;
            case ScriptedRoomObjectCue.Command { Kind: ScriptedRoomObjectCueKind.SetLoweredAcidHeight }:
                context.RoomObjects.ScriptedHooks.SetLoweredAcidHeight();
                return null;
            case ScriptedRoomObjectCue.Command { Kind: ScriptedRoomObjectCueKind.TransformSpikesToSlopes }:
                context.RoomObjects.ScriptedHooks.TransformSpikesToSlopes(context.Level);
                return null;
            case ScriptedRoomObjectCue.Command { Kind: ScriptedRoomObjectCueKind.RevertSlopesToSpikes }:
                context.RoomObjects.ScriptedHooks.RevertSlopesToSpikes(context.Level);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(ScriptedRoomObjectCueKind)} {cue}.");
        }
    }
}
