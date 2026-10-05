namespace SuperMetroid.Core.Game;

/// <summary>One signed pixel delta pair from Spore Spawn's wrapped movement stream.</summary>
internal readonly record struct SporeSpawnMovementDelta(sbyte X, sbyte Y);

/// <summary>Compiled spawn geometry and movement mechanics for Spore Spawn projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
    /// <summary>The 256-byte wrapped signed movement stream at <c>$86:DD6C-$DE6B</c>.</summary>
    private static readonly sbyte[] Movement =
    [
        0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,
        0,1,1,0,1,0,0,1,1,0,0,1,1,0,0,1,
        1,0,0,1,1,0,1,0,0,1,1,0,0,1,1,0,
        1,0,0,1,1,0,0,1,1,0,1,0,0,1,1,0,
        1,0,0,1,1,0,1,0,0,1,1,0,1,0,1,0,
        0,1,1,0,1,0,1,0,0,1,1,0,1,0,1,0,
        1,0,1,0,1,0,1,0,1,0,1,0,1,0,1,0,
        1,0,0,-1,1,0,1,0,1,0,1,0,1,0,1,0,
        0,-1,1,0,1,0,0,0,-1,1,-1,1,-1,1,-1,0,
        0,1,-1,1,-1,1,-1,0,0,1,-1,1,-1,0,0,1,
        -1,0,0,1,-1,0,0,1,-1,0,-1,1,-1,1,-1,0,
        0,1,-1,0,-1,1,-1,0,0,1,-1,0,-1,1,-1,0,
        -1,0,0,1,-1,0,-1,0,0,1,-1,0,-1,0,-1,0,
        -1,1,-1,0,-1,0,-1,0,-1,0,-1,0,-1,0,-1,0,
        -1,0,-1,0,0,-1,-1,0,-1,0,-1,0,-1,-1,-1,0,
        -1,0,-1,0,0,-1,-1,0,-1,0,-1,-1,-1,0,0,0,
    ];

    /// <summary>$86:DCB9: four stalk segments spaced eight pixels apart, starting 64 pixels above the owner.</summary>
    internal static ushort StalkYOffset(ushort spawnArgument) =>
        unchecked((ushort)(-64 + 8 * SpawnIndex(spawnArgument)));

    /// <summary>$86:DCE6: four ceiling emitters centered in successive 64-pixel room columns.</summary>
    internal static ushort SpawnerX(ushort spawnArgument) =>
        (ushort)(32 + 64 * SpawnIndex(spawnArgument));

    internal static SporeSpawnMovementDelta MovementAt(byte offset) =>
        new(Movement[offset], Movement[unchecked((byte)(offset + 1))]);

    private static int SpawnIndex(ushort spawnArgument)
    {
        if (spawnArgument > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        return spawnArgument;
    }
}
