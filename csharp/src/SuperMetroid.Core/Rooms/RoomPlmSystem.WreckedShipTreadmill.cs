using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Hardcoded bank-$84 PLMs spawned by Wrecked Ship entrance door setup.</summary>
public sealed partial class RoomPlmSystem
{
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
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);

        ushort header = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                RoomPlmHeaders.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillDirection.Leftwards =>
                RoomPlmHeaders.WreckedShipEntranceTreadmillFromEast,
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
            WreckedShipTreadmillPlmRomData.BlockX,
            WreckedShipTreadmillPlmRomData.BlockY);
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillPlmRomData.BlockCount);

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
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
        ArgumentNullException.ThrowIfNull(level);
        int blockIndex = level.GetBlockIndex(
            MaridiaElevatubePlmRomData.BlockX,
            MaridiaElevatubePlmRomData.BlockY);

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = RoomPlmHeaders.MaridiaElevatube;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = RoomPlmInstructionLists.MaridiaElevatube;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }

    /// <summary>Consumes the native first-pass boss test for one treadmill PLM.</summary>
    private bool TryStepWreckedShipTreadmill(
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
        int originX = origin % level.WidthInBlocks;
        int originY = origin / level.WidthInBlocks;
        if (originX + count <= level.WidthInBlocks)
            return;
        throw new InvalidDataException(
            $"Wrecked Ship treadmill row ({originX},{originY}).." +
            $"({originX + count - 1},{originY}) exceeds a {level.WidthInBlocks}-block room row.");
    }
}

/// <summary>Semantic discriminator for the one-frame Wrecked Ship treadmill PLM.</summary>
internal sealed record WreckedShipTreadmillPlmState(
    WreckedShipTreadmillDirection Direction,
    bool AreaBossDefeated);

/// <summary>Cartridge literals owned by setup $84:B04A and instructions $84:AD43/$AD58.</summary>
internal static class WreckedShipTreadmillPlmRomData
{
    /// <summary>Hardcoded PLM X block supplied by both bank-$8F door routines.</summary>
    public const byte BlockX = 4;

    /// <summary>Hardcoded PLM Y block supplied by both bank-$8F door routines.</summary>
    public const byte BlockY = 9;

    /// <summary>Number of consecutive row entries written by setup and activation.</summary>
    public const int BlockCount = 0x38;

    /// <summary>Exact blank-air word written synchronously by setup $84:B04A.</summary>
    public const ushort BlankAirWord = 0x00ff;

    /// <summary>Exact type-three treadmill word written after the Phantoon boss-bit test.</summary>
    public const ushort ActiveTreadmillWord = 0x30ff;

    /// <summary>Rightward treadmill BTS written by instruction $84:AD43.</summary>
    public const byte RightwardsBehavior = 0x08;

    /// <summary>Leftward treadmill BTS written by instruction $84:AD58.</summary>
    public const byte LeftwardsBehavior = 0x09;
}

/// <summary>Hardcoded coordinates supplied to elevatube PLM header $84:B8F9.</summary>
internal static class MaridiaElevatubePlmRomData
{
    public const byte BlockX = 1;
    public const byte BlockY = 0;
}
