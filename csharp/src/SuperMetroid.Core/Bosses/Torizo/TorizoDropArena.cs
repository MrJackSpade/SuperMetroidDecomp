namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The stretch of a Torizo's room its death drops scatter over (#2032): each drop lands at the corner plus the two scatters
/// one random sample draws.
/// </summary>
/// <param name="Corner">The arena's top-left corner, in room pixels.</param>
/// <param name="HorizontalScatter">The sample's field that scatters each drop rightward across the arena.</param>
/// <param name="VerticalScatter">The sample's field that scatters each drop downward through the arena.</param>
internal readonly record struct TorizoDropArena(Position16<RoomPixels> Corner, RandomSampleField HorizontalScatter, RandomSampleField VerticalScatter);
