using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

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
    private protected override MotherBrainFakeDeathProgram? Publish(PlmNoCue cue, PlmSlot slot, PlmAnimationContext context) =>
        throw new InvalidOperationException($"Mother Brain's fake-death programs publish no cues; got {cue}.");
}
