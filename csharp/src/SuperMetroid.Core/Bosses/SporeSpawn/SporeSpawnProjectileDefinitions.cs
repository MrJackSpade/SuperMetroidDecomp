namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Compiled spawn geometry and movement mechanics for Spore Spawn projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
    /// <summary>
    /// His stalk's four segments in the order he spawns them, each relative to his body: on his centre line, from 64 pixels
    /// above him downward eight pixels apart.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> StalkSegments { get; } =
        [new(0, -64), new(0, -56), new(0, -48), new(0, -40)];

    /// <summary>The four ceiling spore emitters in the order he opens them, centred in successive 64-pixel columns of his room.</summary>
    internal static IReadOnlyList<Position16<RoomPixels>> Emitters { get; } =
        [new(32, 520), new(96, 520), new(160, 520), new(224, 520)];

    /// <summary>A spore's drift path: its signed pixel movement at each step, repeating after the last.</summary>
    internal static readonly Offset16<RoomPixels>[] Drift =
    [
        new(0, 1), new(1, 0), new(0, 1), new(1, 0), new(0, 1), new(1, 0), new(0, 1), new(1, 0),
        new(0, 1), new(1, 0), new(1, 0), new(0, 1), new(1, 0), new(0, 1), new(1, 0), new(0, 1),
        new(1, 0), new(0, 1), new(1, 0), new(1, 0), new(0, 1), new(1, 0), new(0, 1), new(1, 0),
        new(1, 0), new(0, 1), new(1, 0), new(0, 1), new(1, 0), new(1, 0), new(0, 1), new(1, 0),
        new(1, 0), new(0, 1), new(1, 0), new(1, 0), new(0, 1), new(1, 0), new(1, 0), new(1, 0),
        new(0, 1), new(1, 0), new(1, 0), new(1, 0), new(0, 1), new(1, 0), new(1, 0), new(1, 0),
        new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0),
        new(1, 0), new(0, -1), new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0), new(1, 0),
        new(0, -1), new(1, 0), new(1, 0), new(0, 0), new(-1, 1), new(-1, 1), new(-1, 1), new(-1, 0),
        new(0, 1), new(-1, 1), new(-1, 1), new(-1, 0), new(0, 1), new(-1, 1), new(-1, 0), new(0, 1),
        new(-1, 0), new(0, 1), new(-1, 0), new(0, 1), new(-1, 0), new(-1, 1), new(-1, 1), new(-1, 0),
        new(0, 1), new(-1, 0), new(-1, 1), new(-1, 0), new(0, 1), new(-1, 0), new(-1, 1), new(-1, 0),
        new(-1, 0), new(0, 1), new(-1, 0), new(-1, 0), new(0, 1), new(-1, 0), new(-1, 0), new(-1, 0),
        new(-1, 1), new(-1, 0), new(-1, 0), new(-1, 0), new(-1, 0), new(-1, 0), new(-1, 0), new(-1, 0),
        new(-1, 0), new(-1, 0), new(0, -1), new(-1, 0), new(-1, 0), new(-1, 0), new(-1, -1), new(-1, 0),
        new(-1, 0), new(-1, 0), new(0, -1), new(-1, 0), new(-1, 0), new(-1, -1), new(-1, 0), new(0, 0),
    ];
}
