using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// A room object one of the two Chozo statues spawns, named for what it does to the statue's room (#1622): each carries
/// the room object it becomes and the setup its spawn runs on its block (#2571), so the statue requests it directly and no
/// switch over room-object identities builds or sets it up. The statues' hands are <see cref="ChozoStatueHandObject"/>s. The plug's two crumbles are one object spawned two ways, each
/// leaving its own picture.
/// </summary>
public sealed class ChozoStatueRoomObject
{
    /// <summary>The object's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Creates the room object's behaviour for the spawn, given the statue's room objects.</summary>
    private readonly Func<ChozoStatueObjectSpawn, RoomObjectBehaviour> _create;

    /// <summary>Runs the spawn's setup on the object's block once it is placed.</summary>
    private readonly Action<ChozoStatueObjectSpawn> _setUp;

    /// <summary>An object named <paramref name="name"/> that <paramref name="create"/> builds and <paramref name="setUp"/> sets up.</summary>
    /// <param name="name">The object's name, for diagnostics.</param>
    /// <param name="create">Creates the room object's behaviour.</param>
    /// <param name="setUp">Runs the spawn's setup on the object's block.</param>
    private ChozoStatueRoomObject(string name, Func<ChozoStatueObjectSpawn, RoomObjectBehaviour> create,
        Action<ChozoStatueObjectSpawn> setUp)
    {
        _name = name;
        _create = create;
        _setUp = setUp;
    }

    /// <summary>The Wrecked Ship statue's slope access opening: it draws the opened access, then the spikes become slopes.</summary>
    public static ChozoStatueRoomObject SlopeAccessOpening { get; } = new(nameof(SlopeAccessOpening),
        static _ => ChozoSlopeAccessObject.Clearing(),
        static _ => { });

    /// <summary>The Wrecked Ship statue's slope access closing: it draws the closed access, then the slopes become spikes.</summary>
    public static ChozoStatueRoomObject SlopeAccessClosing { get; } = new(nameof(SlopeAccessClosing),
        static _ => ChozoSlopeAccessObject.Blocking(),
        static _ => { });

    /// <summary>
    /// The Lower Norfair room's plug crumbling as the hand wakes the statue: its spawn at the room's fixed plug block
    /// makes the block air, showing the picture a spawn at a fixed block leaves (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    public static ChozoStatueRoomObject PlugCrumbleAtHand { get; } = new(nameof(PlugCrumbleAtHand),
        static _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug),
        spawn => spawn.Level.SetTerrain(spawn.Block, ChozoStatueObjects.CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFixedBlock)));

    /// <summary>
    /// A plug crumbling under the walking statue's footstep: its spawn at the block the footstep found makes the block
    /// air, showing the picture a spawn at a found block leaves (<see cref="CrumblePlugLeftBlock"/>).
    /// </summary>
    public static ChozoStatueRoomObject PlugCrumbleUnderFootstep { get; } = new(nameof(PlugCrumbleUnderFootstep),
        static _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.CrumblePlug),
        spawn => spawn.Level.SetTerrain(spawn.Block, ChozoStatueObjects.CrumblePlugAir(CrumblePlugLeftBlock.Spawn.AtFoundBlock)));

    /// <summary>Creates the room object's behaviour for <paramref name="spawn"/>.</summary>
    /// <param name="spawn">The statue's room objects, which the Lower Norfair hand reports to, the room's blocks and the object's block.</param>
    internal RoomObjectBehaviour Create(ChozoStatueObjectSpawn spawn) => _create(spawn);

    /// <summary>Runs the setup of <paramref name="spawn"/> on the object's block once it is placed.</summary>
    /// <param name="spawn">The statue's room objects, the room's blocks and the object's block.</param>
    internal void SetUp(ChozoStatueObjectSpawn spawn) => _setUp(spawn);

    /// <summary>Names the object.</summary>
    public override string ToString() => _name;
}
