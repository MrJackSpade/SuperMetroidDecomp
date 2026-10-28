namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One run of tile numbers a Ceres floor animation frame draws onto the rotating floor.</summary>
/// <param name="FloorCell">
/// The floor cell the first tile is drawn at; each tile replaces that cell's character, keeping the characters' pixels.
/// </param>
/// <param name="TileNumbers">Ordered eight-bit character selectors for consecutive floor cells, not character pixel bytes.</param>
public readonly record struct CeresFloorTileRun(int FloorCell, CeresFloorAnimation.TileSequence TileNumbers);
