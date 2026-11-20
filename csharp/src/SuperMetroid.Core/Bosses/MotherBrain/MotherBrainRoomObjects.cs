using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which her mutations contend for with every other room object.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns one of Mother Brain's room changes at its block: a terrain change that draws its layout on its first update
    /// and is gone on the next.
    /// </summary>
    public void Spawn(RoomLevelData level, MotherBrainRoomChangeRequest request)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(request);
        TerrainChange change = request.Change;

        // Each spawn takes the first free place in update order. Do not reserve a boss place or coalesce adjacent row
        // requests: the order of the places they take is their update order.
        if (places.Spawn() is not { } place)
            return;

        place.Block = level.GridBlock(request.Block);
        place.Become(new TerrainChangeObject(change));
        if (request is MotherBrainMutationRequest { Mutation: var mutation } && BlockSetups.TryGetValue(mutation, out RoomBlockShape? setup))
        {
            // The escape door's blocks become the door and its extensions at once, keeping their pictures until the
            // first draw changes them (#2496).
            foreach (RoomBlockShapeCell cell in setup.Cells)
                CellSetups[cell.Collision](level, place.Block + cell.Cell);
        }
    }

    /// <summary>The mutations that also set their blocks up, with the blocks they set up: the escape door's.</summary>
    private static readonly IReadOnlyDictionary<MotherBrainRoomMutation, RoomBlockShape> BlockSetups =
        new Dictionary<MotherBrainRoomMutation, RoomBlockShape>
        {
            [MotherBrainRoomMutation.EscapeDoor] = EscapeDoorSetup,
        };

    /// <summary>
    /// How each block of a setup is set up, by how it collides: the door block leads through her escape door, and each
    /// other block takes the behaviour its collision gives it (<see cref="EscapeDoorSetupBehaviors"/>).
    /// </summary>
    private static readonly IReadOnlyDictionary<RoomCollisionType, Action<RoomLevelData, Position16<RoomBlocks>>> CellSetups =
        EscapeDoorSetupBehaviors.ToDictionary(entry => entry.Key, entry => (Action<RoomLevelData, Position16<RoomBlocks>>)((level, block) =>
                level.SetObjectCollisionAndBehavior(block, entry.Key, entry.Value)))
            .Append(KeyValuePair.Create(RoomCollisionType.DoorBlock, (Action<RoomLevelData, Position16<RoomBlocks>>)((level, block) =>
                RoomObjectLevelWrites.WriteDoorBlock(level, block, MotherBrainDeathDefinitions.EscapeDoor))))
            .ToDictionary();
}
