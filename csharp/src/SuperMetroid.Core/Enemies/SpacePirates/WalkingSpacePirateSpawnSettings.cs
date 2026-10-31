namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a walking space pirate patrols (#1472).</summary>
/// <param name="Options">Its options.</param>
/// <param name="PatrolReach">Pixels, 0 through 65535, it patrols either side of its placement.</param>
internal sealed record WalkingSpacePirateSpawnSettings(WalkingSpacePirateParameterFlags Options, int PatrolReach) : EnemySpawnSettings
{
    /// <summary>Pixels, 0 through 65535, it patrols either side of its placement.</summary>
    public int PatrolReach { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, PatrolReach);
}
