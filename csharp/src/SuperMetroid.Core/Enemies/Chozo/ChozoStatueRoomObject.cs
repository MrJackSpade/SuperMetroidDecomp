using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// A room object one of the two Chozo statues spawns, named for what it does to the statue's room (#1622): each carries
/// the room object it becomes and the setup its spawn runs on its block (#2571), so the statue requests it directly and no
/// switch over room-object identities builds or sets it up. The plug's two crumbles are one object spawned two ways, each
/// leaving its own picture.
/// </summary>
public sealed class ChozoStatueRoomObject
{
    /// <summary>The object's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Creates the room object's behaviour, given the statue's room objects.</summary>
    private readonly Func<ChozoStatueObjects, RoomObjectBehaviour> _create;

    /// <summary>Runs the spawn's setup on the object's block once it is placed.</summary>
    private readonly Action<ChozoStatueObjects, RoomLevelData, Position16<RoomBlocks>> _setUp;

    /// <summary>An object named <paramref name="name"/> that <paramref name="create"/> builds and <paramref name="setUp"/> sets up.</summary>
    /// <param name="name">The object's name, for diagnostics.</param>
    /// <param name="create">Creates the room object's behaviour.</param>
    /// <param name="setUp">Runs the spawn's setup on the object's block.</param>
    private ChozoStatueRoomObject(string name, Func<ChozoStatueObjects, RoomObjectBehaviour> create,
        Action<ChozoStatueObjects, RoomLevelData, Position16<RoomBlocks>> setUp)
    {
        _name = name;
        _create = create;
        _setUp = setUp;
    }

    /// <summary>
    /// The Lower Norfair statue's hand: it lowers the acid at once when the statue has already done so, and otherwise
    /// waits for the Power Bomb block at its trigger to break open and makes that block the hand.
    /// </summary>
    public static ChozoStatueRoomObject LowerNorfairHand { get; } = new(nameof(LowerNorfairHand),
        statue => new LowerNorfairChozoHandObject(statue),
        static (_, _, _) => { });

    /// <summary>The Wrecked Ship statue's hand: its spawn makes its block the hand, and it leaves on its first update.</summary>
    public static ChozoStatueRoomObject WreckedShipHand { get; } = new(nameof(WreckedShipHand),
        _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.WreckedShipHand),
        (_, level, block) => ChozoStatueObjects.WriteChozoBlock(level, block, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.WreckedShipHandBehavior));

    /// <summary>The Wrecked Ship statue's slope access opening: it draws the opened access, then the spikes become slopes.</summary>
    public static ChozoStatueRoomObject SlopeAccessOpening { get; } = new(nameof(SlopeAccessOpening),
        _ => ChozoSlopeAccessObject.Clearing(),
        static (_, _, _) => { });

    /// <summary>The Wrecked Ship statue's slope access closing: it draws the closed access, then the slopes become spikes.</summary>
    public static ChozoStatueRoomObject SlopeAccessClosing { get; } = new(nameof(SlopeAccessClosing),
        _ => ChozoSlopeAccessObject.Blocking(),
        static (_, _, _) => { });

    /// <summary>
    /// The Lower Norfair room's plug crumbling as the hand wakes the statue: its spawn at the room's fixed plug block
    /// makes the block air, showing the picture a spawn at a fixed block leaves (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    public static ChozoStatueRoomObject PlugCrumbleAtHand { get; } = new(nameof(PlugCrumbleAtHand),
        _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug),
        (_, level, block) => level.SetTerrain(block, ChozoStatueObjects.CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFixedBlock)));

    /// <summary>
    /// A plug crumbling under the walking statue's footstep: its spawn at the block the footstep found makes the block
    /// air, showing the picture a spawn at a found block leaves (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    public static ChozoStatueRoomObject PlugCrumbleUnderFootstep { get; } = new(nameof(PlugCrumbleUnderFootstep),
        _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug),
        (_, level, block) => level.SetTerrain(block, ChozoStatueObjects.CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFoundBlock)));

    /// <summary>Creates the room object's behaviour.</summary>
    /// <param name="statue">The statue's room objects, which the Lower Norfair hand reports to.</param>
    internal RoomObjectBehaviour Create(ChozoStatueObjects statue) => _create(statue);

    /// <summary>Runs the spawn's setup on the object's block once it is placed.</summary>
    /// <param name="statue">The statue's room objects.</param>
    /// <param name="level">The room's blocks.</param>
    /// <param name="block">The object's block.</param>
    internal void SetUp(ChozoStatueObjects statue, RoomLevelData level, Position16<RoomBlocks> block) => _setUp(statue, level, block);

    /// <summary>Names the object.</summary>
    public override string ToString() => _name;
}
