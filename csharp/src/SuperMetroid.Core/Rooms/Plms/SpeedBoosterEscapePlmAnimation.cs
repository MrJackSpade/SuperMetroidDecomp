using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster lavaquake's typed program (#1296); its cues install the phase pre-instructions.</summary>
internal sealed class SpeedBoosterEscapePlmAnimation()
    : PlmProgramAnimation<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapeFrameBehavior>(SpeedBoosterEscapeProgram.WaitForSpeedBooster)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapeFrameBehavior> Programs =>
        SpeedBoosterEscapePlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? ProgramAfterSleep(SpeedBoosterEscapeProgram program) =>
        SpeedBoosterEscapePhases.PhaseAfter(program);

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(PlmNoDraw frame) =>
        throw new InvalidOperationException($"The lavaquake program never draws; got {frame}.");

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? Publish(SpeedBoosterEscapeFrameBehavior cue, PlmSlot slot, PlmStepContext context)
    {
        slot.FrameBehavior = SpeedBoosterEscapeFrameBehaviors.Of(cue);
        return null;
    }
}
