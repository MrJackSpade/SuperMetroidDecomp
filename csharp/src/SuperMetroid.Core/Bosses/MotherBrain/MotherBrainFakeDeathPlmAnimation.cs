using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A Mother Brain fake-death room mutation PLM's typed program.</summary>
internal sealed class MotherBrainFakeDeathPlmAnimation(MotherBrainFakeDeathProgram program)
    : PlmProgramAnimation<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, PlmNoCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, PlmNoCue> Programs =>
        MotherBrainFakeDeathPlmProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(MotherBrainFakeDeathDraw frame) => (ushort)frame;

    /// <inheritdoc/>
    private protected override MotherBrainFakeDeathProgram? Publish(PlmNoCue cue, PlmSlot slot, PlmStepContext context) =>
        throw new InvalidOperationException($"Mother Brain's fake-death programs publish no cues; got {cue}.");
}
