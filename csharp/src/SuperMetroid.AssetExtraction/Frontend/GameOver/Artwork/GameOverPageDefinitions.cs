namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// The game-over page as the cartridge lays it out (#1467): its size and the menu atlas cells its text shows. Only the
/// import reads them; it pictures the page once into the menus' picture sheet, and the game places that picture.
/// </summary>
public static class GameOverPageDefinitions
{
    /// <summary>Width of the game-over page in tile cells.</summary>
    public const int TilemapWidth = 32;
    /// <summary>Height of the complete game-over page in tile cells, including rows outside the visible viewport.</summary>
    public const int TilemapHeight = 32;
    /// <summary>1024 row-major background tile cells in the complete 32-by-32 game-over page.</summary>
    public const int TilemapCellCount = TilemapWidth * TilemapHeight;
    /// <summary>The menu atlas cell of the blank glyph, shown by every page cell no text element writes.</summary>
    public const ushort BlankTile = 15;
}
