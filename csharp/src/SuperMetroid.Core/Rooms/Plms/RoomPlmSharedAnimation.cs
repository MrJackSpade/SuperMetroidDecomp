using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM running one of the shared one-instruction programs (#1296).</summary>
internal sealed class RoomPlmSharedAnimation(RoomPlmSharedProgram program)
    : PlmProgramAnimation<RoomPlmSharedProgram, PlmNoDraw, PlmNoCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<RoomPlmSharedProgram, PlmNoDraw, PlmNoCue> Programs =>
        RoomPlmSharedProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(PlmNoDraw frame) =>
        throw new InvalidOperationException($"The shared PLM programs never draw; got {frame}.");

    /// <inheritdoc/>
    private protected override RoomPlmSharedProgram? Publish(PlmNoCue cue, PlmSlot slot, PlmStepContext context) =>
        throw new InvalidOperationException($"The shared PLM programs publish no cues; got {cue}.");
}
