namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The suit pickup's light beam's inclusive left and right screen columns. A left edge right of the right edge lights
/// nothing.
/// </summary>
/// <param name="Left">The inclusive left screen column, across the screen's width.</param>
/// <param name="Right">The inclusive right screen column, across the screen's width.</param>
public readonly record struct SuitPickupBeamEdges(int Left, int Right);
