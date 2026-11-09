namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where each native bank-$84 list of one facing of blue cap begins (#1554).</summary>
/// <param name="Opening">The shot cap opening and its delete.</param>
/// <param name="Closing">The closing behind Samus, which falls into <paramref name="Closed"/>.</param>
/// <param name="Closed">The block made shootable again, the closed image and the delete.</param>
internal sealed record NativeBlueDoorLists(ushort Opening, ushort Closing, ushort Closed);
