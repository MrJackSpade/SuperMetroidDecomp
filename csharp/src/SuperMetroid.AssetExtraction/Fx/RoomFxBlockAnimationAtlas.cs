namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed two-bit characters of the simple room-FX, Tourian statue and Wrecked Ship treadmill pictures (#1409),
/// read back from the installed atlas so the import composes room sheets and the Tourian statues' pictures from them
/// (#1527). The game draws the composed sheets and never reads these characters.
/// </summary>
public sealed class RoomFxBlockAnimationAtlas
{
    /// <summary>Concatenated edited tile bytes in segment order.</summary>
    internal readonly byte[] characters;

    /// <summary>Accepts the compiled character strip only when its length matches the atlas's ordered frame and statue strip.</summary>
    internal RoomFxBlockAnimationAtlas(byte[] characters)
    {
        this.characters = characters;
        if (characters.Length != RoomFxBlockAnimationAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Room-FX artwork has {characters.Length} bytes, expected {RoomFxBlockAnimationAtlasFormat.TotalByteCount}.");
    }
}
