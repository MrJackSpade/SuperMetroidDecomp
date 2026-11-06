namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>How fast a turtle first walks and which way (#1472, #1519).</summary>
/// <param name="Speed">Its horizontal speed in pixels per update; negative walks left.</param>
internal sealed record TurtleSpawnSettings(int Speed) : EnemySpawnSettings;
