namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair pipe bug formation's leader's placement: the width Samus must come within and its speed.</summary>
/// <param name="TriggerWidth">The horizontal distance in pixels Samus must come within for the formation to rise.</param>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal sealed record NorfairPipeBugLeaderSpawnSettings(int TriggerWidth, SignedSixteenSixteen Speed) : NorfairPipeBugSpawnSettings(Speed);
