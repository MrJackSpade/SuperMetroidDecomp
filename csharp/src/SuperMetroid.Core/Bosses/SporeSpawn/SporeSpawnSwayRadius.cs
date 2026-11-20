namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// How wide Spore Spawn sways (#2334): how far either side of his sway's centre he swings across, in room pixels; he
/// swings up and down less, by <see cref="SporeSpawnArenaDefinitions.VerticalSwayNarrowing"/>. His descent and his
/// fight's start set it, and each closing of his shell widens a narrow sway (<see cref="Widened"/>), so it never passes
/// the widest his fight's start sets.
/// </summary>
/// <param name="Pixels">How far either side of his sway's centre he swings across, in room pixels.</param>
internal readonly record struct SporeSpawnSwayRadius(int Pixels)
{
    /// <summary>How far either side of his sway's centre he swings on each axis: across, and less up and down.</summary>
    internal Extent16<RoomPixels> Reach => new(Pixels, Pixels - SporeSpawnArenaDefinitions.VerticalSwayNarrowing);

    /// <summary>The sway a closing of his shell leaves: wider by <see cref="SporeSpawnArenaDefinitions.SwayWidening"/> while narrower than <see cref="SporeSpawnArenaDefinitions.WidenedSwayLimit"/>, otherwise as it was.</summary>
    internal SporeSpawnSwayRadius Widened() =>
        Pixels < SporeSpawnArenaDefinitions.WidenedSwayLimit ? new(Pixels + SporeSpawnArenaDefinitions.SwayWidening) : this;
}
