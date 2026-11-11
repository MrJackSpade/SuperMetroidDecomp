using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The terrain change of each of Mother Brain's room mutations.</summary>
internal static class MotherBrainRoomMutationDefinitions
{
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
