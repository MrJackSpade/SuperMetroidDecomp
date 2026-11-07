using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.RoomObjects.RoomObjectSharedProgram, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoDraw, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoCue>;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The shared one-instruction PLM programs (#1296), in place of their lists.</summary>
internal static class RoomObjectSharedProgramDefinitions
{
    /// <summary>Both shared programs.</summary>
    internal static AnimationProgramSet<RoomObjectSharedProgram, RoomObjectNoDraw, RoomObjectNoCue> Programs { get; } = new(
        new Dictionary<RoomObjectSharedProgram, Step[]>
        {
            [RoomObjectSharedProgram.Delete] = [Step.Stop],
            [RoomObjectSharedProgram.Sleep] = [Step.Hold],
        });
}
