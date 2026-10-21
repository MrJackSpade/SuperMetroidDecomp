namespace SuperMetroid.Verification.Harness;

/// <summary>The tile, x and y of one cutout.</summary>
/// <param name="Tile">The tile.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct Cutout(int Tile, int X, int Y);
