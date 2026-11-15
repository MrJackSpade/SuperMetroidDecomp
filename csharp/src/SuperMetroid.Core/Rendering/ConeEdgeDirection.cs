namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The way one edge of a <see cref="WindowCone"/> leaves its apex (#2009): how far it runs right across the screen and
/// down it for a step of <see cref="WindowCone.EdgeLength"/>, in whole numbers from the game's fine sine, so every device
/// tests a pixel against the edge with the same exact arithmetic.
/// </summary>
/// <param name="Across">How far the edge runs right; negative to the left.</param>
/// <param name="Down">How far the edge runs down; negative upward.</param>
public readonly record struct ConeEdgeDirection(int Across, int Down);
