namespace SuperMetroid.Core.Game;

/// <summary>One signed pixel delta pair from Spore Spawn's wrapped movement stream.</summary>
internal readonly record struct SporeSpawnMovementDelta(sbyte X, sbyte Y);

/// <summary>Compiled spawn geometry and movement mechanics for Spore Spawn projectiles.</summary>
internal static class SporeSpawnProjectileDefinitions
{
    /// <summary>The four signed stalk Y offsets at <c>$86:DCB9-$DCC0</c>.</summary>
    private static readonly ushort[] StalkYOffsets = [0xffc0, 0xffc8, 0xffd0, 0xffd8];

    /// <summary>The four ceiling-emitter X coordinates at <c>$86:DCE6-$DCED</c>.</summary>
    private static readonly ushort[] SpawnerXPositions = [0x0020, 0x0060, 0x00a0, 0x00e0];

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

    internal static ushort StalkYOffset(ushort spawnArgument) =>
        StalkYOffsets[SpawnIndex(spawnArgument)];

    internal static ushort SpawnerX(ushort spawnArgument) =>
        SpawnerXPositions[SpawnIndex(spawnArgument)];

    internal static SporeSpawnMovementDelta MovementAt(byte offset) =>
        new(Movement[offset], Movement[unchecked((byte)(offset + 1))]);

    private static int SpawnIndex(ushort spawnArgument)
    {
        if (spawnArgument > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        return spawnArgument;
    }
}
