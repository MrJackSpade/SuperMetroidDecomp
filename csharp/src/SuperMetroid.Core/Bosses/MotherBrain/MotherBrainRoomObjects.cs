using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which her mutations contend for with every other room object.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectPlaceList places)
{
    /// <summary>The reveal of each of the mural's rows, top row first: the row's draw, drawn once.</summary>
    private static readonly TerrainChange[] MuralRowReveals =
        [.. Enumerable.Range(0, MotherBrainMuralDefinitions.RowCount).Select(row => TerrainChange.DrawnOnce(MotherBrainFakeDeathDraws.MuralRow(row)))];

    /// <summary>
    /// The terrain change <paramref name="request"/> makes: a mural row's reveal draws the mural's row that lies at its
    /// block; every other mutation makes its own change wherever it is spawned.
    /// </summary>
    /// <exception cref="InvalidOperationException">A mural row's reveal is requested at a block no row of the mural lies at.</exception>
    internal static TerrainChange Change(RoomObjectSpawnRequest<MotherBrainRoomMutation> request)
    {
        if (request.RoomObject != MotherBrainRoomMutation.RevealMuralRow)
            return Ensure.Listed(Changes, request.RoomObject);
        int row = request.Block.Y - MotherBrainMuralDefinitions.TopBlockRow;
        if (request.Block.X != MotherBrainMuralDefinitions.Column || row < 0 || row >= MotherBrainMuralDefinitions.RowCount)
            throw new InvalidOperationException($"No row of Mother Brain's mural lies at block ({request.Block.X},{request.Block.Y}).");
        return MuralRowReveals[row];
    }

    /// <summary>
    /// Spawns one of Mother Brain's room mutations at its block: a terrain change that draws its layout on its first
    /// update and is gone on the next.
    /// </summary>
    /// <returns>
    /// False when the room is full; nothing is spawned or changed then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<MotherBrainRoomMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Change(request);

        // Each spawn takes the first free place in update order. Do not reserve a boss place or coalesce adjacent row
        // requests: the order of the places they take is their update order.
        if (places.Spawn() is not { } place)
            return false;

        place.Block = level.GridBlock(request.Block);
        place.Become(new TerrainChangeObject(change));
        if (request.RoomObject == MotherBrainRoomMutation.EscapeDoor)
        {
            // The escape door's setup installs the door block and its extensions at once, before the first draw changes
            // their artwork. Each block is written once, so the order of the writes is unobservable.
            foreach (RoomBlockShapeCell cell in EscapeDoorSetup.Cells)
            {
                Position16<RoomBlocks> block = place.Block + cell.Cell;
                if (cell.Collision == RoomCollisionType.DoorBlock)
                    RoomObjectLevelWrites.WriteDoorBlock(level, block, MotherBrainDeathDefinitions.EscapeDoor);
                else
                    RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, block, cell.Collision,
                        EscapeDoorSetupBehaviors[cell.Collision]);
            }
        }
        return true;
    }
}
