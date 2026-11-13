using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Chozo statue hand room objects: the Wrecked Ship and Lower Norfair hand checks and the hardcoded statue spawns.</summary>
[RoomLifetime]
internal sealed class ChozoStatueObjects : IChozoStatueHand
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>Descending room-object allocation scanned for temporary reactions and persistent statue actors.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds Chozo hand behavior to its room services and room-object slot allocation.</summary>
    internal ChozoStatueObjects(SharedGameState? system, AreaId area, RoomObjectSlotTable slotTable)
    {
        _system = system;
        _area = area;
        _slotTable = slotTable;
    }
    /// <summary>The room's enemy room-object requests for hand activation, bound when the room loads.</summary>
    private IEnemyObjectRequests? _chozoHands;
    /// <summary>The room's FX owner for Lower Norfair acid height writes, bound when the room loads.</summary>
    private RoomFxOverlayState? _chozoRoomFx;

    /// <summary>Connects the hand setup to the existing actor owner.</summary>
    internal void BindChozoStatueContext(IEnemyObjectRequests hands, EnemyRoomFx enemyRoomFx, RoomFxOverlayState roomFx)
    {
        _chozoHands = hands;
        _chozoRoomFx = roomFx;
        enemyRoomFx.BindRoomFx(roomFx);
    }

    /// <summary>Runs the one-shot special-block setup; the caller still clips to solid.</summary>
    public void NotifyChozoStatueHandCollision(RoomLevelData level,
        RoomCollisionBlock block, SamusState samus, SamusPoseId collisionPose, bool movingDown)
    {
        bool wreckedShip = _area == AreaId.WreckedShip &&
            block.Behavior == ChozoStatueRoomDefinitions.WreckedShipHandBehavior;
        bool lowerNorfair = _area == AreaId.Norfair &&
            block.Behavior == ChozoStatueRoomDefinitions.LowerNorfairHandBehavior;
        if (!wreckedShip && !lowerNorfair) return;
        // Both setups return SEC regardless of admission. The morph-left pose
        // is deliberately absent: the game explicitly lists these three poses.
        bool eligible = movingDown && collisionPose is
            (SamusPoseId.MorphBallGroundRightPose or
             SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose);
        eligible &= wreckedShip
            ? (_system ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
                .HasAnyBossBits(_area, BossBits.AreaBoss)
            : samus.CollectedItems.HasAny(SamusEquipmentFlags.SpaceJump);
        if (_slotTable.SpawnAtCurrentBlock() is not { } reaction)
            return;

        reaction.Header = wreckedShip ? RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck
            : RoomObjectHeaderId.CollisionLowerNorfairChozoHandCheck;
        reaction.Block = level.BlockAt(block.Index);
        if (eligible)
        {
            IEnemyObjectRequests hands = _chozoHands ??
                throw new InvalidOperationException("Chozo hand has no bound enemy room-object requests.");
            hands.ActivateChozoStatueHandTrigger(level, reaction.Block);
            // The nested hardcoded spawn occurs while the reaction slot is occupied.
            hands.ApplyPendingChozoStatueObjects(level, this);
        }
        reaction.Delete();
    }

    /// <summary>Spawns one of the statues' room objects at the first free place in the update order and runs its setup.</summary>
    public bool TrySpawnChozoStatueObject(RoomLevelData level, ChozoStatueObjectRequest request)
    {
        RoomObjectHeaderId header = ChozoStatueObjectHeaders.Require(request.Header);
        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if ((request.IsHardcoded ? _slotTable.SpawnHardcoded() : _slotTable.SpawnAtCurrentBlock()) is not { } slot)
            return false;

        slot.Header = header;
        slot.Block = block;
        slot.Become(header == RoomObjectHeaderId.LowerNorfairChozoHand ? new LowerNorfairChozoHandObject(this)
            : header == RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo ? ChozoSlopeAccessObject.Clearing()
            : header == RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo ? ChozoSlopeAccessObject.Blocking()
            : header == RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug ? new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug)
            : new TerrainChangeObject(ChozoStatueObjectDefinitions.WreckedShipHand));
        if (header == RoomObjectHeaderId.WreckedShipChozoHand)
            WriteChozoBlock(level, block, RoomCollisionType.SpecialBlock,
                ChozoStatueRoomDefinitions.WreckedShipHandBehavior);
        else if (request.Header == RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug)
        {
            // The plug spawn never chooses the block it writes: the two spawn entry points
            // leave different blocks behind, so preserve that quirk rather than reading
            // the previous tile.
            level.SetTerrain(block, request.IsHardcoded
                ? ChozoStatueRoomDefinitions.HardcodedCrumbleSetupBlock
                : ChozoStatueRoomDefinitions.SpawnedCrumbleSetupBlock);
        }
        return true;
    }

    /// <summary>Changes a hand or plug block's collision kind and behaviour while preserving its appearance.</summary>
    internal static void WriteChozoBlock(RoomLevelData level, Position16<RoomBlocks> block,
        RoomCollisionType type, RoomBlockBehavior behavior)
    {
        RoomTerrainBlock terrain = level.GetCollisionBlock(block).Terrain;
        level.SetTerrain(block, terrain.WithCollision(type));
        level.SetBehavior(block, behavior);
    }

    /// <summary>Whether the Lower Norfair acid has been lowered, which the Lower Norfair hand checks as it starts.</summary>
    internal bool HasLoweredLowerNorfairAcid =>
        (_system ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
            .HasEvent(StoryEvent.LowerNorfairChozoLoweredAcid);

    /// <summary>Turns the Wrecked Ship Chozo spikes into slopes.</summary>
    internal static void TransformSpikesToSlopes(RoomLevelData level)
    {
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlock,
            RoomCollisionType.Slope, ChozoStatueRoomDefinitions.FirstSlopeBehavior);
        WriteChozoBlock(level, SecondSlopeBlock,
            RoomCollisionType.Slope, ChozoStatueRoomDefinitions.SecondSlopeBehavior);
    }

    /// <summary>Turns the Wrecked Ship Chozo slopes back into spikes.</summary>
    internal static void RevertSlopesToSpikes(RoomLevelData level)
    {
        WriteChozoBlock(level, ChozoStatueRoomDefinitions.FirstSlopeBlock,
            RoomCollisionType.SpikeBlock, RoomBlockBehavior.None);
        WriteChozoBlock(level, SecondSlopeBlock,
            RoomCollisionType.SpikeBlock, RoomBlockBehavior.None);
    }

    /// <summary>The second slope block, to the right of the first.</summary>
    private static Position16<RoomBlocks> SecondSlopeBlock =>
        ChozoStatueRoomDefinitions.FirstSlopeBlock + new Offset16<RoomBlocks>(1, 0);

    /// <summary>Moves the Lower Norfair acid to its lowered height.</summary>
    internal void SetLoweredAcidHeight() =>
        (_chozoRoomFx ?? throw new InvalidOperationException("The Chozo statue's room objects have no FX owner."))
            .SetLiquidMotion(baseYPosition: ChozoStatueRoomDefinitions.LoweredAcidY);
}
