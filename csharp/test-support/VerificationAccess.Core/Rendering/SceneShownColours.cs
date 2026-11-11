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
    internal static RgbColor[] ShownAt(LayeredScene scene, int place) => [.. SourcesAt(scene, place).Select(source => source.Colour)];

    /// <summary>
    /// What <paramref name="scene"/>'s pictures show at <paramref name="place"/> of the native 256-colour image, with the
    /// picture, the shading and the ink each colour comes from, for a failure that must name what shows it. A layer's
    /// picture shows every ink of its sheet at the ink's own place; a sprite draw shows the inks its frame paints, at the
    /// place of the ink's row with the row its colouring ORs into every part, as the cartridge's frontend draws place
    /// them (#1512).
    /// </summary>
    internal static IReadOnlyList<ShownColourSource> SourcesAt(LayeredScene scene, int place)
    {
        Ensure.BetweenInclusive(0, NativeColouredPictures.ColorCount - 1, place);
        return Sources(scene, place)[place];
    }

    /// <summary>
    /// What <paramref name="scene"/>'s pictures show at each place of the native 256-colour image, indexed by place, as
    /// <see cref="SourcesAt"/> answers for one place: the scene's pictures and inks are gathered once for every place
    /// (#1532).
    /// </summary>
    internal static IReadOnlyList<IReadOnlyList<ShownColourSource>> SourcesByPlace(LayeredScene scene) => Sources(scene, only: null);

    /// <summary>
    /// The sources of each place of the native image <paramref name="scene"/>'s pictures colour, indexed by place: only
    /// <paramref name="only"/>'s when it is given, every place's otherwise.
    /// </summary>
    private static List<ShownColourSource>[] Sources(LayeredScene scene, int? only)
    {
        var shown = new List<ShownColourSource>[NativeColouredPictures.ColorCount];
        for (int place = 0; place < shown.Length; place++)
            shown[place] = [];
        foreach (ShownPictureInks entry in ShownInks(scene))
        {
            RgbColor[] inks = entry.Shading.InkColours(entry.Picture);
            string? shading = null;
            foreach (int ink in entry.Inks)
            {
                int place = SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.CartridgeColourOf(entry.Picture, ink, entry.DrawRow);
                if (only is { } wanted && place != wanted)
                    continue;
                shading ??= entry.Shading.ToString() ?? string.Empty;
                shown[place].Add(new ShownColourSource(entry.Picture.Name, shading, ink, inks[ink]));
            }
        }
        return shown;
    }

    /// <summary>
    /// Each distinct picture, shading and draw row <paramref name="scene"/> shows with the inks it shows: a layer's
    /// pictures with every ink of their sheets in no draw row, then each sprite draw's picture with the inks its frame
    /// paints in the row its colouring ORs into its parts.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A picture names no shading, or a layer shows pictures gameplay colours by group, which a frontend scene has none of.
    /// </exception>
    private static List<ShownPictureInks> ShownInks(LayeredScene scene)
    {
        var shown = new List<ShownPictureInks>();
        foreach ((SpriteImage picture, PictureShading shading) in LayerPictures(scene))
            shown.Add(new ShownPictureInks(picture, shading, DrawRow: 0, [.. Enumerable.Range(0, picture.StandardColours.Length)]));
        foreach (SpriteDraw draw in scene.SpriteDraws)
        {
            var image = (ImageSpriteDraw)draw;
            SpriteImage picture = image.Image.Sheet;
            PictureShading shading = image.Shading ?? throw new InvalidOperationException($"Picture {picture.Name} names the shading it shows in.");
            int row = NativeColouredPictures.DrawRow(picture, shading);
            ShownPictureInks? entry = shown.Find(known => ReferenceEquals(known.Picture, picture) && known.Shading.Equals(shading) && known.DrawRow == row);
            if (entry is null)
            {
                entry = new ShownPictureInks(picture, shading, row, []);
                shown.Add(entry);
            }
            entry.Inks.UnionWith(FrameInks(image.Image));
        }
        return shown;
    }

    /// <summary>The inks <paramref name="frame"/>'s opaque pixels are painted in.</summary>
    private static HashSet<int> FrameInks(SpriteSheetFrame frame)
    {
        SpriteImage sheet = frame.Sheet;
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = sheet.Inks;
        int width = sheet.Size.X;
        var painted = new HashSet<int>();
        for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
        {
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
            {
                if (pixels[y * width + x].A != 0 && inks[y * width + x].Colour is int colour)
                    painted.Add(colour);
            }
        }
        return painted;
    }

    /// <summary>Each distinct picture and shading <paramref name="scene"/> draws, layers first, then sprite draws.</summary>
    /// <exception cref="InvalidOperationException">
    /// A picture names no shading, or a layer shows pictures gameplay colours by group, which a frontend scene has none of.
    /// </exception>
    private static List<(SpriteImage Picture, PictureShading Shading)> Pictures(LayeredScene scene)
    {
        List<(SpriteImage Picture, PictureShading Shading)> shown = LayerPictures(scene);
        foreach (SpriteDraw draw in scene.SpriteDraws)
        {
            var image = (ImageSpriteDraw)draw;
            Add(shown, image.Image.Sheet, image.Shading);
        }
        return shown;
    }

    /// <summary>Adds the image of each of <paramref name="picture"/>'s parts (#1634), as <paramref name="shading"/> shows it.</summary>
    private static void AddParts(List<(SpriteImage Picture, PictureShading Shading)> shown, LayeredPicture picture, PictureShading shading)
    {
        foreach (SpriteSheetFrame part in picture.Parts)
            Add(shown, part.Sheet, shading);
    }

    /// <summary>Each distinct picture and shading the layers of <paramref name="scene"/> draw, layer by layer.</summary>
    /// <exception cref="InvalidOperationException">
    /// A picture names no shading, or a layer shows pictures gameplay colours by group, which a frontend scene has none of.
    /// </exception>
    private static List<(SpriteImage Picture, PictureShading Shading)> LayerPictures(LayeredScene scene)
    {
        Ensure.NotNull(scene);
        var shown = new List<(SpriteImage Picture, PictureShading Shading)>();
        foreach (DisplayLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case PicturedPlaneLayer plane:
                    foreach (PlacedPlanePicture placed in plane.Plane.Pictures)
                        AddParts(shown, placed.Picture, placed.Shading);
                    break;
                case AddedPicturedPlaneLayer added:
                    foreach (PlacedPlanePicture placed in added.Plane.Pictures)
                        AddParts(shown, placed.Picture, placed.Shading);
                    break;
                case ColourAddWindowLayer or ScreenGradientLayer or ScreenTintLayer or SpritesLayer:
                    // These layers draw colours, or sprites, which are read from the scene's sprite draws below.
                    break;
                case PictureGridLayer grid:
                    foreach (GridPicture? cell in grid.Grid.Cells)
                    {
                        if (cell is { } picture)
                            AddParts(shown, picture.Picture, picture.Shading);
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
                default:
                    throw new InvalidOperationException($"Layer {layer.GetType().Name} is not one this check knows the pictures of.");
            }
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

/// <summary>A picture a scene shows under one shading in one draw row, with the inks it shows so.</summary>
/// <param name="Picture">The picture.</param>
/// <param name="Shading">The shading it shows under.</param>
/// <param name="DrawRow">The sprite row its draws OR into every part, zero for a layer's picture or a draw in no frontend colouring.</param>
/// <param name="Inks">The inks it shows.</param>
internal sealed record ShownPictureInks(SpriteImage Picture, PictureShading Shading, int DrawRow, HashSet<int> Inks);

/// <summary>One colour a picture of a scene shows at a place of the native 256-colour image.</summary>
/// <param name="Picture">The picture's name.</param>
/// <param name="Shading">The shading it shows under.</param>
/// <param name="Ink">The picture's ink coloured from that place.</param>
/// <param name="Colour">The colour it shows there.</param>
internal readonly record struct ShownColourSource(string Picture, string Shading, int Ink, RgbColor Colour);
