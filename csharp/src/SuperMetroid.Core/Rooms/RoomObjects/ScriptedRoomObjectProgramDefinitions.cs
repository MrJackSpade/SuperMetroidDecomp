using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectFrame;
using P = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectProgram;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectProgram, SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectFrame, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoCue>;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The scripted room PLMs' programs (#1296), step for step their lists.</summary>
internal static class ScriptedRoomObjectProgramDefinitions
{
    /// <summary>Every scripted room program.</summary>
    internal static AnimationProgramSet<P, F, RoomObjectNoCue> Programs { get; } = new(
        new Dictionary<P, Step[]>
        {
        });
}
