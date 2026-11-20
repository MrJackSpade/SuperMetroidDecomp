namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A band below Ridley's full health and the health below which he is in it.</summary>
/// <param name="HealthBelow">The health below which Ridley is in <paramref name="Band"/>.</param>
/// <param name="Band">The band his health puts him in.</param>
internal readonly record struct NorfairRidleyWoundBand(int HealthBelow, NorfairRidleyHealthBand Band);
