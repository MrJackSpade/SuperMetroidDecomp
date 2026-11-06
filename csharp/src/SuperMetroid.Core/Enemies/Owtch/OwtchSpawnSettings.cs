namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>How an Owtch patrols (#1472, #1519).</summary>
/// <param name="InitialBehavior">What it is doing when it spawns: walking either way, buried, sinking or rising.</param>
/// <param name="UndergroundWait">The updates it stays buried each time it sinks.</param>
/// <param name="Speed">The speed, in pixels per update, at which it walks.</param>
/// <param name="TravelDistance">The pixels it walks to either side of its spawn position before turning.</param>
internal sealed record OwtchSpawnSettings(OwtchBehaviorState InitialBehavior, int UndergroundWait, SignedSixteenSixteen Speed, int TravelDistance) : EnemySpawnSettings;
