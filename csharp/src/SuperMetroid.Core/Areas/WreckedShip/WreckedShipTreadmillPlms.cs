using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>Wrecked Ship treadmill and Maridia elevatube PLMs: the conveyor rows they write and step.</summary>
internal sealed class WreckedShipTreadmillPlms : IPlmFamilySource
{
    /// <summary>Registers the WreckedShipTreadmill family's step.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(PlmFamily.WreckedShipTreadmill, static (slot, context) =>
            TryStepWreckedShipTreadmill(context.Level, slot) ? PlmStepOutcome.Consumed : PlmStepOutcome.Shared);
    }

    /// <summary>Native PLM slot pool used for treadmill and elevatube allocation.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Attaches treadmill and elevatube spawning to the room's PLM pool.</summary>
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
        WreckedShipTreadmillDirection direction,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);

        PlmHeaderId header = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillDirection.Leftwards =>
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast,
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Unknown Wrecked Ship treadmill direction."),
        };

        int origin = level.GetBlockIndex(
            new(WreckedShipTreadmillBlockDefinitions.BlockX, WreckedShipTreadmillBlockDefinitions.BlockY));
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillBlockDefinitions.BlockCount);

        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = header;
        slot.BlockIndex = origin;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Family = new WreckedShipTreadmillPlmState(
            direction,
            areaBossDefeated);

        // Setup $84:B04A writes exact level words and deliberately leaves BTS alone.
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
        {
            int blockIndex = origin + offset;
            level.SetForegroundEntry(blockIndex, WreckedShipTreadmillBlockDefinitions.BlankAirWord);
        }
        return true;
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
        int blockIndex = level.GetBlockIndex(MaridiaElevatubeDefinitions.PlmBlock);

        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = PlmHeaderId.MaridiaElevatube;
        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.MaridiaElevatube));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }

    /// <summary>Consumes the native first-pass boss test for one treadmill PLM.</summary>
    internal static bool TryStepWreckedShipTreadmill(
        RoomLevelData level,
        PlmSlot slot)
    {
        if (slot.Family is not WreckedShipTreadmillPlmState state)
            return false;

        if (state.AreaBossDefeated)
        {
            EnsureHorizontalRangeFits(
                level,
                slot.BlockIndex,
                WreckedShipTreadmillBlockDefinitions.BlockCount);
            bool rightwards = state.Direction == WreckedShipTreadmillDirection.Rightwards;

            // Instructions $AD43/$AD58 call the shared raw row writer. The level word's
            // low twelve bits stay $0FF, so the visual block definition is unchanged;
            // only collision type three and direction BTS become active.
            for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
            {
                int blockIndex = slot.BlockIndex + offset;
                level.SetForegroundEntry(
                    blockIndex,
                    WreckedShipTreadmillBlockDefinitions.ActiveTreadmillWord);
                level.SetBehavior(blockIndex, rightwards
                    ? WreckedShipTreadmillBlockDefinitions.RightwardsBehavior
                    : WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior);
            }
        }

        // Both branches terminate in $84:86BC during this same handler pass.
        slot.Delete();
        slot.Family = null;
        return true;
    }

    /// <summary>Rejects a treadmill row that would wrap into the next room block row.</summary>
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
