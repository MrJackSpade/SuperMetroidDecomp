using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The looks enemy encounters show after their rooms load, and the pictures of frames drawn over them (#1392, #1406, #1463):
/// the installed sheets hold a picture of each such frame for each look that can show in each region it reads. Where each
/// look lands among the sprites' artwork is the import pipeline's knowledge; this names looks and pictures only.
/// </summary>
public interface IEncounterLookCatalog : ISpriteArtworkOverlap
{
    /// <summary>The looks the encounters of graphics set <paramref name="set"/>'s members show, in catalogue order.</summary>
    IReadOnlyList<SpriteArtworkSource> EncounterLooks(RoomEnemyGraphicsSetDefinition set);

    /// <summary>Whether some catalogued encounter shows <paramref name="look"/>.</summary>
    bool IsEncounterLook(SpriteArtworkSource look);

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    string PictureName(string frame, int layer, IReadOnlyList<LookRegionContent> shown);

    /// <summary>
    /// The artwork the Ceres Ridley arena's backgrounds show, which point at the sprites' artwork (#1392, #1422): what
    /// <paramref name="looks"/> leave visible there.
    /// </summary>
    IBgCharacterArtwork ObjectPage(ShownSpriteArtwork looks);
}
