using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Boss-room terrain PLMs: Botwoon's wall, the Spore Spawn ceiling, and the Kraid, Crocomire, baby Metroid wall and Mother Brain room mutations.</summary>
internal sealed class BossRoomMutationPlms
{
    /// <summary>The shared native PLM allocation table used by each boss-room hardcoded spawn.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Uses the room's PLM table so boss mutations contend with ordinary room PLMs for the same forty slots.</summary>
    internal BossRoomMutationPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>
    /// Spawns Botwoon's hardcoded wall PLM at room block (15,4), preserving the header's
    /// setup routine and the retail descending 40-slot allocation order.
    /// </summary>
    /// <remarks>
    /// Header <c>$B797</c> has an RTS setup and clears an already-defeated room on the next
    /// handler pass. Header <c>$B79B</c> runs setup <c>$84:AB28</c>, which delays its crumble
    /// list for exactly 64 PLM frames. The live list itself moves one block downward after
    /// each four-frame, four-image row and repeats nine times using PLM_Timers.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnBotwoonWall(RoomLevelData level, PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
            PlmHeaderId.ClearBotwoonWall or
            PlmHeaderId.CrumbleBotwoonWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Botwoon wall header must be $B797 (clear) or $B79B (crumble).");
        }

        // Both `$B3:9590` and `$B3:9ADD` pass the literal hardcoded coordinates (15,4).
        // Validate through RoomLevelData rather than allowing a malformed fixture to install
        // a PLM whose later vertical draws would address unrelated host memory.
        int blockIndex = level.GetBlockIndex(new(15, 4));
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            // SpawnHardcodedPlm stores the header before setup. Keep identity intact
            // for owner queries and mixed populations, not just instruction dispatch.
            slot.HeaderPointer = header;
            slot.LoopTimer = 0;
            slot.StartAnimation(new BotwoonWallPlmAnimation(header == PlmHeaderId.ClearBotwoonWall
                ? BotwoonWallProgram.Clear
                : BotwoonWallProgram.Crumble));

            // SpawnHardcodedPLM initializes a new slot's instruction timer to one. Only the
            // live crumble header replaces it: setup `$84:AB28` writes $0040 to the separate
            // PLM instruction-timer allocation before returning to the enemy initializer.
            slot.InstructionTimer = NativeCountdown.FromRaw(header == PlmHeaderId.CrumbleBotwoonWall
                ? (ushort)64
                : (ushort)1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns Spore Spawn's hardcoded 2x2 ceiling mutation at room block (7,30).
    /// Header $B78F animates three crumble frames and then falls through to $AB21's clear;
    /// header $B793 starts directly at that clear list for an already-defeated room.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnSporeSpawnCeiling(RoomLevelData level, PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Spore Spawn ceiling header must be $B78F (crumble) or $B793 (clear).");
        }

        int blockIndex = level.GetBlockIndex(new(7, 30));
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.InstructionPointer = header == PlmHeaderId.CrumbleSporeSpawnCeiling
                ? RoomPlmInstructionLists.CrumbleSporeSpawnCeiling
                : RoomPlmInstructionLists.ClearSporeSpawnCeiling;
            slot.InstructionTimer = NativeCountdown.Start(1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns one hardcoded Kraid ceiling/platform mutation and runs setup $84:B3C1.
    /// Setup immediately deactivates the collision type while preserving priority and the
    /// visual block index; the selected ROM list then draws its three crumble frames and
    /// deletes the finite PLM on the authored cadence.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnKraidRoomMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        KraidRoomProgram program = header switch
        {
            PlmHeaderId.CrumbleKraidCeilingIntoBackground1 => KraidRoomProgram.CrumbleCeilingIntoBackground1,
            PlmHeaderId.CrumbleKraidPlatformVariant1 => KraidRoomProgram.SetCeilingToBackground2,
            PlmHeaderId.CrumbleKraidCeilingIntoBackground2 => KraidRoomProgram.CrumbleCeilingIntoBackground2,
            PlmHeaderId.CrumbleKraidPlatformVariant2 => KraidRoomProgram.SetCeilingToBackground3,
            PlmHeaderId.CrumbleKraidCeilingIntoBackground3 => KraidRoomProgram.CrumbleCeilingIntoBackground3,
            PlmHeaderId.ClearKraidCeiling => KraidRoomProgram.ClearCeiling,
            PlmHeaderId.ClearKraidSpikes => KraidRoomProgram.ClearSpikes,
            PlmHeaderId.CrumbleKraidSpikes => KraidRoomProgram.CrumbleSpikes,
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Kraid room mutation header is outside the authored Kraid mutation set."),
        };

        int blockIndex = level.GetBlockIndex(block);
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = blockIndex;
            slot.StartAnimation(new KraidRoomPlmAnimation(program));
            slot.InstructionTimer = NativeCountdown.Start(1);

            // PlmSetup_DeactivatePlm clears level-word bits 12..14, not all four collision
            // bits. Bit 15 is the independent tile priority flag and survives setup.
            ushort authored = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, unchecked((ushort)(authored & 0x8fff)));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns one of Crocomire's five hardcoded arena PLMs at its literal room-block origin.
    /// </summary>
    /// <remarks>
    /// The instruction pointers are the words selected by entries $84:B747-$B757 after
    /// their no-op setup. The shared instruction interpreter below already understands the
    /// positive timer/draw pair and $86BC deletion used by every one of these lists.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnCrocomireArenaMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        CrocomireArenaProgram program = header switch
        {
            PlmHeaderId.ClearCrocomireBridge => CrocomireArenaProgram.ClearBridge,
            PlmHeaderId.CrumbleCrocomireBridgeBlock => CrocomireArenaProgram.CrumbleBridgeBlock,
            PlmHeaderId.ClearCrocomireBridgeBlock => CrocomireArenaProgram.ClearBridgeBlock,
            PlmHeaderId.ClearCrocomireInvisibleWall => CrocomireArenaProgram.ClearInvisibleWall,
            PlmHeaderId.CreateCrocomireInvisibleWall => CrocomireArenaProgram.CreateInvisibleWall,
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Crocomire arena header must be $B747, $B74B, $B74F, $B753, or $B757."),
        };

        int blockIndex = level.GetBlockIndex(block);
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.StartAnimation(new CrocomireArenaPlmAnimation(program));
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.HeaderPointer = header;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns Shitroid's hardcoded ten-block vertical wall mutation at `$84:B763/$B767`.
    /// Setup executes synchronously inside <c>SpawnHardcodedPLM</c>: `$B763` clears the
    /// collision nibble and `$B767` replaces it with type eight while preserving the low
    /// twelve bits of every authored level word.
    /// </summary>
    /// <returns>False when all forty native PLM slots are occupied; no terrain is changed.</returns>
    public bool TrySpawnShitroidWallMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
            PlmHeaderId.ClearBabyMetroidInvisibleWall or
            PlmHeaderId.CreateBabyMetroidInvisibleWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Shitroid wall header must be $B763 or $B767.");
        }

        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = level.GetBlockIndex(block);
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            PlmProgramStart.Delete.StartOn(slot);
            slot.InstructionTimer = NativeCountdown.Start(1);

            // Both setup routines walk downward through ten blocks in the same column.
            // The mutation is part of setup, so it must occur before the PLM handler's
            // instruction timer is allowed to consume and delete this allocated slot.
            for (int rowOffset = 0; rowOffset < 10; rowOffset++)
            {
                int blockIndex = level.GetBlockIndex(block + new Offset16<RoomBlocks>(0, checked((short)rowOffset)));
                ushort current = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
                ushort replacement = header == PlmHeaderId.CreateBabyMetroidInvisibleWall
                    ? unchecked((ushort)((current & 0x0fff) | 0x8000))
                    : unchecked((ushort)(current & 0x0fff));
                level.SetForegroundEntry(blockIndex, replacement);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Allocates one of Mother Brain's literal fake-death room mutations from
    /// <c>$84:B673-$84:B6C7</c>. Every listed header uses setup $B3D0 (deactivate) and a
    /// one-frame draw/delete list, so the shared PLM interpreter—not boss-specific terrain
    /// painting—owns the actual room edit and tilemap upload.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnMotherBrainMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        MotherBrainFakeDeathProgram program = header switch
        {
            PlmHeaderId.FillMotherBrainsWall => MotherBrainFakeDeathProgram.FillWall,
            PlmHeaderId.MotherBrainsRoomEscapeDoor => MotherBrainFakeDeathProgram.EscapeDoor,
            PlmHeaderId.MotherBrainsBackgroundRow2 => MotherBrainFakeDeathProgram.BackgroundRow2,
            PlmHeaderId.MotherBrainsBackgroundRow3 => MotherBrainFakeDeathProgram.BackgroundRow3,
            PlmHeaderId.MotherBrainsBackgroundRow4 => MotherBrainFakeDeathProgram.BackgroundRow4,
            PlmHeaderId.MotherBrainsBackgroundRow5 => MotherBrainFakeDeathProgram.BackgroundRow5,
            PlmHeaderId.MotherBrainsBackgroundRow6 => MotherBrainFakeDeathProgram.BackgroundRow6,
            PlmHeaderId.MotherBrainsBackgroundRow7 => MotherBrainFakeDeathProgram.BackgroundRow7,
            PlmHeaderId.MotherBrainsBackgroundRow8 => MotherBrainFakeDeathProgram.BackgroundRow8,
            PlmHeaderId.MotherBrainsBackgroundRow9 => MotherBrainFakeDeathProgram.BackgroundRow9,
            PlmHeaderId.MotherBrainsBackgroundRowA => MotherBrainFakeDeathProgram.BackgroundRowA,
            PlmHeaderId.MotherBrainsBackgroundRowB => MotherBrainFakeDeathProgram.BackgroundRowB,
            PlmHeaderId.MotherBrainsBackgroundRowC => MotherBrainFakeDeathProgram.BackgroundRowC,
            PlmHeaderId.MotherBrainsBackgroundRowD => MotherBrainFakeDeathProgram.BackgroundRowD,
            PlmHeaderId.ClearMotherBrainCeilingBlock => MotherBrainFakeDeathProgram.ClearCeilingBlock,
            PlmHeaderId.ClearMotherBrainCeilingTube => MotherBrainFakeDeathProgram.ClearCeilingTube,
            PlmHeaderId.ClearMotherBrainBottomMiddleSideTube => MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube,
            PlmHeaderId.ClearMotherBrainBottomMiddleTubes => MotherBrainFakeDeathProgram.ClearBottomMiddleTubes,
            PlmHeaderId.ClearMotherBrainBottomLeftTube => MotherBrainFakeDeathProgram.ClearBottomLeftTube,
            PlmHeaderId.ClearMotherBrainBottomRightTube => MotherBrainFakeDeathProgram.ClearBottomRightTube,
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Mother Brain mutation header is outside $B673-$B6C7's authored set."),
        };

        // SpawnHardcodedPLM probes native slots $4E,$4C,...,$00. Do not reserve a boss
        // slot or coalesce adjacent row requests: their allocation order is observable.
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (slot.Active)
                continue;

            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.BlockIndex = level.GetBlockIndex(block);
            slot.RestoreLevelWord = 0;
            slot.LoopTimer = 0;
            slot.StartAnimation(new MotherBrainFakeDeathPlmAnimation(program));
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.HeaderPointer = header;
            if (header == PlmHeaderId.MotherBrainsRoomEscapeDoor)
            {
                // B5F8 installs the door-list index and three upward-linked extensions
                // synchronously, before the first draw instruction changes their artwork.
                RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, MotherBrainDeathRomData.EscapeDoorCollision);
                for (int row = 1; row < 4; row++)
                    RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex + row * level.Size.X,
                        MotherBrainDeathRomData.EscapeDoorExtension);
            }
            return true;
        }

        return false;
    }
}
