using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The characters animations paint in one tileset's rooms (#1406), as asset extraction declared them: those of the room
/// tileset's blocks and the words written over BG1 and BG2, those of the OBJ character page a room's blocks may read
/// instead, and BG3's.
/// </summary>
/// <param name="Blocks">The runs of the tileset's BG1 and BG2 characters animations paint.</param>
/// <param name="ObjectPage">The runs of the OBJ character page animations paint, for rooms whose blocks read it.</param>
/// <param name="Bg3">The runs of BG3's characters animations paint, numbered from the HUD's character base.</param>
public sealed record RoomAnimatedCharacters(AnimatedCharacters Blocks, AnimatedCharacters ObjectPage, AnimatedCharacters Bg3)
{
    /// <summary>A tileset no animation paints characters of.</summary>
    public static RoomAnimatedCharacters None { get; } = new(AnimatedCharacters.None, AnimatedCharacters.None, AnimatedCharacters.None);

    /// <summary>The runs animations paint of the characters a room of <paramref name="characters"/> reads its BG1 and BG2 from.</summary>
    /// <exception cref="InvalidOperationException">The page is undefined.</exception>
    public AnimatedCharacters Of(RoomBackgroundCharacters characters) => characters switch
    {
        RoomBackgroundCharacters.Tileset => Blocks,
        RoomBackgroundCharacters.ObjectCharacterPage => ObjectPage,
        _ => throw new InvalidOperationException($"Undefined {nameof(RoomBackgroundCharacters)} {characters}."),
    };
}
