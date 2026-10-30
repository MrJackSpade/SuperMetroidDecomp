using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo statue hand PLMs ($84:D6EA family): the Wrecked Ship and Lower Norfair hand checks and the hardcoded statue spawns.</summary>
[RoomLifetime]
internal sealed class ChozoStatuePlms : IChozoStatueHand
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Descending native PLM allocation scanned for temporary reactions and persistent statue actors.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds Chozo hand behavior to its room services and PLM slot allocation.</summary>
    internal ChozoStatuePlms(SharedGameState? system, AreaId area, PlmSlotTable slotTable)
    {
        _system = system;
        _area = area;
        _slotTable = slotTable;
    }
    /// <summary>Bound enemy PLM requests for hand activation; reattached after room load or state restore.</summary>
    [NonSerialized] private IEnemyPlmRequests? _chozoHands;
    /// <summary>Bound room FX owner for Lower Norfair acid height writes; reattached after restore.</summary>
    [NonSerialized] private RoomLayer3FxState? _chozoRoomFx;

    /// <summary>Connects the bank-$84 hand setup to the existing bank-$AA actor owner.</summary>
    internal void BindChozoStatueContext(IEnemyPlmRequests hands, EnemyRoomFx enemyRoomFx, RoomLayer3FxState roomFx)
    {
        _chozoHands = hands;
        _chozoRoomFx = roomFx;
        enemyRoomFx.BindRoomFx(roomFx);
    }

    /// <summary>Runs the native one-shot special-block setup; the caller still clips to solid.</summary>
    public void NotifyChozoStatueHandCollision(RoomLevelData level,
        RoomCollisionBlock block, SamusState samus, SamusPoseId collisionPose, bool movingDown)
    {
        bool wreckedShip = _area == AreaId.WreckedShip &&
            block.Behavior == ChozoStatueRoomDefinitions.WreckedShipHandBehavior;
        bool lowerNorfair = _area == AreaId.Norfair &&
            block.Behavior == ChozoStatueRoomDefinitions.LowerNorfairHandBehavior;
        if (!wreckedShip && !lowerNorfair) return;
        // Both native setups return SEC regardless of admission. The morph-left pose
        // is deliberately absent: the cartridge explicitly lists these three poses.
        bool eligible = movingDown && collisionPose is
            (SamusPoseId.MorphBallGroundRightPose or
             SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose);
        eligible &= wreckedShip
            ? (_system ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
                .HasAnyBossBits(_area, BossBits.AreaBoss)
            : samus.CollectedItems.HasAny(SamusEquipmentFlags.SpaceJump);
        if (_slotTable.SpawnAtCurrentBlock() is not { } reaction)
            return;

        reaction.Header = wreckedShip ? PlmHeaderId.CollisionWreckedShipChozoHandCheck
            : PlmHeaderId.CollisionLowerNorfairChozoHandCheck;
        reaction.BlockIndex = block.Index;
        if (eligible)
        {
            IEnemyPlmRequests hands = _chozoHands ??
                throw new InvalidOperationException("Chozo hand has no bound enemy PLM requests.");
            hands.ActivateChozoStatueHandTrigger(level, block.Index);
            // The nested hardcoded spawn occurs while the reaction slot is occupied.
            hands.ApplyPendingChozoStatuePlms(level, this);
        }
        reaction.Clear();
    }

    /// <summary>Allocates the native descending PLM slot, runs setup, then leaves its ROM list live.</summary>
    public bool TrySpawnChozoStatuePlm(RoomLevelData level, ChozoStatuePlmRequest request)
    {
        ChozoStatuePlmDefinition definition =
            ChozoStatuePlmHeaders.Resolve(request.Header);
        int index = level.GetBlockIndex(request.Block);
        if ((request.IsHardcoded ? _slotTable.SpawnHardcoded() : _slotTable.SpawnAtCurrentBlock()) is not { } slot)
            return false;

        slot.Header = definition.Header;
        slot.BlockIndex = index;
        definition.Program.StartOn(slot);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        if (request.Header == PlmHeaderId.WreckedShipChozoHand)
            WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
                ChozoStatueRoomDefinitions.WreckedShipHandBehavior);
        else if (request.Header == PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug)
        {
            // Native D108 never chooses the block it writes: the two spawn entry points
            // leave different blocks behind, so preserve that quirk rather than reading
            // the previous tile.
            level.SetTerrain(index, request.IsHardcoded
                ? ChozoStatueRoomDefinitions.HardcodedCrumbleSetupBlock
                : ChozoStatueRoomDefinitions.SpawnedCrumbleSetupBlock);
        }
        return true;
    }

    /// <summary>Changes a hand or plug block's collision kind and behaviour while preserving its appearance.</summary>
    private static void WriteChozoBlock(RoomLevelData level, int index,
        RoomCollisionType type, RoomBlockBehavior behavior)
    {
        RoomTerrainBlock terrain = level.GetCollisionBlockByIndex(index).Terrain;
        level.SetTerrain(index, terrain.WithCollision(type));
        level.SetBehavior(index, behavior);
    }

    /// <summary>Whether the Lower Norfair acid has been lowered, the event the hand's program branches on.</summary>
    internal bool HasLoweredLowerNorfairAcid =>
        (_system ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
            .HasEvent(EventNumber.LowerNorfairChozoLoweredAcid);

    /// <summary>Turns the Wrecked Ship Chozo spikes into slopes.</summary>
    internal static void TransformSpikesToSlopes(RoomLevelData level)
    {
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlockIndex,
            RoomCollisionType.Slope, ChozoStatueRoomDefinitions.FirstSlopeBehavior);
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlockIndex + 1,
            RoomCollisionType.Slope, ChozoStatueRoomDefinitions.SecondSlopeBehavior);
    }

    /// <summary>Turns the Wrecked Ship Chozo slopes back into spikes.</summary>
    internal static void RevertSlopesToSpikes(RoomLevelData level)
    {
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlockIndex,
            RoomCollisionType.SpikeBlock, RoomBlockBehaviorValues.None);
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlockIndex + 1,
            RoomCollisionType.SpikeBlock, RoomBlockBehaviorValues.None);
    }

    /// <summary>Moves the Lower Norfair acid to its lowered height.</summary>
    internal void SetLoweredAcidHeight() =>
        (_chozoRoomFx ?? throw new InvalidOperationException("Chozo PLM has no FX owner."))
            .SetLiquidMotion(baseYPosition: ChozoStatueRoomDefinitions.LoweredAcidY);

    /// <summary>Restores the Lower Norfair hand collision block once its trigger location becomes air.</summary>
    internal static void RunChozoStatuePreInstruction(RoomLevelData level, PlmSlot slot)
    {
        int index = level.GetBlockIndex(new(ChozoStatueRoomDefinitions.LowerNorfairTriggerX, ChozoStatueRoomDefinitions.LowerNorfairTriggerY));
        if (level.GetCollisionBlockByIndex(index).Terrain != RoomTerrainBlocks.BlankAir) return;
        WriteChozoBlock(level, index, RoomCollisionType.SpecialBlock,
            ChozoStatueRoomDefinitions.LowerNorfairHandBehavior);
        slot.Delete();
    }
}
