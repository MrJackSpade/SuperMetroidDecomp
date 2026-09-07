using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    // These are wiring, not emulated state. Runtime binds them before collision and on
    // room initialization, including the first frame after restoring an older state.
    [NonSerialized] private RoomEnemySystem? _chozoEnemies;
    [NonSerialized] private RoomLayer3FxState? _chozoRoomFx;

    /// <summary>Connects the bank-$84 hand setup to the existing bank-$AA actor owner.</summary>
    public void BindChozoStatueContext(RoomEnemySystem enemies, RoomLayer3FxState roomFx)
    {
        _chozoEnemies = enemies;
        _chozoRoomFx = roomFx;
    }

    /// <summary>Runs the native one-shot special-block setup; the caller still clips to solid.</summary>
    public void NotifyChozoStatueHandCollision(ISnesAddressSpace bus, RoomLevelData level,
        RoomCollisionBlock block, SamusState samus, byte collisionPose, bool movingDown)
    {
        bool wreckedShip = _activeAreaIndex == AreaId.WreckedShip &&
            block.Bts == ChozoStatuePlmRomData.WreckedShipHandBts;
        bool lowerNorfair = _activeAreaIndex == AreaId.Norfair &&
            block.Bts == ChozoStatuePlmRomData.LowerNorfairHandBts;
        if (!wreckedShip && !lowerNorfair) return;
        // Both native setups return SEC regardless of admission. The morph-left pose
        // is deliberately absent: the cartridge explicitly lists these three poses.
        bool eligible = movingDown && collisionPose is
            (SamusPoseIds.MorphBallGroundRightPose or
             SamusPoseIds.SpringBallGroundRightPose or SamusPoseIds.SpringBallGroundLeftPose);
        eligible &= wreckedShip
            ? (_coloredDoorSystem ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
                .HasAnyBossBits(_activeAreaIndex, BossBits.AreaBoss)
            : (samus.CollectedItems & (ushort)SamusEquipmentFlags.SpaceJump) != 0;
        for (int i = _slots.Length - 1; i >= 0; i--)
        {
            PlmSlot reaction = _slots[i];
            if (reaction.Active) continue;
            ClearSlot(reaction);
            reaction.Active = true;
            reaction.HeaderPointer = wreckedShip ? ChozoStatuePlmRomData.WreckedShipTrigger
                : ChozoStatuePlmRomData.LowerNorfairTrigger;
            reaction.BlockIndex = block.Index;
            if (eligible)
            {
                RoomEnemySystem enemies = _chozoEnemies ??
                    throw new InvalidOperationException("Chozo hand has no bound enemy owner.");
                enemies.ActivateChozoStatueHandTrigger(level, block.Index);
                // The nested hardcoded spawn occurs while the reaction slot is occupied.
                enemies.ApplyPendingChozoStatuePlms(bus, level, this);
            }
            ClearSlot(reaction);
            return;
        }
    }

    /// <summary>Allocates the native descending PLM slot, runs setup, then leaves its ROM list live.</summary>
    public bool TrySpawnChozoStatuePlm(ISnesAddressSpace bus, RoomLevelData level,
        ChozoStatuePlmRequest request)
    {
        if (request.HeaderPointer is not (ChozoStatuePlmRomData.WreckedShipHand or
            ChozoStatuePlmRomData.ClearSlopeAccess or ChozoStatuePlmRomData.BlockSlopeAccess or
            ChozoStatuePlmRomData.LowerNorfairHand or ChozoStatuePlmRomData.CrumblePlug))
            throw new InvalidDataException($"Unsupported Chozo PLM ${request.HeaderPointer:X4}.");
        int index = level.GetBlockIndex(request.BlockX, request.BlockY);
        for (int i = _slots.Length - 1; i >= 0; i--)
        {
            PlmSlot slot = _slots[i];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = request.HeaderPointer;
            slot.BlockIndex = index;
            slot.InstructionPointer = ReadBank84Word(bus, (ushort)(request.HeaderPointer + 2));
            slot.InstructionTimer = 1;
            if (request.HeaderPointer == ChozoStatuePlmRomData.WreckedShipHand)
                WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
                    ChozoStatuePlmRomData.WreckedShipHandBts.Value);
            else if (request.HeaderPointer == ChozoStatuePlmRomData.CrumblePlug)
            {
                // Native D108 omits LDA: the two spawn entry points leave different A
                // values, so preserve that quirk rather than reading the previous tile.
                level.SetForegroundEntry(index, request.IsHardcoded
                    ? ChozoStatuePlmRomData.HardcodedCrumbleSetupLevelWord : (ushort)0);
            }
            return true;
        }
        return false;
    }

    private static void WriteChozoBlock(RoomLevelData level, int index,
        RoomCollisionType type, byte bts)
    {
        RoomLevelWord word = level.GetCollisionBlockByIndex(index).PackedWord;
        level.SetForegroundEntry(index, word.WithCollisionType(type).Raw);
        level.SetBehavior(index, bts);
    }

    private bool TryExecuteChozoStatueInstruction(RoomLevelData level, PlmSlot slot, ushort instruction)
    {
        switch (instruction)
        {
            case ChozoStatuePlmRomData.TransformSpikesToSlopes:
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex,
                    RoomCollisionType.Slope, ChozoStatuePlmRomData.FirstSlopeBts);
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex + 1,
                    RoomCollisionType.Slope, ChozoStatuePlmRomData.SecondSlopeBts);
                break;
            case ChozoStatuePlmRomData.RevertSlopesToSpikes:
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex,
                    RoomCollisionType.SpikeBlock, 0);
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex + 1,
                    RoomCollisionType.SpikeBlock, 0);
                break;
            case ChozoStatuePlmRomData.SetLoweredAcidHeight:
                (_chozoRoomFx ?? throw new InvalidOperationException("Chozo PLM has no FX owner."))
                    .ApplyCartridgeMotionWrites(baseYPosition: ChozoStatuePlmRomData.LoweredAcidY);
                break;
            default: return false;
        }
        slot.InstructionPointer += 2;
        return true;
    }

    private static void RunChozoStatuePreInstruction(RoomLevelData level, PlmSlot slot)
    {
        if (slot.PreInstruction != ChozoStatuePlmRomData.WaitForLowerNorfairHand) return;
        int index = level.GetBlockIndex(ChozoStatuePlmRomData.LowerNorfairTriggerX,
            ChozoStatuePlmRomData.LowerNorfairTriggerY);
        if (level.GetCollisionBlockByIndex(index).LevelWord != ChozoStatuePlmRomData.BlankAirLevelWord) return;
        WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
            ChozoStatuePlmRomData.LowerNorfairHandBts.Value);
        slot.Active = false;
    }
}
