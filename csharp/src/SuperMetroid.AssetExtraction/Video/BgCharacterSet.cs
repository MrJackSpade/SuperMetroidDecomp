namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// A set of background characters a layer reads (#1406): what a tile word's character number names. Artwork binds over a
/// set's characters, and a layer's pictures name what is bound under the characters they read.
/// </summary>
public enum BgCharacterSet
{
    /// <summary>
    /// The characters BG1's and BG2's blocks and tile words name: the room tileset's, the common room elements', the
    /// collectibles' and what bosses bind among them.
    /// </summary>
    Tiles,

    /// <summary>The characters layer 3 names: the HUD's, the minimap's and the room effects' glyphs.</summary>
    Layer3,
}
