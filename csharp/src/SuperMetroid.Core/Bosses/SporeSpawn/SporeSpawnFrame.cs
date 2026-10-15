namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One Spore Spawn body frame: what it shows and its pose.</summary>
/// <param name="Kind">What the frame shows.</param>
/// <param name="Pose">The opening or open pose; zero for the husk and the closed head.</param>
internal readonly record struct SporeSpawnFrame(SporeSpawnFrameKind Kind, int Pose);
