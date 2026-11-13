using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening cinematic's scene pictures once (#1467): each scene page, Samus's portrait and each eye frame, over
/// the scene characters under the cinematic's palette, each installed whole (#1634). A tile naming a
/// character past those the cinematic loads shows nothing, as the scene planes showed nothing there. The sheet holds a shade
/// for every cross-fade level of each faded colour group (#1512).
/// </summary>
internal static class IntroScenePictureSheetComposer
{
    /// <summary>The name of every picture the sheet holds.</summary>
    internal static IEnumerable<string> FrameNames =>
        [.. Enum.GetValues<IntroScenePage>().Select(IntroScenePictures.Page), IntroScenePictures.Portrait, .. Enum.GetValues<IntroEyeFrame>().Select(IntroScenePictures.Eye)];

    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>Composes the sheet from the installed intro artwork.</summary>
    internal static ComposedSpriteSheet Compose(ImportIntroCinematicArtwork intro)
    {
        Ensure.NotNull(intro);
        byte[] characters = intro.BackgroundCharacters.Characters.Span[..IntroArtworkExtentDefinitions.BackgroundCharacterBytes].ToArray();
        RgbColor[] colours = [.. intro.Palette.BackgroundColours];
        int cells = IntroScenePictures.PlaneCells;
        int planeCells = cells * cells;
        IntroScenePage[] pages = Enum.GetValues<IntroScenePage>();
        IntroEyeFrame[] eyes = Enum.GetValues<IntroEyeFrame>();
        Extent16<CharacterCells> eye = IntroArtworkExtentDefinitions.EyeFrameSize;
        int width = Math.Max(cells, eyes.Length * eye.X) * CharacterPixels;
        int height = ((pages.Length + 1) * cells + eye.Y) * CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)height)));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        ImportIntroCinematicArtwork sources = intro;
        ReadOnlySpan<BackgroundTile> pageTiles = sources.BackgroundPages.Span;
        for (int page = 0; page < pages.Length; page++)
            frames.Add(IntroScenePictures.Page(pages[page]),
                PaintRect(canvas, 0, page * cells, cells, pageTiles.Slice(page * planeCells, planeCells), characters, colours));
        frames.Add(IntroScenePictures.Portrait,
            PaintRect(canvas, 0, pages.Length * cells, cells, sources.PortraitTilemap.Span[..planeCells], characters, colours));
        for (int frame = 0; frame < eyes.Length; frame++)
            frames.Add(IntroScenePictures.Eye(eyes[frame]),
                PaintRect(canvas, frame * eye.X, (pages.Length + 1) * cells, eye.X, sources.EyeFrames.FrameTiles(frame), characters, colours));
        ComposedSpriteSheet sheet = canvas.ToSheet(colours, frames, SpriteImageColours.Background);
        // The scene pictures show every cross-fade level of the colour groups the opening fades (#1512).
        return sheet with { Manifest = sheet.Manifest with { Shades = SheetShade.Documents(IntroSheetShades.Scene(colours)) } };
    }

    /// <summary>
    /// Paints <paramref name="tiles"/>, rows of <paramref name="columns"/> cells, from cell (<paramref name="left"/>,
    /// <paramref name="top"/>), returning its frame.
    /// </summary>
    private static SpriteSheetFrameDocument PaintRect(RoomTileCanvas canvas, int left, int top, int columns, ReadOnlySpan<BackgroundTile> tiles,
        byte[] characters, RgbColor[] colours)
    {
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int cell = 0; cell < tiles.Length; cell++)
        {
            int start = tiles[cell].Tile * RoomTileCanvas.CharacterBytes;
            if (start + RoomTileCanvas.CharacterBytes <= characters.Length)
                coloured = coloured.Union(canvas.PaintTile((left + cell % columns) * CharacterPixels, (top + cell / columns) * CharacterPixels, tiles[cell],
                    characters.AsSpan(start, RoomTileCanvas.CharacterBytes), colours));
        }
        return new SpriteSheetFrameDocument
        {
            Corner = new Position16<ImagePixels>(checked((ushort)(left * CharacterPixels)), checked((ushort)(top * CharacterPixels))),
            Size = new Extent16<ImagePixels>(checked((ushort)(columns * CharacterPixels)), checked((ushort)(tiles.Length / columns * CharacterPixels))),
            Origin = default,
            ColourGroups = coloured.ManifestMembers,
        };
    }
}
