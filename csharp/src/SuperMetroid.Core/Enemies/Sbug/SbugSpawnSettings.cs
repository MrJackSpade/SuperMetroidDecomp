namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>How an Sbug swarms (#1472, #1519).</summary>
/// <param name="Speed">The speed its headings are scaled by.</param>
/// <param name="InitialAngle">The heading it starts on.</param>
/// <param name="ActivationRadius">The distance in pixels Samus must come within on both axes to set it moving.</param>
/// <param name="Behavior">How it moves once activated.</param>
internal sealed record SbugSpawnSettings(int Speed, RotationAngle InitialAngle, int ActivationRadius, SbugActivationBehavior Behavior) : EnemySpawnSettings;
