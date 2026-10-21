namespace SuperMetroid.Verification.Hardware;

/// <summary>The name and raw of one map cell round trip.</summary>
/// <param name="Name">The name.</param>
/// <param name="Raw">The raw.</param>
internal readonly record struct MapCellRoundTrip(string Name, ushort Raw);
