using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// What the HUD shows (#1466): its images, each placed at the screen pixel its corner shows at, in the order they are
/// drawn, a later image covering an earlier one where they meet. Each image is opaque over the pixels it covers.
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
            if (placed.Corner.X >= GameplayHudDefinitions.WidthPixels || placed.Corner.Y >= GameplayHudDefinitions.HeightPixels)
                throw new ArgumentException($"{placed.Image.FrameName} is placed outside the HUD.", nameof(images));
        }
        Images = images;
    }

    /// <summary>A HUD that shows nothing: no artwork is bound.</summary>
    public static ShownHud Nothing { get; } = new([]);

    /// <summary>The placed images, earliest first.</summary>
    public ImmutableArray<PlacedHudImage> Images { get; }

    /// <summary>
    /// The HUD as the backend draws it: each placed image's installed picture at the screen pixel its corner shows at, in
    /// drawing order (#1466, #1514).
    /// </summary>
    /// <param name="pictureOf">The installed picture of each image, under the palette the HUD shows in.</param>
    /// <param name="shading">The shading a frontend screen shows every picture in (#1512), or null for gameplay's HUD, which shows its frame's HUD shading.</param>
    /// <exception cref="ArgumentException">A picture runs past the HUD's edge.</exception>
    public PicturedHud Pictured(Func<HudImage, SpriteSheetFrame> pictureOf, PictureShading? shading = null)
    {
        Ensure.NotNull(pictureOf);
        return new PicturedHud([.. Images.Select(placed => new PlacedHudPicture(pictureOf(placed.Image), placed.Corner, shading))]);
    }
}
