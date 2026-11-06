using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Bosses.MotherBrain.ShitroidWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns the Shitroid's invisible-wall PLM in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the wall contends for with every other room PLM.</param>
internal sealed class ShitroidWallPlms(PlmSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall PLM performing the request's mutation at its block. The setup runs as the PLM is spawned: it
    /// gives each block of the wall column the mutation's collision, keeping every block's appearance, and the PLM
    /// deletes itself on its first pass.
    /// </summary>
    /// <returns>False when every PLM slot is occupied; no terrain is changed then.</returns>
    public bool TrySpawn(RoomLevelData level, PlmSpawnRequest<ShitroidWallMutation> request)
    {
        Ensure.NotNull(level);
        PlmHeaderId plm = Ensure.Listed(Plms, request.Plm);
        RoomCollisionType collision = Ensure.Listed(Collisions, request.Plm);

        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = plm;
        slot.BlockIndex = level.GetBlockIndex(request.Block);
        slot.LoopTimer = WrappingCountdown.Start(0);
        slot.FrameBehavior = PlmFrameBehaviors.Inert;
        slot.Argument = null;
        PlmProgramStart.Delete.StartOn(slot);
        slot.FrameHold = WrappingCountdown.Start(1);

        // The setup walks down the wall column. The change is part of setup, so it happens before the PLM handler's
        // timer can consume and delete this slot.
        for (int rowOffset = 0; rowOffset < WallHeight; rowOffset++)
        {
            int blockIndex = level.GetBlockIndex(request.Block + new Offset16<RoomBlocks>(0, rowOffset));
            level.SetTerrain(blockIndex, level.GetCollisionBlockByIndex(blockIndex).Terrain.WithCollision(collision));
        }
        return true;
    }
}
