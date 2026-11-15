namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Which part of a Magdollite a placement is, and, for its head, how it rises (#1472, #1519, #2023).</summary>
/// <param name="Part">The part.</param>
/// <param name="Rise">How the head rises; only a head placement has one, which its arm also reads.</param>
internal sealed record MagdolliteSpawnSettings(MagdollitePart Part, MagdolliteHeadRise? Rise) : EnemySpawnSettings;
