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
    private readonly IRoomLiquid _roomFx;

    /// <summary>Binds Chozo hand behavior to its room services and room objects.</summary>
    /// <param name="system">The room's system and event state.</param>
    /// <param name="area">The room's area.</param>
    /// <param name="hands">Starts the statue's hand sequence, which the room's enemies run.</param>
    /// <param name="roomFx">The room's liquid.</param>
    /// <param name="places">The room's room objects.</param>
    internal ChozoStatueObjects(SharedGameState system, AreaId area, IChozoStatueHandActivation hands, IRoomLiquid roomFx,
        RoomObjectPlaceList places)
    {
        _system = system;
        _area = area;
        _hands = hands;
        _roomFx = roomFx;
        _places = places;
    }

    /// <summary>
    /// Samus touching the hand: she is still stopped by the solid block, and an admitted touch turns the hand block to
    /// air and wakes the statue.
    /// </summary>
    public void NotifyChozoStatueHandCollision(RoomLevelData level,
        Position16<RoomBlocks> block, SamusState samus, SamusPoseId collisionPose, bool movingDown)
    {
        Ensure.NotNull(level);
        RoomBlockBehavior behavior = level.GetCollisionBlock(block).Behavior;
        if (ChozoStatueSiteDefinitions.Sites.Values.FirstOrDefault(site => site.Area == _area && site.HandBehavior == behavior) is not { } site)
            return;
        // The hand block is solid whether or not the touch is admitted. Samus morphed facing left is not admitted; only
        // these three poses are.
        bool eligible = movingDown && collisionPose is
            (SamusPoseId.MorphBallGroundRightPose or
             SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose);
        eligible &= site.Admission switch
        {
            ChozoHandAdmission.AreaBossDefeated => _system.HasDefeatedAnyOf(_area, DefeatedBosses.AreaBoss),
            ChozoHandAdmission.SpaceJumpCollected => samus.CollectedItems.HasAny(SamusEquipmentFlags.SpaceJump),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChozoHandAdmission)} {site.Admission}."),
        };
        // The check holds a place of its own while it runs, so the statue's objects it spawns take the places after it.
        if (_places.Spawn() is not { } check)
            return;

        if (eligible)
        {
            // Both triggers turn the hand block to air before waking the statue (#2502); its behaviour, picture and
            // what it shows survive until the room object the statue then spawns runs.
            level.SetCollision(block, RoomCollisionType.Air);
            _hands.ActivateChozoStatueHand(level);
        }
        check.Delete();
    }

    /// <summary>Creates the Lower Norfair statue's hand, which reports to these room objects.</summary>
    internal LowerNorfairChozoHandObject CreateLowerNorfairHand() => new(this);

    /// <summary>
    /// Spawns the statue's room object <paramref name="request"/> names at its block, at the first free place in the
    /// update order, and runs its setup.
    /// </summary>
    /// <remarks>When the room is full nothing is spawned or changed (<see cref="Compatibility.RoomObjectPlaces"/>).</remarks>
    public void SpawnChozoStatueObject(RoomLevelData level, RoomObjectSpawnRequest<ChozoStatueRoomObject> request)
    {
        Ensure.NotNull(level);
        var spawn = new ChozoStatueObjectSpawn(this, level, level.GridBlock(request.Block));
        RoomObjectBehaviour roomObject = request.RoomObject.Create(spawn);
        if (_places.Spawn() is not { } place)
            return;

        place.Block = spawn.Block;
        place.Become(roomObject);
        request.RoomObject.SetUp(spawn);
    }

    /// <summary>
    /// The air block a crumbling plug's setup leaves after <paramref name="spawn"/>, showing the picture that spawn left
    /// (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    internal static RoomTerrainBlock CrumblePlugAir(CrumblePlugLeftBlock.Spawn spawn)
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
    /// Changes a hand, trigger or slope block's collision and behaviour, keeping its picture and what it shows (#2502).
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
