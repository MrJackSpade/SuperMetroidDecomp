using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed pictures of the HUD's images under one palette (#1466): the frames of one sheet the import pictured the
/// HUD's images in, looked up by each image's name once.
/// </summary>
/// <param name="sheet">The sheet picturing the HUD's images.</param>
public sealed class HudPictures(SpriteSheet sheet)
{
    /// <summary>Each image's picture, once looked up.</summary>
    private readonly Dictionary<HudImage, SpriteSheetFrame> pictures = [];

    /// <summary>The sheet picturing the HUD's images.</summary>
    public SpriteSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>The picture of <paramref name="image"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet does not picture the image.</exception>
    public SpriteSheetFrame PictureOf(HudImage image)
    {
        Ensure.NotNull(image);
        if (!pictures.TryGetValue(image, out SpriteSheetFrame? picture))
            pictures.Add(image, picture = Sheet.Frame(image.FrameName));
        return picture;
    }

    /// <summary>The picture each of <paramref name="hud"/>'s cells shows under this sheet's palette, as <see cref="ShownHud.CellPictures"/> lays them out.</summary>
    public ImmutableArray<BgTilePicture?> CellPictures(ShownHud hud) => Ensure.NotNull(hud).CellPictures(PictureOf);
}
