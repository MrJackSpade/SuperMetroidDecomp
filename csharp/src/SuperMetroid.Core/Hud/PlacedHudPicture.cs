using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud;

/// <summary>A HUD image's installed picture placed on the HUD (#1466).</summary>
/// <param name="Picture">The image's picture, under the palette the HUD shows in.</param>
/// <param name="Cell">The HUD cell its upper-left corner sits in: row zero is the static top row, the drawn rows below it.</param>
public readonly record struct PlacedHudPicture(SpriteSheetFrame Picture, Position16<TilemapCells> Cell);
