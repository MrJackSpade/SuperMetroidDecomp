namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One randomized room-coordinate placement record for misc dust.</summary>
/// <param name="XScatter">The random field of the sample's low byte that scatters the X axis across the placement's span.</param>
/// <param name="YScatter">The random field of the sample's high byte that scatters the Y axis across the placement's span.</param>
/// <param name="Base">Signed offset added before the random scatter.</param>
internal readonly record struct MiscDustPlacementDefinition(
    RandomSampleField XScatter,
    RandomSampleField YScatter,
    Offset16<RoomPixels> Base);
