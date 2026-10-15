using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo statue hand PLMs ($84:D6EA family): the Wrecked Ship and Lower Norfair hand checks and the hardcoded statue spawns.</summary>
internal sealed class ChozoStatuePlms
{
    /// <summary>Room progression, area, and other services used by statue hand setup.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Descending native PLM allocation scanned for temporary reactions and persistent statue actors.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds Chozo hand behavior to its room services and PLM slot allocation.</summary>
    internal ChozoStatuePlms(PlmRoomBindings bindings, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _slotTable = slotTable;
    }
    /// <summary>Bound enemy owner for statue hand activation; reattached after room load or state restore.</summary>
    [NonSerialized] internal RoomEnemySystem? _chozoEnemies;
    /// <summary>Bound room FX owner for Lower Norfair acid height writes; reattached after restore.</summary>
    [NonSerialized] internal RoomLayer3FxState? _chozoRoomFx;

    /// <summary>Connects the bank-$84 hand setup to the existing bank-$AA actor owner.</summary>
    public void BindChozoStatueContext(RoomEnemySystem enemies, RoomLayer3FxState roomFx)
    {
        _chozoEnemies = enemies;
        _chozoRoomFx = roomFx;
        enemies.RoomFx.BindRoomFx(roomFx);
    }

    /// <summary>Runs the native one-shot special-block setup; the caller still clips to solid.</summary>
    public void NotifyChozoStatueHandCollision(RoomLevelData level,
        RoomCollisionBlock block, SamusState samus, SamusPoseId collisionPose, bool movingDown)
    {
        bool wreckedShip = _bindings.Area == AreaId.WreckedShip &&
            block.Bts == ChozoStatuePlmRomData.WreckedShipHandBts;
        bool lowerNorfair = _bindings.Area == AreaId.Norfair &&
            block.Bts == ChozoStatuePlmRomData.LowerNorfairHandBts;
        if (!wreckedShip && !lowerNorfair) return;
        // Both native setups return SEC regardless of admission. The morph-left pose
        // is deliberately absent: the cartridge explicitly lists these three poses.
        bool eligible = movingDown && collisionPose is
            (SamusPoseId.MorphBallGroundRightPose or
             SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose);
        eligible &= wreckedShip
            ? (_bindings.System ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
                .HasAnyBossBits(_bindings.Area, BossBits.AreaBoss)
            : (samus.CollectedItems & (ushort)SamusEquipmentFlags.SpaceJump) != 0;
        for (int i = _slotTable._slots.Length - 1; i >= 0; i--)
        {
            PlmSlot reaction = _slotTable._slots[i];
            if (reaction.Active) continue;
            PlmPopulationLoader.ClearSlot(reaction);
            reaction.Active = true;
            reaction.HeaderPointer = wreckedShip ? PlmHeaderId.CollisionWreckedShipChozoHandCheck
                : PlmHeaderId.CollisionLowerNorfairChozoHandCheck;
            reaction.BlockIndex = block.Index;
            if (eligible)
            {
                RoomEnemySystem enemies = _chozoEnemies ??
                    throw new InvalidOperationException("Chozo hand has no bound enemy owner.");
                enemies.ChozoStatueBehavior.ActivateChozoStatueHandTrigger(level, block.Index);
                // The nested hardcoded spawn occurs while the reaction slot is occupied.
                enemies.ChozoStatueBehavior.ApplyPendingChozoStatuePlms(level, this);
            }
            PlmPopulationLoader.ClearSlot(reaction);
            return;
        }
    }

    /// <summary>Allocates the native descending PLM slot, runs setup, then leaves its ROM list live.</summary>
    public bool TrySpawnChozoStatuePlm(RoomLevelData level, ChozoStatuePlmRequest request)
    {
        ChozoStatuePlmDefinition definition =
            ChozoStatuePlmDefinitions.Resolve(request.HeaderPointer);
        int index = level.GetBlockIndex(request.Block);
        for (int i = _slotTable._slots.Length - 1; i >= 0; i--)
        {
            PlmSlot slot = _slotTable._slots[i];
            if (slot.Active) continue;
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = definition.HeaderPointer;
            slot.BlockIndex = index;
            definition.Program.StartOn(slot);
            slot.InstructionTimer = NativeCountdown.Start(1);
            if (request.HeaderPointer == PlmHeaderId.WreckedShipChozoHand)
                WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
                    ChozoStatuePlmRomData.WreckedShipHandBts.Value);
            else if (request.HeaderPointer == PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug)
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

    /// <summary>Changes a hand or plug block's collision type and BTS while preserving its visual word.</summary>
    private static void WriteChozoBlock(RoomLevelData level, int index,
        RoomCollisionType type, byte bts)
    {
        RoomLevelWord word = level.GetCollisionBlockByIndex(index).PackedWord;
        level.SetForegroundEntry(index, word.WithCollisionType(type).Raw);
        level.SetBehavior(index, bts);
    }

    /// <summary>Handles the hand's event branch, spike/slope changes, and lowered acid FX instruction.</summary>
    internal bool TryExecuteChozoStatueInstruction(ISnesAddressSpace bus, RoomLevelData level, PlmSlot slot, RoomPlmInstruction instruction)
    {
        // GotoIfEventSet belongs to this handler only for the Lower Norfair hand.
        if (instruction is not (
                RoomPlmInstruction.TransformSpikesToSlopes or
                RoomPlmInstruction.RevertSlopesToSpikes or
                RoomPlmInstruction.SetLoweredAcidHeight) &&
            !(instruction == RoomPlmInstruction.GotoIfEventSet &&
                slot.HeaderPointer == PlmHeaderId.LowerNorfairChozoHand))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.GotoIfEventSet when
                slot.HeaderPointer == PlmHeaderId.LowerNorfairChozoHand:
                ushort eventNumber = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                if (eventNumber != (ushort)EventNumber.LowerNorfairChozoLoweredAcid)
                    throw new InvalidDataException($"Lower Norfair hand referenced unexpected event ${eventNumber:X4}.");
                // The same list initializes the intact hand or restores drained acid on
                // re-entry. Do not bypass this branch by installing collision at spawn.
                slot.InstructionPointer = (_bindings.System ??
                    throw new InvalidOperationException("Chozo hand has no progression owner."))
                    .HasEvent(EventNumber.LowerNorfairChozoLoweredAcid)
                    ? RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(slot.InstructionPointer + 4)))
                    : unchecked((ushort)(slot.InstructionPointer + 6));
                return true;
            case RoomPlmInstruction.TransformSpikesToSlopes:
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex,
                    RoomCollisionType.Slope, ChozoStatuePlmRomData.FirstSlopeBts);
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex + 1,
                    RoomCollisionType.Slope, ChozoStatuePlmRomData.SecondSlopeBts);
                break;
            case RoomPlmInstruction.RevertSlopesToSpikes:
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex,
                    RoomCollisionType.SpikeBlock, 0);
                WriteChozoBlock(level, ChozoStatuePlmRomData.FirstSlopeBlockIndex + 1,
                    RoomCollisionType.SpikeBlock, 0);
                break;
            case RoomPlmInstruction.SetLoweredAcidHeight:
                (_chozoRoomFx ?? throw new InvalidOperationException("Chozo PLM has no FX owner."))
                    .ApplyCartridgeMotionWrites(baseYPosition: ChozoStatuePlmRomData.LoweredAcidY);
                break;
            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
        }
        slot.InstructionPointer += 2;
        return true;
    }

    /// <summary>Restores the Lower Norfair hand collision block once its trigger location becomes air.</summary>
    internal static void RunChozoStatuePreInstruction(RoomLevelData level, PlmSlot slot)
    {
        if (slot.PreInstruction != ChozoStatuePlmRomData.WaitForLowerNorfairHand) return;
        int index = level.GetBlockIndex(new(ChozoStatuePlmRomData.LowerNorfairTriggerX, ChozoStatuePlmRomData.LowerNorfairTriggerY));
        if (level.GetCollisionBlockByIndex(index).LevelWord != ChozoStatuePlmRomData.BlankAirLevelWord) return;
        WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
            ChozoStatuePlmRomData.LowerNorfairHandBts.Value);
        slot.Active = false;
    }
}
