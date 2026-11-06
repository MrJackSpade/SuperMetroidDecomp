namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Which part of a Magdollite a placement is, and how its head rises (#1472, #1519).</summary>
/// <param name="Part">The part.</param>
/// <param name="Speed">The speed, in pixels per update, at which the head rises and sinks.</param>
/// <param name="Range">The horizontal distance in pixels Samus must come within.</param>
internal sealed record MagdolliteSpawnSettings(MagdollitePart Part, SignedSixteenSixteen Speed, int Range) : EnemySpawnSettings;
