namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Which flames a Norfair Rio's flame follower shows (#2450): rising flames while they sit level with or below the Rio,
/// falling flames while they trail above it. Its values are arbitrary.
/// </summary>
internal enum NorfairRioFlames
{
    /// <summary>The flames rise, level with or below the Rio.</summary>
    Ascending,

    /// <summary>The flames fall, trailing above the Rio.</summary>
    Descending,
}
