using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Chozo statues' room objects: the Wrecked Ship and Lower Norfair hand checks and the room objects the statues spawn.</summary>
[RoomLifetime]
internal sealed class ChozoStatueObjects : IChozoStatueHand
{
    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>The room's room objects, which the hand checks and the statues' objects join.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds Chozo hand behavior to its room services and room objects.</summary>
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
        Position16<RoomBlocks> block, SamusState samus, SamusPoseId collisionPose, bool movingDown)
    {
        Ensure.NotNull(level);
        RoomBlockBehavior behavior = level.GetCollisionBlock(block).Behavior;
        bool wreckedShip = _area == AreaId.WreckedShip &&
            behavior == ChozoStatueRoomDefinitions.WreckedShipHandBehavior;
        bool lowerNorfair = _area == AreaId.Norfair &&
            behavior == ChozoStatueRoomDefinitions.LowerNorfairHandBehavior;
        if (!wreckedShip && !lowerNorfair) return;
        // Both setups report the block solid regardless of admission. The morph-left pose
        // is deliberately absent: the game explicitly lists these three poses.
        bool eligible = movingDown && collisionPose is
            (SamusPoseId.MorphBallGroundRightPose or
             SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose);
        eligible &= wreckedShip
            ? (_system ?? throw new InvalidOperationException("Chozo hand has no progression owner."))
                .HasAnyBossBits(_area, BossBits.AreaBoss)
            : samus.CollectedItems.HasAny(SamusEquipmentFlags.SpaceJump);
        // The check holds a place of its own while it runs, so the statue's objects it spawns take the places after it.
        if (_slotTable.Spawn() is not { } check)
            return;

        if (eligible)
        {
            IEnemyObjectRequests hands = _chozoHands ??
                throw new InvalidOperationException("Chozo hand has no bound enemy room-object requests.");
            hands.ActivateChozoStatueHandTrigger(level, block);
            hands.ApplyPendingChozoStatueObjects(level, this);
        }
        check.Delete();
    }

    /// <summary>
    /// Spawns the statue's room object <paramref name="request"/> names at its block, at the first free place in the
    /// update order, and runs its setup.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned or changed then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    /// <exception cref="ArgumentOutOfRangeException">The request names no statue room object.</exception>
    public bool TrySpawnChozoStatueObject(RoomLevelData level, RoomObjectSpawnRequest<ChozoStatueRoomObject> request)
    {
        Ensure.NotNull(level);
        RoomObjectBehaviour roomObject = request.RoomObject switch
        {
            ChozoStatueRoomObject.LowerNorfairHand => new LowerNorfairChozoHandObject(this),
            ChozoStatueRoomObject.WreckedShipHand => new TerrainChangeObject(ChozoStatueObjectDefinitions.WreckedShipHand),
            ChozoStatueRoomObject.SlopeAccessOpening => ChozoSlopeAccessObject.Clearing(),
            ChozoStatueRoomObject.SlopeAccessClosing => ChozoSlopeAccessObject.Blocking(),
            ChozoStatueRoomObject.PlugCrumbleAtHand or ChozoStatueRoomObject.PlugCrumbleUnderFootstep =>
                new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.RoomObject, "Not a Chozo statue's room object."),
        };
        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(roomObject);
        switch (request.RoomObject)
        {
            case ChozoStatueRoomObject.WreckedShipHand:
                WriteChozoBlock(level, block, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.WreckedShipHandBehavior);
                return true;
            case ChozoStatueRoomObject.PlugCrumbleAtHand:
                // The plug's setup never chooses the block it leaves: each of its two spawns leaves its own.
                level.SetTerrain(block, ChozoStatueRoomDefinitions.HandCrumbleSetupBlock);
                return true;
            case ChozoStatueRoomObject.PlugCrumbleUnderFootstep:
                level.SetTerrain(block, ChozoStatueRoomDefinitions.FootstepCrumbleSetupBlock);
                return true;
            case ChozoStatueRoomObject.LowerNorfairHand:
            case ChozoStatueRoomObject.SlopeAccessOpening:
            case ChozoStatueRoomObject.SlopeAccessClosing:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.RoomObject, "Not a Chozo statue's room object.");
        }
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
