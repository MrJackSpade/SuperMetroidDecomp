using SuperMetroid.Core.Rooms.LevelData;
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

    /// <summary>
    /// The blocks the escape door's setup installs as it spawns, before its first draw changes their artwork: the door
    /// block at the origin and the three vertical extensions below it.
    /// </summary>
    internal static readonly RoomBlockShape EscapeDoorSetup = new(
    [
        new(new(0, 0), RoomCollisionType.DoorBlock),
        new(new(0, 1), RoomCollisionType.VerticalExtension),
        new(new(0, 2), RoomCollisionType.VerticalExtension),
        new(new(0, 3), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>
    /// How each block of <see cref="EscapeDoorSetup"/> behaves, by how it collides: the door block leads through the
    /// escape door, and each extension links to the block above it.
    /// </summary>
    internal static readonly Dictionary<RoomCollisionType, RoomBlockBehavior> EscapeDoorSetupBehaviors = new()
    {
        [RoomCollisionType.DoorBlock] = MotherBrainDeathDefinitions.EscapeDoorBehavior,
        [RoomCollisionType.VerticalExtension] = MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior,
    };

    /// <summary>The change that draws <paramref name="layout"/> once.</summary>
    private static TerrainChange Drawn(MotherBrainFakeDeathDraw layout) => TerrainChange.DrawnOnce(RoomObjectDraw.Of(layout));
}
