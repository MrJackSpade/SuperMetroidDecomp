namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Whether an Evir placement is its body or its arms, and how the body bobs (#1472, #1519).</summary>
/// <param name="Part">Which part of the Evir it is.</param>
/// <param name="Speed">The speed the body bobs at, as a downward displacement per update; it rises at the same speed.</param>
/// <param name="DirectionChangePeriod">Updates between the body's direction changes.</param>
internal sealed record EvirSpawnSettings(EvirBodyPart Part, SignedSixteenSixteen Speed, int DirectionChangePeriod) : EnemySpawnSettings;
