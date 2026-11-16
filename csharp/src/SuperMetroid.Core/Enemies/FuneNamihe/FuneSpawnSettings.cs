namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>How a Fune faces, waits and spits (#1472, #1519, #2063).</summary>
/// <param name="Facing">The way it faces.</param>
/// <param name="CooldownTime">The updates it waits between spits.</param>
/// <param name="LaunchSpeed">The speed, in pixels per update, at which its fireballs fly away from it.</param>
internal sealed record FuneSpawnSettings(HorizontalDirection Facing, int CooldownTime, SignedEightEight LaunchSpeed) : EnemySpawnSettings;
