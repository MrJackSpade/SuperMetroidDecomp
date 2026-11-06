using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Bosses.Botwoon.BotwoonWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Spawns Botwoon's wall PLM in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the wall contends for with every other room PLM.</param>
internal sealed class BotwoonWallPlms(PlmSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall PLM performing <paramref name="mutation"/> at the wall's top block, in the first free slot of the
    /// room's processing order.
    /// </summary>
    /// <remarks>
    /// The clear has an empty setup and clears an already-defeated room on the next handler pass. The crumble's setup
    /// delays its program for exactly 64 PLM frames; the program then moves one block downward after each four-frame,
    /// four-image row and repeats nine times using its frame hold.
    /// </remarks>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned then.</returns>
    public bool TrySpawn(RoomLevelData level, BotwoonWallMutation mutation)
    {
        Ensure.NotNull(level);
        PlmHeaderId plm = Ensure.Listed(Plms, mutation);
        BotwoonWallProgram program = Ensure.Listed(Programs, mutation);
        int firstStepDelay = Ensure.Listed(FirstStepDelays, mutation);

        // Resolve the block through the level so a malformed fixture cannot install a wall whose later vertical draws
        // fall outside the room.
        int blockIndex = level.GetBlockIndex(Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        // The slot keeps the wall's PLM identity for owner queries and mixed populations, not just its program.
        slot.Header = plm;
        slot.LoopTimer = WrappingCountdown.Start(0);
        slot.StartAnimation(new BotwoonWallPlmAnimation(program));
        slot.FrameHold = WrappingCountdown.FromRaw(firstStepDelay);
        return true;
    }
}
