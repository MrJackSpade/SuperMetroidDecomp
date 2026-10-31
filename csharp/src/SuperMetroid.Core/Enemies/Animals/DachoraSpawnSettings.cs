namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Which way a Dachora faces and whether it is one of its speed-boost echoes (#1472).</summary>
/// <param name="Facing">The way it faces.</param>
/// <param name="Echo">Whether it is a speed-boost echo.</param>
internal sealed record DachoraSpawnSettings(HorizontalDirection Facing, bool Echo) : EnemySpawnSettings;
