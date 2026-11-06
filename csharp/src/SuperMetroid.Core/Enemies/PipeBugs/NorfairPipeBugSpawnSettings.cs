namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>How a Norfair pipe bug flies and whether it leads its formation (#1472, #1519).</summary>
/// <param name="TriggerWidth">The horizontal distance in pixels Samus must come within; zero follows the leader.</param>
/// <param name="Speed">The speed, in pixels per update, at which it flies across the room.</param>
internal sealed record NorfairPipeBugSpawnSettings(int TriggerWidth, SignedSixteenSixteen Speed) : EnemySpawnSettings;
