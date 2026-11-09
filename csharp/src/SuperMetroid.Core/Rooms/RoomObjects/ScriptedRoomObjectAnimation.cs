using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A scripted room PLM's typed program (#1296); the remaining scripted programs only draw and end.</summary>
internal sealed class ScriptedRoomObjectAnimation(ScriptedRoomObjectProgram program)
    : RoomObjectCuelessProgramAnimation<ScriptedRoomObjectProgram, ScriptedRoomObjectFrame>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ScriptedRoomObjectProgram, ScriptedRoomObjectFrame, RoomObjectNoCue> Programs =>
        ScriptedRoomObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(ScriptedRoomObjectFrame frame) => ScriptedRoomObjectDrawDefinitions.DrawLists[frame];
}
