using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Places Spore Spawn's spores: the stalk and ceiling emitters they start from and the wrapped movement stream they follow.</summary>
internal static class SporeSpawnProjectiles
{
    /// <summary>Four stalk segments spaced eight pixels apart, starting 64 pixels above the owner.</summary>
    internal static int StalkYOffset(int spawnArgument) =>
        CoordinateAxis.Wrap(-64 + 8 * SpawnIndex(spawnArgument));

    /// <summary>Four ceiling emitters centered in successive 64-pixel room columns.</summary>
    internal static int SpawnerX(int spawnArgument) =>
        CoordinateAxis.Wrap(32 + 64 * SpawnIndex(spawnArgument));

    /// <summary>The signed pixel movement of a spore's drift at <paramref name="step"/>.</summary>
    /// <param name="step">The spore's step along its drift path, 0 through <see cref="DriftStepCount"/> minus one.</param>
    internal static Offset16<RoomPixels> DriftAt(int step) => Ensure.Element(Drift, step);

    /// <summary>How many steps a spore's drift path holds before it repeats.</summary>
    internal static int DriftStepCount => Drift.Length;

    /// <summary>Accepts only the four stalk or ceiling-emitter ordinals the spawns use.</summary>
    private static int SpawnIndex(int spawnArgument)
    {
        if (spawnArgument is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        return spawnArgument;
    }
}
