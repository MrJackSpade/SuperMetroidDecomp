using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured pictures of gameplay's HUD in one tileset's rooms (#1466, #1731): each of the HUD's images as
/// its own frame, named by the image's frame name, coloured with the tileset's standard colours and pre-coloured in the
/// shades the HUD shows there, a door's fades and the room effects' palette blends. The HUD draws from this sheet alone;
/// the room effect's pictures are a sheet of their own.
/// </summary>
public sealed class HudPictureSheet
{
    /// <summary>Keeps <paramref name="tileset"/>'s pictures of the HUD.</summary>
    /// <param name="tileset">The tileset whose rooms show the HUD in these colours.</param>
    /// <param name="sheet">The pictures.</param>
    public HudPictureSheet(TilesetDefinition tileset, SpriteSheet sheet)
    {
        Tileset = tileset;
        Sheet = Ensure.NotNull(sheet);
    }

    /// <summary>The tileset whose rooms show the HUD in these colours.</summary>
    public TilesetDefinition Tileset { get; }

    /// <summary>The installed name.</summary>
    public string Name => NameOf(Tileset);

    /// <summary>The pictures.</summary>
    public SpriteSheet Sheet { get; }

    /// <summary>The installed name of the HUD's pictures in <paramref name="tileset"/>'s rooms.</summary>
    public static string NameOf(TilesetDefinition tileset) => $"Hud.{RoomBlockSheet.NameOf(tileset)}";
}
