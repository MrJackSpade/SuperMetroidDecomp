namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The way a baby turtle first crawls (#1472, #1519, #2338).</summary>
/// <param name="Heading">The way it crawls from its spawn.</param>
internal sealed record BabyTurtleSpawnSettings(HorizontalDirection Heading) : EnemySpawnSettings;
