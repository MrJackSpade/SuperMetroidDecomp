namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Where a dust cloud or explosion lands around the point it is spawned at, scattered by one random sample.</summary>
/// <param name="XScatter">The part of the random sample that scatters the X axis across the placement's span.</param>
/// <param name="YScatter">The part of the random sample, apart from the X axis's, that scatters the Y axis across the placement's span.</param>
/// <param name="Base">Signed offset added before the random scatter.</param>
internal readonly record struct DustExplosionPlacement(
    RandomSampleField XScatter,
    RandomSampleField YScatter,
    Offset16<RoomPixels> Base);
