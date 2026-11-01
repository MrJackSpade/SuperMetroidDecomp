namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// One requested drop from a Metroid's death. Actual pickup
/// actors are spawned into the shared enemy-projectile pool. This empty marker counts
/// each of the five requests; it does not retain the pickup's RNG-scattered coordinates.
/// </summary>
public readonly record struct MetroidDropRequest();
