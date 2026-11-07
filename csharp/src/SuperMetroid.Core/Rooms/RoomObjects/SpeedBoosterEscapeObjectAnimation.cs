using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster lavaquake's typed program (#1296); its cues install the phase pre-instructions.</summary>
internal sealed class SpeedBoosterEscapeObjectAnimation()
    : RoomObjectProgramAnimation<SpeedBoosterEscapeProgram, RoomObjectNoDraw, SpeedBoosterEscapeFrameBehavior>(SpeedBoosterEscapeProgram.WaitForSpeedBooster)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterEscapeProgram, RoomObjectNoDraw, SpeedBoosterEscapeFrameBehavior> Programs =>
        SpeedBoosterEscapeObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? ProgramAfterSleep(SpeedBoosterEscapeProgram program) =>
        SpeedBoosterEscapePhases.PhaseAfter(program);

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(RoomObjectNoDraw frame) =>
        throw new InvalidOperationException($"The lavaquake program never draws; got {frame}.");

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? Publish(SpeedBoosterEscapeFrameBehavior cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        slot.FrameBehavior = SpeedBoosterEscapeFrameBehaviors.Of(cue);
        return null;
    }
}
