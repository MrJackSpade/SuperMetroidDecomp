using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Tourian PLMs: the access gate, the Metroids-cleared states, and the old Tourian escape-shaft wall.</summary>
internal sealed class TourianPlms
{
    private readonly PlmRoomBindings _bindings;
    private readonly PlmSlotTable _slotTable;

    internal TourianPlms(PlmRoomBindings bindings, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _slotTable = slotTable;
    }
    /// <summary>
    /// Applies setup <c>$84:DB1E</c>: select one bank-$84 pre-instruction through the exact
    /// room-argument table and leave the actor on its permanent sleeping instruction list.
    /// </summary>
    internal static void SetupMetroidsClearedSlot(PlmSlot slot)
    {
        slot.PreInstruction = MetroidsClearedPlmRomData.ResolvePreInstruction(slot.RoomArgument);
        slot.InstructionPointer =
            RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired;
    }

    /// <summary>
    /// Runs the selected pre-instruction before the ordinary timer/list pass, matching
    /// <c>PLM_Handler</c>. Arguments below $12 are deliberate RTS handlers. The four active
    /// handlers compare the complete enemy-death word with the room's required-kill byte and
    /// repeatedly mark their idempotent event once the quota is reached.
    /// </summary>
    internal void RunMetroidsClearedPreInstruction(
        PlmSlot slot,
        ushort enemyDeaths,
        byte enemyDeathQuota)
    {
        if (slot.HeaderPointer != PlmHeaderId.SetMetroidsClearedStatesWhenRequired)
            return;

        ushort expectedPreInstruction =
            MetroidsClearedPlmRomData.ResolvePreInstruction(slot.RoomArgument);
        if (slot.PreInstruction != expectedPreInstruction)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM argument ${slot.RoomArgument:X4} selected " +
                $"pre-instruction $84:{slot.PreInstruction:X4}, expected " +
                $"$84:{expectedPreInstruction:X4}.");
        }

        EventNumber? eventNumber = MetroidsClearedPlmRomData.ResolveEvent(slot.RoomArgument);
        if (eventNumber is null || enemyDeaths < enemyDeathQuota)
            return;

        (_bindings.Events ?? throw new InvalidOperationException(
            "Metroids-cleared PLM reached its kill quota without a room event writer.")).Set(
                eventNumber.Value);
    }

    /// <summary>
    /// Runs room setup $8F:91A9: Spawn_Hardcoded_PLM at block ($10,$87), whose setup
    /// $84:B3C1 clears the block's collision bits before the list starts on timer one.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(
            new(OldTourianEscapeShaftWallPlmDefinitions.BlockX, OldTourianEscapeShaftWallPlmDefinitions.BlockY));
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.HeaderPointer = PlmHeaderId.OldTourianEscapeShaftFakeWall;
            slot.LoopTimer = 0;
            DoorPlms.SetupDoorTransitionDeactivatedSlot(level, slot);
            slot.InstructionPointer = OldTourianEscapeShaftWallPlmDefinitions.InstructionList;
            slot.InstructionTimer = 1;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ports pre-instruction $84:B927. WakePLMIfSamusIsBelowRightOfTarget ($84:B8FD) wakes
    /// the list past its sleep and leaves carry clear, which spawns the explosion; while
    /// Samus is not strictly below and right of the target it returns carry set and nothing
    /// happens.
    /// </summary>
    internal void RunOldTourianEscapeShaftWallPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.OldTourianEscapeShaftFakeWall || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != OldTourianEscapeShaftWallPlmDefinitions.WaitForSamusPreInstruction)
        {
            throw new InvalidDataException(
                $"Old Tourian escape-shaft wall installed unknown pre-instruction $84:{slot.PreInstruction:X4}.");
        }

        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Old Tourian escape-shaft wall has no live Samus owner.");
        if (OldTourianEscapeShaftWallPlmDefinitions.WakeTargetX >= samus.Position.X.Whole ||
            OldTourianEscapeShaftWallPlmDefinitions.WakeTargetY >= samus.Position.Y.Whole)
            return;

        // $84:B90B-$B914 advances past the sleep and arms timer one; unlike other wakes it
        // leaves the loop timer alone.
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.InstructionTimer = 1;
        (_bindings.Enemies ?? throw new InvalidOperationException(
            "Old Tourian escape-shaft wall has no enemy-projectile owner.")).SpawnOldTourianEscapeShaftWallExplosion();
    }
    /// <summary>$88:DB8A/DC69 spawn the native clear/crumble lists at block (6,12).</summary>
    public bool TrySpawnTourianAccess(RoomLevelData level, bool clear)
    {
        TourianAccessPlmDefinition definition = TourianAccessPlmDefinitions.ForState(clear);
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            var slot = _slotTable._slots[index];
            if (slot.Active) continue;
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = definition.HeaderPointer;
            slot.BlockIndex = level.GetBlockIndex(new(6, 12));
            slot.InstructionPointer = definition.InstructionListPointer;
            slot.InstructionTimer = 1;
            return true;
        }
        return false;
    }
}
