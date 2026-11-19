namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>How a Skultera swims (#1472, #1519).</summary>
/// <param name="Speed">Its swimming speed in pixels per update; it swims equally fast either way.</param>
/// <param name="StartDirection">The way it starts swimming.</param>
/// <param name="Radius">The height of its wave, in pixels.</param>
/// <param name="AngleStep">How far along its wave it moves each update.</param>
internal sealed record SkulteraSpawnSettings(SignedSixteenSixteen Speed, HorizontalDirection StartDirection, int Radius, RotationAngle AngleStep) : EnemySpawnSettings;
