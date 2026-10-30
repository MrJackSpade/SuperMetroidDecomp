namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The suit pickup's light beam's inclusive left and right screen columns. A left edge right of the right edge lights
/// nothing.
/// </summary>
/// <param name="Left">The inclusive left column, 0 through 255.</param>
/// <param name="Right">The inclusive right column, 0 through 255.</param>
public readonly record struct SuitPickupBeamEdges(int Left, int Right);
