namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// How a Norfair pipe bug flies and whether it leads its formation (#1472, #1519): the leader and its followers each have
/// settings of their own (#2536), so only the leader carries the width Samus must come within.
/// </summary>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal abstract record NorfairPipeBugSpawnSettings(SignedSixteenSixteen Speed) : EnemySpawnSettings;

/// <summary>A Norfair pipe bug formation's leader's placement: the width Samus must come within and its speed.</summary>
/// <param name="TriggerWidth">The horizontal distance in pixels Samus must come within for the formation to rise.</param>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal sealed record NorfairPipeBugLeaderSpawnSettings(int TriggerWidth, SignedSixteenSixteen Speed) : NorfairPipeBugSpawnSettings(Speed);

/// <summary>A Norfair pipe bug formation's follower's placement: its speed; it rises when its leader's Samus comes near.</summary>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal sealed record NorfairPipeBugFollowerSpawnSettings(SignedSixteenSixteen Speed) : NorfairPipeBugSpawnSettings(Speed);
