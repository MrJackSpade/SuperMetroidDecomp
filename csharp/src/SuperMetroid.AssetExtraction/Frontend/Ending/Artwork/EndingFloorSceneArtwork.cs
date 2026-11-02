
namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// One installed ending backdrop (#1409): its low-byte map, copied into both halves of the full map, and how many Mode 7
/// characters its artwork holds. The installed ending floor sheets picture the characters; their pixels and the map's
/// file belong to the import pipeline that reads the installed artwork.
/// </summary>
public class EndingFloorSceneArtwork
{
    /// <summary>Retains the validated low-byte map and the installed characters' count for one scene.</summary>
    /// <param name="map">The row-ordered tile indices.</param>
    /// <param name="characterCount">The installed Mode 7 characters.</param>
    internal EndingFloorSceneArtwork(byte[] map, int characterCount)
    {
        Map = map;
        CharacterCount = characterCount;
    }

    /// <summary>The 8192 row-ordered tile indices written to the map's low byte lane in both halves.</summary>
    public ReadOnlyMemory<byte> Map { get; }
    /// <summary>The installed Mode 7 characters.</summary>
    public int CharacterCount { get; }
}
