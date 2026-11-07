using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room PLM and program of each of Mother Brain's room mutations.</summary>
internal static class MotherBrainRoomMutationDefinitions
{
    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<MotherBrainRoomMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<MotherBrainRoomMutation, RoomObjectHeaderId>
        {
            [MotherBrainRoomMutation.FillWall] = RoomObjectHeaderId.FillMotherBrainsWall,
            [MotherBrainRoomMutation.EscapeDoor] = RoomObjectHeaderId.MotherBrainsRoomEscapeDoor,
            [MotherBrainRoomMutation.BackgroundRow2] = RoomObjectHeaderId.MotherBrainsBackgroundRow2,
            [MotherBrainRoomMutation.BackgroundRow3] = RoomObjectHeaderId.MotherBrainsBackgroundRow3,
            [MotherBrainRoomMutation.BackgroundRow4] = RoomObjectHeaderId.MotherBrainsBackgroundRow4,
            [MotherBrainRoomMutation.BackgroundRow5] = RoomObjectHeaderId.MotherBrainsBackgroundRow5,
            [MotherBrainRoomMutation.BackgroundRow6] = RoomObjectHeaderId.MotherBrainsBackgroundRow6,
            [MotherBrainRoomMutation.BackgroundRow7] = RoomObjectHeaderId.MotherBrainsBackgroundRow7,
            [MotherBrainRoomMutation.BackgroundRow8] = RoomObjectHeaderId.MotherBrainsBackgroundRow8,
            [MotherBrainRoomMutation.BackgroundRow9] = RoomObjectHeaderId.MotherBrainsBackgroundRow9,
            [MotherBrainRoomMutation.BackgroundRowA] = RoomObjectHeaderId.MotherBrainsBackgroundRowA,
            [MotherBrainRoomMutation.BackgroundRowB] = RoomObjectHeaderId.MotherBrainsBackgroundRowB,
            [MotherBrainRoomMutation.BackgroundRowC] = RoomObjectHeaderId.MotherBrainsBackgroundRowC,
            [MotherBrainRoomMutation.BackgroundRowD] = RoomObjectHeaderId.MotherBrainsBackgroundRowD,
            [MotherBrainRoomMutation.ClearCeilingBlock] = RoomObjectHeaderId.ClearMotherBrainCeilingBlock,
            [MotherBrainRoomMutation.ClearCeilingTube] = RoomObjectHeaderId.ClearMotherBrainCeilingTube,
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = RoomObjectHeaderId.ClearMotherBrainBottomMiddleSideTube,
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = RoomObjectHeaderId.ClearMotherBrainBottomMiddleTubes,
            [MotherBrainRoomMutation.ClearBottomLeftTube] = RoomObjectHeaderId.ClearMotherBrainBottomLeftTube,
            [MotherBrainRoomMutation.ClearBottomRightTube] = RoomObjectHeaderId.ClearMotherBrainBottomRightTube,
        };

    /// <summary>The room program each mutation runs: one draw, then deletion.</summary>
    internal static readonly IReadOnlyDictionary<MotherBrainRoomMutation, MotherBrainFakeDeathProgram> Programs =
        new Dictionary<MotherBrainRoomMutation, MotherBrainFakeDeathProgram>
        {
            [MotherBrainRoomMutation.FillWall] = MotherBrainFakeDeathProgram.FillWall,
            [MotherBrainRoomMutation.EscapeDoor] = MotherBrainFakeDeathProgram.EscapeDoor,
            [MotherBrainRoomMutation.BackgroundRow2] = MotherBrainFakeDeathProgram.BackgroundRow2,
            [MotherBrainRoomMutation.BackgroundRow3] = MotherBrainFakeDeathProgram.BackgroundRow3,
            [MotherBrainRoomMutation.BackgroundRow4] = MotherBrainFakeDeathProgram.BackgroundRow4,
            [MotherBrainRoomMutation.BackgroundRow5] = MotherBrainFakeDeathProgram.BackgroundRow5,
            [MotherBrainRoomMutation.BackgroundRow6] = MotherBrainFakeDeathProgram.BackgroundRow6,
            [MotherBrainRoomMutation.BackgroundRow7] = MotherBrainFakeDeathProgram.BackgroundRow7,
            [MotherBrainRoomMutation.BackgroundRow8] = MotherBrainFakeDeathProgram.BackgroundRow8,
            [MotherBrainRoomMutation.BackgroundRow9] = MotherBrainFakeDeathProgram.BackgroundRow9,
            [MotherBrainRoomMutation.BackgroundRowA] = MotherBrainFakeDeathProgram.BackgroundRowA,
            [MotherBrainRoomMutation.BackgroundRowB] = MotherBrainFakeDeathProgram.BackgroundRowB,
            [MotherBrainRoomMutation.BackgroundRowC] = MotherBrainFakeDeathProgram.BackgroundRowC,
            [MotherBrainRoomMutation.BackgroundRowD] = MotherBrainFakeDeathProgram.BackgroundRowD,
            [MotherBrainRoomMutation.ClearCeilingBlock] = MotherBrainFakeDeathProgram.ClearCeilingBlock,
            [MotherBrainRoomMutation.ClearCeilingTube] = MotherBrainFakeDeathProgram.ClearCeilingTube,
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube,
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = MotherBrainFakeDeathProgram.ClearBottomMiddleTubes,
            [MotherBrainRoomMutation.ClearBottomLeftTube] = MotherBrainFakeDeathProgram.ClearBottomLeftTube,
            [MotherBrainRoomMutation.ClearBottomRightTube] = MotherBrainFakeDeathProgram.ClearBottomRightTube,
        };
}
