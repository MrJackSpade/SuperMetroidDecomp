namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>Where the Metroid stands and how much health Samus has after one update of the bomb detach trajectory.</summary>
/// <param name="X">The Metroid's x.</param>
/// <param name="Y">The Metroid's y.</param>
/// <param name="Health">Samus's health, which an attached Metroid drains.</param>
internal readonly record struct MetroidBombPlacementExpected(ushort X, ushort Y, ushort Health);
