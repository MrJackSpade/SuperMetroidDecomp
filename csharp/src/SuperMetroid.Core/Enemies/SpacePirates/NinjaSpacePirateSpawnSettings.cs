namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a ninja space pirate patrols (#1472).</summary>
/// <param name="Options">Its options.</param>
/// <param name="PostDistance">Pixels, 0 through 65535, from its placement to each post.</param>
internal sealed record NinjaSpacePirateSpawnSettings(NinjaSpacePirateParameterFlags Options, int PostDistance) : EnemySpawnSettings
{
    /// <summary>Pixels, 0 through 65535, from its placement to each post.</summary>
    public int PostDistance { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, PostDistance);
}
