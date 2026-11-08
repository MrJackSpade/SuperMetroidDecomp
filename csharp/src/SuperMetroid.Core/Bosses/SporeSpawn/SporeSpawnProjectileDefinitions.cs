namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Compiled spawn geometry and movement mechanics for Spore Spawn projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
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
