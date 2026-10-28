namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The installed map and menu sprites (#1409, #1463). The game draws them from the installed map sprite sheet; their
/// compositions, and the menu OBJ characters the menu sheets are composed over, belong to the import pipeline that reads
/// the installed artwork.
/// </summary>
public abstract class MapSpriteCompositions
{
    /// <summary>Restricts the catalog to the import pipeline's reader.</summary>
    internal MapSpriteCompositions()
    {
    }
}
