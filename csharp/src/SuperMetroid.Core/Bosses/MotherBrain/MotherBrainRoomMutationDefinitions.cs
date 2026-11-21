using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The terrain change of each of Mother Brain's room mutations.</summary>
internal static class MotherBrainRoomMutationDefinitions
{
    /// <summary>
    /// The change each mutation makes: its layout, drawn on the mutation's first update; it is gone on the next.
    /// </summary>
    internal static readonly IdentityTable<MotherBrainRoomMutation, TerrainChange> Changes = new(new Dictionary<MotherBrainRoomMutation, TerrainChange>
        {
            [MotherBrainRoomMutation.FillWall] = Drawn(MotherBrainFakeDeathDraw.FillWall),
            [MotherBrainRoomMutation.EscapeDoor] = Drawn(MotherBrainFakeDeathDraw.EscapeDoor),
            [MotherBrainRoomMutation.ClearCeilingBlock] = Drawn(MotherBrainFakeDeathDraw.ClearCeilingBlock),
            [MotherBrainRoomMutation.ClearCeilingTube] = Drawn(MotherBrainFakeDeathDraw.ClearCeilingTube),
            [MotherBrainRoomMutation.ClearBottomMiddleSideTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube),
            [MotherBrainRoomMutation.ClearBottomMiddleTubes] = Drawn(MotherBrainFakeDeathDraw.ClearBottomMiddleTubes),
            [MotherBrainRoomMutation.ClearBottomLeftTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomLeftTube),
            [MotherBrainRoomMutation.ClearBottomRightTube] = Drawn(MotherBrainFakeDeathDraw.ClearBottomRightTube),
        });

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
    /// How each block of <see cref="EscapeDoorSetup"/> other than its door block behaves, by how it collides: each extension
    /// links to the block above it. The door block leads through <see cref="MotherBrainDeathDefinitions.EscapeDoor"/>.
    /// </summary>
    internal static readonly Dictionary<RoomCollisionType, RoomBlockBehavior> EscapeDoorSetupBehaviors = new()
    {
        [RoomCollisionType.VerticalExtension] = MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior,
    };

    /// <summary>The change that draws <paramref name="layout"/> once.</summary>
    private static TerrainChange Drawn(MotherBrainFakeDeathDraw layout) => TerrainChange.DrawnOnce(MotherBrainFakeDeathDraws.Family.Of(layout));
}
