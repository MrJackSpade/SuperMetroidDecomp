using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;

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

    /// <summary>The HUD as the backend draws it: each placed image's installed picture, in drawing order (#1466).</summary>
    /// <param name="pictureOf">The installed picture of each image, under the palette the HUD shows in.</param>
    /// <exception cref="ArgumentException">A picture is not whole cells, or runs past the HUD's edge.</exception>
    public PicturedHud Pictured(Func<HudImage, SpriteSheetFrame> pictureOf)
    {
        Ensure.NotNull(pictureOf);
        return new PicturedHud([.. Images.Select(placed => new PlacedHudPicture(pictureOf(placed.Image), placed.Cell))]);
    }
}
