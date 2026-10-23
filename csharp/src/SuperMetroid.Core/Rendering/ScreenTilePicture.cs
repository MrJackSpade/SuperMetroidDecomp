namespace SuperMetroid.Core.Rendering;

/// <summary>One character of a frontend background artwork, as its pre-coloured sheet pictures it (#1392).</summary>
/// <param name="Sheet">The artwork's sheet.</param>
/// <param name="SheetTile">The tile within the sheet.</param>
public readonly record struct ScreenTilePicture(ScreenTileSheet Sheet, int SheetTile);
