namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a ninja space pirate patrols (#1472, #1519).</summary>
/// <param name="Options">Its options.</param>
/// <param name="PostDistance">The pixels from its placement to each post.</param>
internal sealed record NinjaSpacePirateSpawnSettings(NinjaSpacePirateOptions Options, int PostDistance) : EnemySpawnSettings;
