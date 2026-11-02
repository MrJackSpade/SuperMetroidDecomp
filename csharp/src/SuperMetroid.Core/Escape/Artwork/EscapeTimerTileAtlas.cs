using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// The installed four-bit OBJ characters the Ceres and Zebes escape timers show (#1409), compiled by the import pipeline
/// from the editable timer typeface.
/// </summary>
public class EscapeTimerTileAtlas
{
    /// <summary>Retains the compiled timer characters.</summary>
    /// <param name="characters">The 25 timer characters in transfer order.</param>
    internal EscapeTimerTileAtlas(byte[] characters) =>
        Artwork = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles), characters);

    /// <summary>The 25 timer characters as owned sprite artwork, in transfer order: character 0 is OBJ character $1E0.</summary>
    public SpriteArtwork Artwork { get; }
}
