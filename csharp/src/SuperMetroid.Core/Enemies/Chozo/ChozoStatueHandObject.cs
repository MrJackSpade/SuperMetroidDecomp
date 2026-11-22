using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The room object a Chozo statue's setup spawns as its hand (#2554): each carries the room object it becomes and the
/// setup its spawn runs, as <see cref="ChozoStatueRoomObject"/> does, and is spawned only with the statue it belongs to.
/// </summary>
public sealed partial class ChozoStatueHandObject
{
    /// <summary>The object's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Creates the room object's behaviour for the spawn, given the statue's room objects.</summary>
    private readonly Func<ChozoStatueObjectSpawn, RoomObjectBehaviour> _create;

    /// <summary>Runs the spawn's setup on the object's block once it is placed.</summary>
    private readonly Action<ChozoStatueObjectSpawn> _setUp;

    /// <summary>A hand named <paramref name="name"/> that <paramref name="create"/> builds and <paramref name="setUp"/> sets up.</summary>
    /// <param name="name">The object's name, for diagnostics.</param>
    /// <param name="create">Creates the room object's behaviour.</param>
    /// <param name="setUp">Runs the spawn's setup on the object's block.</param>
    private ChozoStatueHandObject(string name, Func<ChozoStatueObjectSpawn, RoomObjectBehaviour> create,
        Action<ChozoStatueObjectSpawn> setUp)
    {
        _name = name;
        _create = create;
        _setUp = setUp;
    }

    /// <summary>
    /// The Lower Norfair statue's hand: it lowers the acid at once when the statue has already done so, and otherwise
    /// waits for the Power Bomb block at its trigger to break open and makes that block the hand.
    /// </summary>
    public static ChozoStatueHandObject LowerNorfair { get; } = new(nameof(LowerNorfair),
        spawn => spawn.Statue.CreateLowerNorfairHand(),
        static _ => { });

    /// <summary>The Wrecked Ship statue's hand: its spawn makes its block the hand, and it leaves on its first update.</summary>
    public static ChozoStatueHandObject WreckedShip { get; } = new(nameof(WreckedShip),
        static _ => new TerrainChangeObject(ChozoStatueObjectDefinitions.WreckedShipHand),
        spawn => ChozoStatueObjects.WriteChozoBlock(spawn.Level, spawn.Block, RoomCollisionType.SpecialBlock, ChozoStatueRoomDefinitions.WreckedShipHandBehavior));

    /// <summary>Creates the room object's behaviour for <paramref name="spawn"/>.</summary>
    /// <param name="spawn">The statue's room objects, which the Lower Norfair hand reports to, the room's blocks and the object's block.</param>
    internal RoomObjectBehaviour Create(ChozoStatueObjectSpawn spawn) => _create(spawn);

    /// <summary>Runs the setup of <paramref name="spawn"/> on the object's block once it is placed.</summary>
    /// <param name="spawn">The statue's room objects, the room's blocks and the object's block.</param>
    internal void SetUp(ChozoStatueObjectSpawn spawn) => _setUp(spawn);

    /// <summary>Names the object.</summary>
    public override string ToString() => _name;
}
