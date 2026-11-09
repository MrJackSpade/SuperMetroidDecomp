using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room object and terrain change of each of Mother Brain's room mutations.</summary>
internal static class MotherBrainRoomMutationDefinitions
{
    /// <summary>The room object each mutation is.</summary>
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

    /// <summary>The change each mutation makes: its layout, drawn on the mutation's first update; it is gone on the next.</summary>
    internal static readonly IReadOnlyDictionary<MotherBrainRoomMutation, TerrainChange> Changes =
        new Dictionary<MotherBrainRoomMutation, TerrainChange>
        {
            [MotherBrainRoomMutation.FillWall] = Drawn(MotherBrainFakeDeathDraw.FillWall),
            [MotherBrainRoomMutation.EscapeDoor] = Drawn(MotherBrainFakeDeathDraw.EscapeDoor),
            [MotherBrainRoomMutation.MuralRow1] = Drawn(MotherBrainFakeDeathDraw.MuralRow1),
            [MotherBrainRoomMutation.MuralRow2] = Drawn(MotherBrainFakeDeathDraw.MuralRow2),
            [MotherBrainRoomMutation.MuralRow3] = Drawn(MotherBrainFakeDeathDraw.MuralRow3),
            [MotherBrainRoomMutation.MuralRow4] = Drawn(MotherBrainFakeDeathDraw.MuralRow4),
            [MotherBrainRoomMutation.MuralRow5] = Drawn(MotherBrainFakeDeathDraw.MuralRow5),
            [MotherBrainRoomMutation.MuralRow6] = Drawn(MotherBrainFakeDeathDraw.MuralRow6),
            [MotherBrainRoomMutation.MuralRow7] = Drawn(MotherBrainFakeDeathDraw.MuralRow7),
            [MotherBrainRoomMutation.MuralRow8] = Drawn(MotherBrainFakeDeathDraw.MuralRow8),
            [MotherBrainRoomMutation.MuralRow9] = Drawn(MotherBrainFakeDeathDraw.MuralRow9),
            [MotherBrainRoomMutation.MuralRow10] = Drawn(MotherBrainFakeDeathDraw.MuralRow10),
            [MotherBrainRoomMutation.MuralRow11] = Drawn(MotherBrainFakeDeathDraw.MuralRow11),
            [MotherBrainRoomMutation.MuralRow12] = Drawn(MotherBrainFakeDeathDraw.MuralRow12),
            [MotherBrainRoomMutation.ClearCeilingBlock] = Drawn(MotherBrainFakeDeathDraw.ClearCeilingBlock),
            [MotherBrainRoomMutation.ClearCeilingTube] = Drawn(MotherBrainFakeDeathDraw.ClearCeilingTube),
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube),
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = Drawn(MotherBrainFakeDeathDraw.ClearBottomMiddleTubes),
            [MotherBrainRoomMutation.ClearBottomLeftTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomLeftTube),
            [MotherBrainRoomMutation.ClearBottomRightTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomRightTube),
        };

    /// <summary>The change that draws <paramref name="layout"/> once.</summary>
    private static TerrainChange Drawn(MotherBrainFakeDeathDraw layout) => TerrainChange.DrawnOnce(RoomObjectDraw.Of(layout));
}
