using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A Mother Brain fake-death room mutation PLM's typed program.</summary>
internal sealed class MotherBrainFakeDeathPlmAnimation(MotherBrainFakeDeathProgram program)
    : PlmCuelessProgramAnimation<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, PlmNoCue> Programs =>
        MotherBrainFakeDeathPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override PlmDraw DrawList(MotherBrainFakeDeathDraw frame) => PlmDraw.Of(frame);
}
