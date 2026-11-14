using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnProjectileDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Moves Spore Spawn's spores along the wrapped drift path they follow.</summary>
internal static class SporeSpawnProjectiles
{
    /// <summary>The signed pixel movement of a spore's drift at <paramref name="step"/>.</summary>
    /// <param name="step">The spore's step along its drift path, 0 through <see cref="DriftStepCount"/> minus one.</param>
    internal static Offset16<RoomPixels> DriftAt(int step) => Ensure.Element(Drift, step);

    /// <summary>How many steps a spore's drift path holds before it repeats.</summary>
    internal static int DriftStepCount => Drift.Length;
}
