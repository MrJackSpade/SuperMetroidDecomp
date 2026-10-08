using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge translation of PLM $84:B964, the old Tourian escape shaft fake wall.</summary>
public sealed partial class RoomPlmSystem
{
    private Action? _spawnOldTourianEscapeShaftWallExplosion;

    private void ResetOldTourianEscapeShaftWallState() =>
        _spawnOldTourianEscapeShaftWallExplosion = null;

    /// <summary>
    /// Runs room setup $8F:91A9: Spawn_Hardcoded_PLM at block ($10,$87), whose setup
    /// $84:B3C1 clears the block's collision bits before the list starts on timer one.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        ArgumentNullException.ThrowIfNull(level);
        int blockIndex = level.GetBlockIndex(
            OldTourianEscapeShaftWallPlmDefinitions.BlockX,
            OldTourianEscapeShaftWallPlmDefinitions.BlockY);
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.HeaderPointer = RoomPlmHeaders.OldTourianEscapeShaftFakeWall;
            slot.LoopTimer = 0;
            SetupDoorTransitionDeactivatedSlot(level, slot);
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
    private void RunOldTourianEscapeShaftWallPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.OldTourianEscapeShaftFakeWall || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != OldTourianEscapeShaftWallPlmDefinitions.WaitForSamusPreInstruction)
        {
            throw new InvalidDataException(
                $"Old Tourian escape-shaft wall installed unknown pre-instruction $84:{slot.PreInstruction:X4}.");
        }

        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException("Old Tourian escape-shaft wall has no live Samus owner.");
        if (OldTourianEscapeShaftWallPlmDefinitions.WakeTargetX >= samus.XPosition ||
            OldTourianEscapeShaftWallPlmDefinitions.WakeTargetY >= samus.YPosition)
            return;

        // $84:B90B-$B914 advances past the sleep and arms timer one; unlike other wakes it
        // leaves the loop timer alone.
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.InstructionTimer = 1;
        (_spawnOldTourianEscapeShaftWallExplosion ?? throw new InvalidOperationException(
            "Old Tourian escape-shaft wall has no enemy-projectile owner."))();
    }
}
