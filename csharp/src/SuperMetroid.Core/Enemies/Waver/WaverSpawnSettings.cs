namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Which way a Waver first flies (#1472).</summary>
/// <param name="Facing">The way it first flies.</param>
internal sealed partial record WaverSpawnSettings(HorizontalDirection Facing) : EnemySpawnSettings;
