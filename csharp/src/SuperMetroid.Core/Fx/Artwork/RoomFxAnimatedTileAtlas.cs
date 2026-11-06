namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The installed two-bit characters of the simple room-FX, Tourian statue and Wrecked Ship treadmill frames (#1409),
/// compiled by the import pipeline. They remain bytes here because the Tourian statue's sprite character source resolves them.
/// </summary>
public sealed class RoomFxAnimatedTileAtlas
{
    /// <summary>Concatenated edited tile bytes in segment order.</summary>
    internal readonly byte[] characters;

    /// <summary>Accepts the compiled character strip only when its length matches the atlas's ordered frame and statue strip.</summary>
    internal RoomFxAnimatedTileAtlas(byte[] characters)
    {
        this.characters = characters;
        if (characters.Length != RoomFxAnimatedTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Room-FX artwork has {characters.Length} bytes, expected {RoomFxAnimatedTileAtlasFormat.TotalByteCount}.");
    }
}
