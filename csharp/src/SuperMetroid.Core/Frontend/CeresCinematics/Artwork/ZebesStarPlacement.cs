namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One star of the Zebes star grid: its eight-pixel grid cell and tile.</summary>
/// <param name="Cell">The star's offset in eight-pixel grid cells.</param>
/// <param name="Tile">The star's tile number.</param>
internal readonly record struct ZebesStarPlacement(Offset16<TilemapCells> Cell, int Tile);
