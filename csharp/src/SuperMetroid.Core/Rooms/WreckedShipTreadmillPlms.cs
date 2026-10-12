using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Wrecked Ship treadmill and Maridia elevatube PLMs: the conveyor rows they write and step.</summary>
internal sealed class WreckedShipTreadmillPlms
{
    private readonly PlmSlotTable _slotTable;

    internal WreckedShipTreadmillPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>
    /// Spawns header $B64B or $B64F at cartridge block coordinate (4,9), then executes
    /// shared setup $84:B04A synchronously by replacing the following $38 level words with
    /// blank air. The first ordinary PLM pass later either installs treadmill collision
    /// when Phantoon is dead or deletes the actor without doing so.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnWreckedShipEntranceTreadmill(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        WreckedShipTreadmillDirection direction,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(streamer);

        PlmHeaderId header = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillDirection.Leftwards =>
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast,
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Unknown Wrecked Ship treadmill direction."),
        };
        ushort instructionList = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                RoomPlmInstructionLists.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillDirection.Leftwards =>
                RoomPlmInstructionLists.WreckedShipEntranceTreadmillFromEast,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

        int origin = level.GetBlockIndex(
            new(WreckedShipTreadmillPlmRomData.BlockX, WreckedShipTreadmillPlmRomData.BlockY));
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillPlmRomData.BlockCount);

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = origin;
            slot.InstructionPointer = instructionList;
            slot.InstructionTimer = 1;
            slot.Treadmill = new WreckedShipTreadmillPlmState(
                direction,
                areaBossDefeated);

            // Setup $84:B04A writes exact level words and deliberately leaves BTS alone.
            // The streamer mirrors decompressed level memory, so update it at the same
            // synchronous setup seam without inventing an immediate DrawPLM VRAM upload.
            for (int offset = 0; offset < WreckedShipTreadmillPlmRomData.BlockCount; offset++)
            {
                int blockIndex = origin + offset;
                level.SetForegroundEntry(blockIndex, WreckedShipTreadmillPlmRomData.BlankAirWord);
                streamer.SetLevelEntry(blockIndex, WreckedShipTreadmillPlmRomData.BlankAirWord);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns the effectively invisible $B8F9 elevatube PLM at (1,0). Its ordinary ROM
    /// instruction list supplies a sixteen-frame delay, queues library-two sound $15, and
    /// deletes; the shared interpreter already owns each of those operations.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnMaridiaElevatube(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(
            new(MaridiaElevatubePlmRomData.BlockX, MaridiaElevatubePlmRomData.BlockY));

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = PlmHeaderId.MaridiaElevatube;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = RoomPlmInstructionLists.MaridiaElevatube;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>Consumes the native first-pass boss test for one treadmill PLM.</summary>
    internal static bool TryStepWreckedShipTreadmill(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot)
    {
        if (slot.Treadmill is not WreckedShipTreadmillPlmState state)
            return false;

        if (state.AreaBossDefeated)
        {
            EnsureHorizontalRangeFits(
                level,
                slot.BlockIndex,
                WreckedShipTreadmillPlmRomData.BlockCount);
            byte bts = state.Direction == WreckedShipTreadmillDirection.Rightwards
                ? WreckedShipTreadmillPlmRomData.RightwardsBehavior
                : WreckedShipTreadmillPlmRomData.LeftwardsBehavior;

            // Instructions $AD43/$AD58 call the shared raw row writer. The level word's
            // low twelve bits stay $0FF, so the visual block definition is unchanged;
            // only collision type three and direction BTS become active.
            for (int offset = 0; offset < WreckedShipTreadmillPlmRomData.BlockCount; offset++)
            {
                int blockIndex = slot.BlockIndex + offset;
                level.SetForegroundEntry(
                    blockIndex,
                    WreckedShipTreadmillPlmRomData.ActiveTreadmillWord);
                level.SetBehavior(blockIndex, bts);
                streamer.SetLevelEntry(
                    blockIndex,
                    WreckedShipTreadmillPlmRomData.ActiveTreadmillWord);
            }
        }

        // Both branches terminate in $84:86BC during this same handler pass.
        slot.Active = false;
        slot.Treadmill = null;
        return true;
    }

    private static void EnsureHorizontalRangeFits(
        RoomLevelData level,
        int origin,
        int count)
    {
        int originX = origin % level.Size.X;
        int originY = origin / level.Size.X;
        if (originX + count <= level.Size.X)
            return;
        throw new InvalidDataException(
            $"Wrecked Ship treadmill row ({originX},{originY}).." +
            $"({originX + count - 1},{originY}) exceeds a {level.Size.X}-block room row.");
    }
}
