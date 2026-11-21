using SuperMetroid.Core.Enemies;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The spawn geometry and movement of Spore Spawn's projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
    /// <summary>
    /// His stalk's four segments in the order he spawns them, each relative to his body: on his centre line, from 64 pixels
    /// above him downward eight pixels apart.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> StalkSegments { get; } =
        [new(0, -64), new(0, -56), new(0, -48), new(0, -40)];

    /// <summary>
    /// The four ceiling spore emitters in the order he opens them, centred in successive 64-pixel columns of his room: the
    /// two over the room's left half release spores that drift right first, the two over its right half spores that
    /// drift left first.
    /// </summary>
    internal static IReadOnlyList<SporeSpawnEmitter> Emitters { get; } =
    [
        new(new(32, 520), HorizontalDirection.Right), new(new(96, 520), HorizontalDirection.Right),
        new(new(160, 520), HorizontalDirection.Left), new(new(224, 520), HorizontalDirection.Left),
    ];

    /// <summary>The chances the drop a shot spore leaves rolls (#2244): mostly a missile, sometimes energy, never a Super Missile or Power Bomb.</summary>
    internal static readonly EnemyDropChances SporeDropChances = new(smallEnergy: 10, bigEnergy: 20, missile: 200, nothing: 25, superMissile: 0, powerBomb: 0);

    /// <summary>
    /// A spore's drift path: how many pixels it moves across and down at each step, repeating after the last (#2335). It
    /// falls two pixels on each step it falls.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] Drift =
    [
        new(0, 2), new(1, 0), new(0, 2), new(1, 0), new(0, 2), new(1, 0), new(0, 2), new(1, 0),
        new(0, 2), new(1, 0), new(1, 0), new(0, 2), new(1, 0), new(0, 2), new(1, 0), new(0, 2),
        new(1, 0), new(0, 2), new(1, 0), new(1, 0), new(0, 2), new(1, 0), new(0, 2), new(1, 0),
        new(1, 0), new(0, 2), new(1, 0), new(0, 2), new(1, 0), new(1, 0), new(0, 2), new(1, 0),
        new(1, 0), new(0, 2), new(1, 0), new(1, 0), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 3),
        new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 3), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 12), new(0, -2), .. Repeated<Offset16<RoomPixels>>(new(1, 0), 6),
        new(0, -2), new(1, 0), new(1, 0), new(0, 0), .. Repeated<Offset16<RoomPixels>>(new(-1, 2), 3), new(-1, 0),
        new(0, 2), new(-1, 2), new(-1, 2), new(-1, 0), new(0, 2), new(-1, 2), new(-1, 0), new(0, 2),
        new(-1, 0), new(0, 2), new(-1, 0), new(0, 2), new(-1, 0), new(-1, 2), new(-1, 2), new(-1, 0),
        new(0, 2), new(-1, 0), new(-1, 2), new(-1, 0), new(0, 2), new(-1, 0), new(-1, 2), new(-1, 0),
        new(-1, 0), new(0, 2), new(-1, 0), new(-1, 0), new(0, 2), .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 3),
        new(-1, 2), .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 9), new(0, -2), .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 3), new(-1, -2), .. Repeated<Offset16<RoomPixels>>(new(-1, 0), 3), new(0, -2), new(-1, 0), new(-1, 0), new(-1, -2), new(-1, 0), new(0, 0),
    ];
}
