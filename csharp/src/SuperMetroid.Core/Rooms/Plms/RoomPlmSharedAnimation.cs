using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM running one of the shared one-instruction programs (#1296).</summary>
internal sealed class RoomPlmSharedAnimation(RoomPlmSharedProgram program)
    : PlmCuelessProgramAnimation<RoomPlmSharedProgram, PlmNoDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<RoomPlmSharedProgram, PlmNoDraw, PlmNoCue> Programs =>
        RoomPlmSharedProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override ushort DrawList(PlmNoDraw frame) =>
        throw new InvalidOperationException($"The shared PLM programs never draw; got {frame}.");
}
