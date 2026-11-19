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
    private readonly SharedGameState _system;
    /// <summary>The room's area.</summary>
    private readonly AreaId _area;
    /// <summary>The room's room objects, which the hand checks and the statues' objects join.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Starts the statue's hand sequence, which the room's enemies run.</summary>
    private readonly IChozoStatueHandActivation _hands;
    /// <summary>The room's liquid, which the Lower Norfair statue lowers.</summary>
    private readonly IRoomFxMotion _roomFx;

    /// <summary>Binds Chozo hand behavior to its room services and room objects.</summary>
    /// <param name="system">The room's system and event state.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="hands">Starts the statue's hand sequence, which the room's enemies run.</param>
    /// <param name="roomFx">The room's liquid.</param>
    /// <param name="places">The room's room objects.</param>
    internal ChozoStatueObjects(SharedGameState system, AreaId area, IChozoStatueHandActivation hands, IRoomFxMotion roomFx,
        RoomObjectPlaceList places)
    {
        _system = system;
        _area = area;
        _hands = hands;
        _roomFx = roomFx;
        _places = places;
    }

    /// <summary>
    /// Runs the one-shot special-block setup; Samus's collision still clips her to the solid block. An admitted trigger
    /// turns the hand block to air, then wakes the statue.
    /// </summary>
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
            ? _system.HasDefeatedAnyOf(_area, DefeatedBosses.AreaBoss)
            : samus.CollectedItems.HasAny(SamusEquipmentFlags.SpaceJump);
        // The check holds a place of its own while it runs, so the statue's objects it spawns take the places after it.
        if (_places.Spawn() is not { } check)
            return;

        if (eligible)
        {
            // Both trigger setups clear only the hand block's type before waking the statue (#2502). Its behaviour, picture
            // and what it shows survive until the room object the statue then spawns runs.
            level.SetCollision(block, RoomCollisionType.Air);
            _hands.ActivateChozoStatueHand(level);
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
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        place.Become(roomObject);
        switch (request.RoomObject)
        {
            case ChozoStatueRoomObject.WreckedShipHand:
                WriteChozoBlock(level, block, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.WreckedShipHandBehavior);
                return true;
            case ChozoStatueRoomObject.PlugCrumbleAtHand:
                // The plug's setup never chooses the picture it leaves: each of its two spawns leaves its own.
                level.SetTerrain(block, CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFixedBlock));
                return true;
            case ChozoStatueRoomObject.PlugCrumbleUnderFootstep:
                level.SetTerrain(block, CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFoundBlock));
                return true;
            case ChozoStatueRoomObject.LowerNorfairHand:
            case ChozoStatueRoomObject.SlopeAccessOpening:
            case ChozoStatueRoomObject.SlopeAccessClosing:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(request), request.RoomObject, "Not a Chozo statue's room object.");
        }
    }

    /// <summary>
    /// The air block a crumbling plug's setup leaves after <paramref name="spawn"/>, showing the picture that spawn left
    /// (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    private static RoomTerrainBlock CrumblePlugAir(CrumblePlugLeftBlock.Spawn spawn)
    {
        CrumblePlugLeftBlock.Picture left = CrumblePlugLeftBlock.LeftBy(spawn);
        BlockMirroring mirroring = left.Mirrored switch
        {
            CrumblePlugLeftBlock.Mirroring.None => BlockMirroring.None,
            CrumblePlugLeftBlock.Mirroring.BothWays => BlockMirroring.Both,
            _ => throw new InvalidOperationException($"Undefined {nameof(CrumblePlugLeftBlock.Mirroring)} {left.Mirrored}."),
        };
        return RoomTerrainBlock.Of(left.Number, mirroring, RoomCollisionType.Air);
    }

    /// <summary>
    /// Changes a hand, trigger or slope block's collision kind and behaviour, keeping its picture and what it shows, as the
    /// hands' setups and the slopes' changes rewrite only each block's type and parameter (#2502).
    /// </summary>
    internal static void WriteChozoBlock(RoomLevelData level, Position16<RoomBlocks> block,
        RoomCollisionType type, RoomBlockBehavior behavior)
    {
        level.SetCollision(block, type);
        level.SetBehavior(block, behavior);
    }

    /// <summary>Whether the Lower Norfair acid has been lowered, which the Lower Norfair hand checks as it starts.</summary>
    internal bool HasLoweredLowerNorfairAcid => _system.HasEvent(StoryEvent.LowerNorfairChozoLoweredAcid);

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
        ChozoStatueRoomDefinitions.FirstSlopeBlock + Offset16<RoomBlocks>.OneRight;

    /// <summary>Moves the Lower Norfair acid to its lowered height.</summary>
    internal void SetLoweredAcidHeight() => _roomFx.SetLiquidLevel(ChozoStatueRoomDefinitions.LoweredAcidY);
}
