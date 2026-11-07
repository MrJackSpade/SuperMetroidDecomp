using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A PLM running one of the shared one-instruction programs (#1296).</summary>
internal sealed class RoomObjectSharedAnimation(RoomObjectSharedProgram program)
    : RoomObjectCuelessProgramAnimation<RoomObjectSharedProgram, RoomObjectNoDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<RoomObjectSharedProgram, RoomObjectNoDraw, RoomObjectNoCue> Programs =>
        RoomObjectSharedProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(RoomObjectNoDraw frame) =>
        throw new InvalidOperationException($"The shared PLM programs never draw; got {frame}.");
}
