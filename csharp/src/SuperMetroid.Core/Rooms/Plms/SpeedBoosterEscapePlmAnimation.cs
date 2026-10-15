using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster lavaquake's typed program (#1296); its cues install the phase pre-instructions.</summary>
internal sealed class SpeedBoosterEscapePlmAnimation()
    : PlmProgramAnimation<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapePreInstruction>(SpeedBoosterEscapeProgram.WaitForSpeedBooster)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapePreInstruction> Programs =>
        SpeedBoosterEscapePlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? ProgramAfterSleep(SpeedBoosterEscapeProgram program) =>
        SpeedBoosterEscapePlmProgramDefinitions.PhaseAfter(program);

    /// <inheritdoc/>
    private protected override ushort DrawList(PlmNoDraw frame) =>
        throw new InvalidOperationException($"The lavaquake program never draws; got {frame}.");

    /// <inheritdoc/>
    private protected override SpeedBoosterEscapeProgram? Publish(SpeedBoosterEscapePreInstruction cue, PlmSlot slot, PlmAnimationContext context)
    {
        slot.PreInstruction = (ushort)cue;
        return null;
    }
}
