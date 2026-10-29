namespace SuperMetroid.Core.Hud;

/// <summary>A HUD image placed on the HUD (#1466).</summary>
/// <param name="Image">The image.</param>
/// <param name="Cell">The HUD cell its upper-left corner sits in: row zero is the static top row, the drawn rows below it.</param>
public readonly record struct PlacedHudImage(HudImage Image, Position16<TilemapCells> Cell);
