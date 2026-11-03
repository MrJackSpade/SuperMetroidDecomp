using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The colours a frontend scene's pictures show (#1512), for checks that compared a screen's colour memory: each distinct
/// picture and shading the scene draws, in the order the scene first draws it, as the native 256-colour image it shows
/// (<see cref="NativeColouredPictures.ShownIn"/>). A scene's pictures name their shadings; the frame holds no colours of
/// its own besides its backdrop.
/// </summary>
internal static class SceneShownColours
{
    /// <summary>The native image each distinct picture and shading of <paramref name="scene"/> shows, layers first, then sprite draws.</summary>
    /// <exception cref="InvalidOperationException">
    /// A picture names no shading, or a layer shows pictures gameplay colours by group, which a frontend scene has none of.
    /// </exception>
    internal static IReadOnlyList<RgbColor[]> Of(LayeredScene scene) =>
        [.. Pictures(scene).Select(entry => NativeColouredPictures.ShownIn(entry.Picture, entry.Shading))];

    /// <summary>
    /// The colours <paramref name="scene"/>'s pictures show at <paramref name="place"/> of the native 256-colour image, one
    /// for each distinct picture and shading with an ink coloured from that place; empty where no picture has one.
    /// </summary>
    internal static RgbColor[] ShownAt(LayeredScene scene, int place)
    {
        Ensure.BetweenInclusive(0, NativeColouredPictures.ColorCount - 1, place);
        var shown = new List<RgbColor>();
        foreach ((SpriteImage picture, PictureShading shading) in Pictures(scene))
        {
            RgbColor[] inks = shading.InkColours(picture);
            for (int ink = 0; ink < inks.Length; ink++)
            {
                if (SuperMetroid.AssetExtraction.Artwork.InstalledPictureColours.CartridgeColourOf(picture, ink) == place)
                    shown.Add(inks[ink]);
            }
        }
        return [.. shown];
    }

    /// <summary>Each distinct picture and shading <paramref name="scene"/> draws, layers first, then sprite draws.</summary>
    /// <exception cref="InvalidOperationException">
    /// A picture names no shading, or a layer shows pictures gameplay colours by group, which a frontend scene has none of.
    /// </exception>
    private static List<(SpriteImage Picture, PictureShading Shading)> Pictures(LayeredScene scene)
    {
        Ensure.NotNull(scene);
        var shown = new List<(SpriteImage Picture, PictureShading Shading)>();
        foreach (DisplayLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case PicturedPlaneLayer plane:
                    foreach (PlacedPlanePicture placed in plane.Plane.Pictures)
                        Add(shown, placed.Picture.Sheet, placed.Shading);
                    break;
                case PictureGridLayer grid:
                    foreach (GridPicture? cell in grid.Grid.Cells)
                    {
                        if (cell is { } picture)
                            Add(shown, picture.Picture.Sheet, picture.Shading);
                    }
                    break;
                case HudLayer hud:
                    foreach (PlacedHudPicture placed in hud.Hud.Pictures)
                        Add(shown, placed.Picture.Sheet, placed.Shading);
                    break;
                case TurningFloorLayer floor:
                    foreach (SpriteSheetFrame picture in floor.Floor.Pictures)
                        Add(shown, picture.Sheet, floor.Floor.Shading);
                    break;
                case GameplaySceneLayer or GameplayBlendLayer or MessageBoxLayer or OverlayBlendLayer or TurningRoomLayer or InsetSceneLayer:
                    throw new InvalidOperationException($"Layer {layer.GetType().Name} shows pictures a frontend scene's colours do not describe.");
            }
        }
        foreach (SpriteDraw draw in scene.Paint.SpriteDraws)
        {
            var image = (ImageSpriteDraw)draw;
            Add(shown, image.Image.Sheet, image.Shading);
        }
        return shown;
    }

    /// <summary>Whether two scenes' pictures show the same colours, picture by picture.</summary>
    internal static bool Same(IReadOnlyList<RgbColor[]> first, IReadOnlyList<RgbColor[]> second)
    {
        Ensure.NotNull(first);
        Ensure.NotNull(second);
        return first.Count == second.Count && first.Zip(second).All(pair => pair.First.SequenceEqual(pair.Second));
    }

    /// <summary>Adds <paramref name="picture"/> under <paramref name="shading"/> unless the scene showed it so already.</summary>
    /// <exception cref="InvalidOperationException">The picture names no shading.</exception>
    private static void Add(List<(SpriteImage Picture, PictureShading Shading)> shown, SpriteImage picture, PictureShading? shading)
    {
        PictureShading named = shading ?? throw new InvalidOperationException($"Picture {picture.Name} names the shading it shows in.");
        if (!shown.Exists(entry => ReferenceEquals(entry.Picture, picture) && entry.Shading.Equals(named)))
            shown.Add((picture, named));
    }
}
