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
            [MotherBrainRoomMutation.MuralRow1] = RoomObjectHeaderId.MotherBrainMuralRow1,
            [MotherBrainRoomMutation.MuralRow2] = RoomObjectHeaderId.MotherBrainMuralRow2,
            [MotherBrainRoomMutation.MuralRow3] = RoomObjectHeaderId.MotherBrainMuralRow3,
            [MotherBrainRoomMutation.MuralRow4] = RoomObjectHeaderId.MotherBrainMuralRow4,
            [MotherBrainRoomMutation.MuralRow5] = RoomObjectHeaderId.MotherBrainMuralRow5,
            [MotherBrainRoomMutation.MuralRow6] = RoomObjectHeaderId.MotherBrainMuralRow6,
            [MotherBrainRoomMutation.MuralRow7] = RoomObjectHeaderId.MotherBrainMuralRow7,
            [MotherBrainRoomMutation.MuralRow8] = RoomObjectHeaderId.MotherBrainMuralRow8,
            [MotherBrainRoomMutation.MuralRow9] = RoomObjectHeaderId.MotherBrainMuralRow9,
            [MotherBrainRoomMutation.MuralRow10] = RoomObjectHeaderId.MotherBrainMuralRow10,
            [MotherBrainRoomMutation.MuralRow11] = RoomObjectHeaderId.MotherBrainMuralRow11,
            [MotherBrainRoomMutation.MuralRow12] = RoomObjectHeaderId.MotherBrainMuralRow12,
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
            [MotherBrainRoomMutation.MuralRow1] = MotherBrainFakeDeathProgram.MuralRow1,
            [MotherBrainRoomMutation.MuralRow2] = MotherBrainFakeDeathProgram.MuralRow2,
            [MotherBrainRoomMutation.MuralRow3] = MotherBrainFakeDeathProgram.MuralRow3,
            [MotherBrainRoomMutation.MuralRow4] = MotherBrainFakeDeathProgram.MuralRow4,
            [MotherBrainRoomMutation.MuralRow5] = MotherBrainFakeDeathProgram.MuralRow5,
            [MotherBrainRoomMutation.MuralRow6] = MotherBrainFakeDeathProgram.MuralRow6,
            [MotherBrainRoomMutation.MuralRow7] = MotherBrainFakeDeathProgram.MuralRow7,
            [MotherBrainRoomMutation.MuralRow8] = MotherBrainFakeDeathProgram.MuralRow8,
            [MotherBrainRoomMutation.MuralRow9] = MotherBrainFakeDeathProgram.MuralRow9,
            [MotherBrainRoomMutation.MuralRow10] = MotherBrainFakeDeathProgram.MuralRow10,
            [MotherBrainRoomMutation.MuralRow11] = MotherBrainFakeDeathProgram.MuralRow11,
            [MotherBrainRoomMutation.MuralRow12] = MotherBrainFakeDeathProgram.MuralRow12,
            [MotherBrainRoomMutation.ClearCeilingBlock] = MotherBrainFakeDeathProgram.ClearCeilingBlock,
            [MotherBrainRoomMutation.ClearCeilingTube] = MotherBrainFakeDeathProgram.ClearCeilingTube,
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube,
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = MotherBrainFakeDeathProgram.ClearBottomMiddleTubes,
            [MotherBrainRoomMutation.ClearBottomLeftTube] = MotherBrainFakeDeathProgram.ClearBottomLeftTube,
            [MotherBrainRoomMutation.ClearBottomRightTube] = MotherBrainFakeDeathProgram.ClearBottomRightTube,
        };
}
