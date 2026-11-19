namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>A Norfair pipe bug formation's follower's placement: its speed; it rises when its leader's Samus comes near.</summary>
/// <param name="Speed">The speed at which it flies across the room.</param>
internal sealed record NorfairPipeBugFollowerSpawnSettings(SignedSixteenSixteen Speed) : NorfairPipeBugSpawnSettings(Speed);
