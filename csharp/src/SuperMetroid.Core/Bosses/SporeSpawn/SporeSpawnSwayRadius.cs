namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// How wide Spore Spawn sways (#2334): how far either side of his sway's centre he swings across, in room pixels; he
/// swings up and down sixteen pixels less. His descent leaves 48, his fight's start sets 64, and each closing of his shell
/// widens a sway narrower than forty by eight, so it stays within 0 through 64.
/// </summary>
/// <param name="Pixels">How far either side of his sway's centre he swings across, in room pixels.</param>
internal readonly record struct SporeSpawnSwayRadius(int Pixels)
{
    /// <summary>How far either side of his sway's centre he swings on each axis: across, and sixteen pixels less up and down.</summary>
    internal Extent16<RoomPixels> Reach => new(Pixels, Pixels - SporeSpawnArenaDefinitions.VerticalSwayNarrowing);

    /// <summary>The sway a closing of his shell leaves: eight pixels wider while narrower than forty, otherwise as it was.</summary>
    internal SporeSpawnSwayRadius Widened() =>
        Pixels < SporeSpawnArenaDefinitions.WidenedSwayLimit ? new(Pixels + SporeSpawnArenaDefinitions.SwayWidening) : this;
}
