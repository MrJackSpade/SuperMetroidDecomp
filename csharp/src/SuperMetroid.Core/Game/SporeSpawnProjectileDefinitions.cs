namespace SuperMetroid.Core.Game;

/// <summary>One signed pixel delta pair from Spore Spawn's wrapped movement stream.</summary>
/// <param name="X">Signed horizontal pixel step before the spore's facing direction is applied.</param>
/// <param name="Y">Signed vertical pixel step read alongside the horizontal step.</param>
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

    /// <summary>Reads the signed horizontal and vertical pixel steps at a position in the wrapping movement stream.</summary>
    /// <param name="offset">Byte offset into the 256-entry stream; the following byte wraps at the end.</param>
    /// <returns>The paired movement deltas consumed by the spore pre-instruction.</returns>
    internal static SporeSpawnMovementDelta MovementAt(byte offset) =>
        new(Movement[offset], Movement[unchecked((byte)(offset + 1))]);

    /// <summary>Validates and returns the zero-based index of one of Spore Spawn's four child emitters.</summary>
    /// <param name="spawnArgument">Native spawn argument selecting a stalk segment or ceiling emitter.</param>
    /// <returns>An index from zero through three.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The spawn argument does not select one of the four defined children.</exception>
    private static int SpawnIndex(ushort spawnArgument)
    {
        if (spawnArgument > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        return spawnArgument;
    }
}
