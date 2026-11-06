using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room PLM and program of each of Mother Brain's room mutations.</summary>
internal static class MotherBrainRoomMutationDefinitions
{
    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<MotherBrainRoomMutation, PlmHeaderId> Plms =
        new Dictionary<MotherBrainRoomMutation, PlmHeaderId>
        {
            [MotherBrainRoomMutation.FillWall] = PlmHeaderId.FillMotherBrainsWall,
            [MotherBrainRoomMutation.EscapeDoor] = PlmHeaderId.MotherBrainsRoomEscapeDoor,
            [MotherBrainRoomMutation.BackgroundRow2] = PlmHeaderId.MotherBrainsBackgroundRow2,
            [MotherBrainRoomMutation.BackgroundRow3] = PlmHeaderId.MotherBrainsBackgroundRow3,
            [MotherBrainRoomMutation.BackgroundRow4] = PlmHeaderId.MotherBrainsBackgroundRow4,
            [MotherBrainRoomMutation.BackgroundRow5] = PlmHeaderId.MotherBrainsBackgroundRow5,
            [MotherBrainRoomMutation.BackgroundRow6] = PlmHeaderId.MotherBrainsBackgroundRow6,
            [MotherBrainRoomMutation.BackgroundRow7] = PlmHeaderId.MotherBrainsBackgroundRow7,
            [MotherBrainRoomMutation.BackgroundRow8] = PlmHeaderId.MotherBrainsBackgroundRow8,
            [MotherBrainRoomMutation.BackgroundRow9] = PlmHeaderId.MotherBrainsBackgroundRow9,
            [MotherBrainRoomMutation.BackgroundRowA] = PlmHeaderId.MotherBrainsBackgroundRowA,
            [MotherBrainRoomMutation.BackgroundRowB] = PlmHeaderId.MotherBrainsBackgroundRowB,
            [MotherBrainRoomMutation.BackgroundRowC] = PlmHeaderId.MotherBrainsBackgroundRowC,
            [MotherBrainRoomMutation.BackgroundRowD] = PlmHeaderId.MotherBrainsBackgroundRowD,
            [MotherBrainRoomMutation.ClearCeilingBlock] = PlmHeaderId.ClearMotherBrainCeilingBlock,
            [MotherBrainRoomMutation.ClearCeilingTube] = PlmHeaderId.ClearMotherBrainCeilingTube,
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = PlmHeaderId.ClearMotherBrainBottomMiddleSideTube,
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = PlmHeaderId.ClearMotherBrainBottomMiddleTubes,
            [MotherBrainRoomMutation.ClearBottomLeftTube] = PlmHeaderId.ClearMotherBrainBottomLeftTube,
            [MotherBrainRoomMutation.ClearBottomRightTube] = PlmHeaderId.ClearMotherBrainBottomRightTube,
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
