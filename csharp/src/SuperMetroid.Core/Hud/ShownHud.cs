using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// What the HUD shows (#1466): its images, each placed on the HUD's four rows, in the order they are drawn, a later image
/// covering an earlier one where they meet. Each image is opaque over the cells it covers.
/// </summary>
public sealed class ShownHud
{
    /// <summary>Keeps the placements in drawing order.</summary>
    /// <param name="images">The placed images, earliest first.</param>
    public ShownHud(ImmutableArray<PlacedHudImage> images)
    {
        if (images.IsDefault)
            throw new ArgumentException("A shown HUD needs its placements.", nameof(images));
        foreach (PlacedHudImage placed in images)
        {
            _ = Ensure.NotNull(placed.Image);
            if (placed.Cell.X >= GameplayHudDefinitions.Width || placed.Cell.Y >= GameplayHudDefinitions.Rows)
                throw new ArgumentException($"{placed.Image.FrameName} is placed outside the HUD.", nameof(images));
        }
        Images = images;
    }

    /// <summary>A HUD that shows nothing: no artwork is bound.</summary>
    public static ShownHud Nothing { get; } = new([]);

    /// <summary>The placed images, earliest first.</summary>
    public ImmutableArray<PlacedHudImage> Images { get; }

    /// <summary>
    /// This HUD's static top row with <paramref name="drawn"/>'s drawn rows: what a screen keeping this HUD resident shows
    /// once it mirrors the rows gameplay draws again, leaving the top row it kept.
    /// </summary>
    /// <param name="drawn">The HUD gameplay shows now.</param>
    public ShownHud WithDrawnRowsOf(ShownHud drawn) => new([
        .. Images.Where(placed => placed.Cell.Y == 0),
        .. Ensure.NotNull(drawn).Images.Where(placed => placed.Cell.Y != 0),
    ]);

    /// <summary>
    /// The picture each of the HUD's cells shows, row by row across its four rows: the part of the last image placed over
    /// the cell, or null where no image is.
    /// </summary>
    /// <param name="pictureOf">The installed picture of each image, under the palette the HUD shows in.</param>
    /// <exception cref="InvalidDataException">A picture is not whole cells, or runs past the HUD's edge.</exception>
    public ImmutableArray<BgTilePicture?> CellPictures(Func<HudImage, SpriteSheetFrame> pictureOf)
    {
        Ensure.NotNull(pictureOf);
        const int cell = GameplayHudDefinitions.CellPixels;
        var cells = new BgTilePicture?[GameplayHudDefinitions.Width * GameplayHudDefinitions.Rows];
        foreach (PlacedHudImage placed in Images)
        {
            SpriteSheetFrame picture = pictureOf(placed.Image);
            if (picture.Size.X % cell != 0 || picture.Size.Y % cell != 0)
                throw new InvalidDataException($"HUD picture {placed.Image.FrameName} is not whole cells.");
            int columns = picture.Size.X / cell, rows = picture.Size.Y / cell;
            if (placed.Cell.X + columns > GameplayHudDefinitions.Width || placed.Cell.Y + rows > GameplayHudDefinitions.Rows)
                throw new InvalidDataException($"HUD picture {placed.Image.FrameName} runs past the HUD's edge.");
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                cells[(placed.Cell.Y + y) * GameplayHudDefinitions.Width + placed.Cell.X + x] =
                    new BgTilePicture(picture, new Position16<ImagePixels>(checked((ushort)(x * cell)), checked((ushort)(y * cell))));
        }
        return [.. cells];
    }
}
