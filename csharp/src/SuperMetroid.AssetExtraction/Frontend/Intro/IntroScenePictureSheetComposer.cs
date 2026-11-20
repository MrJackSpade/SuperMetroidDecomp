using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Composes the opening cinematic's scene pictures once (#1467): each scene page, Samus's portrait and each eye frame, over
/// the scene characters under the cinematic's palette, each installed whole (#1634). A tile naming a
/// character past those the cinematic loads shows nothing, as the scene planes showed nothing there. The sheet holds a shade
/// for every cross-fade level of each faded colour group (#1512) and for every record the old Mother Brain fight's background
/// lights show (#1733).
/// </summary>
internal static class IntroScenePictureSheetComposer
{
    /// <summary>The scene pages in the sheet's order (#2567), top to bottom.</summary>
    private static readonly IReadOnlyList<IntroScenePage> Pages = Ensure.EveryMemberOnce<IntroScenePage>(
    [
        IntroScenePage.MotherBrainRoom,
        IntroScenePage.BabyDiscoveryRoom,
        IntroScenePage.ScientistDelivery,
        IntroScenePage.ScientistExamination,
    ]);

    /// <summary>The eye frames in the sheet's order (#2567), left to right: open, half open, closed, deadpan.</summary>
    private static readonly IReadOnlyList<IntroEyeFrame> Eyes = Ensure.EveryMemberOnce<IntroEyeFrame>(
    [
        IntroEyeFrame.Open,
        IntroEyeFrame.HalfOpen,
        IntroEyeFrame.Closed,
        IntroEyeFrame.Deadpan,
    ]);

    /// <summary>The name of every picture the sheet holds.</summary>
    internal static IEnumerable<string> FrameNames =>
        [.. Pages.Select(IntroScenePictures.Page), IntroScenePictures.Portrait, .. Eyes.Select(IntroScenePictures.Eye)];

    /// <summary>Pixels along each side of a character.</summary>
    private const int CharacterPixels = RoomTileCanvas.CharacterPixels;

    /// <summary>Composes the sheet from the installed intro artwork, the fight lights' records coloured from <paramref name="paletteFx"/>.</summary>
    /// <exception cref="InvalidDataException">A fight lights record has no installed colour, or writes past the lights' set.</exception>
    internal static ComposedSpriteSheet Compose(ImportIntroCinematicArtwork intro, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(intro);
        Ensure.NotNull(paletteFx);
        byte[] characters = intro.BackgroundCharacters.Characters.Span[..IntroArtworkExtentDefinitions.BackgroundCharacterBytes].ToArray();
        RgbColor[] colours = [.. intro.Palette.BackgroundColours];
        int cells = IntroArtworkExtentDefinitions.PlaneCells;
        int planeCells = cells * cells;
        Extent16<CharacterCells> eye = IntroArtworkExtentDefinitions.EyeFrameSize;
        int width = Math.Max(cells, Eyes.Count * eye.X) * CharacterPixels;
        int height = ((Pages.Count + 1) * cells + eye.Y) * CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)width), checked((ushort)height)));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        ImportIntroCinematicArtwork sources = intro;
        ReadOnlySpan<BackgroundTile> pageTiles = sources.BackgroundPages.Span;
        for (int place = 0; place < Pages.Count; place++)
        {
            int stored = Ensure.Listed(IntroCinematicArtworkFormat.BackgroundPageOf, Pages[place]);
            frames.Add(IntroScenePictures.Page(Pages[place]),
                PaintRect(canvas, 0, place * cells, cells, pageTiles.Slice(stored * planeCells, planeCells), characters, colours));
        }
        frames.Add(IntroScenePictures.Portrait,
            PaintRect(canvas, 0, Pages.Count * cells, cells, sources.PortraitTilemap.Span[..planeCells], characters, colours));
        for (int place = 0; place < Eyes.Count; place++)
        {
            frames.Add(IntroScenePictures.Eye(Eyes[place]),
                PaintRect(canvas, place * eye.X, (Pages.Count + 1) * cells, eye.X, sources.EyeFrames.FrameTiles(Eyes[place]), characters, colours));
        }
        ComposedSpriteSheet sheet = canvas.ToSheet(colours, frames, SpriteImageColours.Background);
        // The scene pictures show every cross-fade level of the colour groups the opening fades (#1512), and the Mother Brain
        // room each record its fight's background lights show over the colours they recolour (#1733).
        return sheet with
        {
            Manifest = sheet.Manifest with
            {
                Shades = SheetShade.Documents([.. IntroSheetShades.Scene(colours),
                    .. PaletteFxScreenShades.Of([IntroCinematicDefinitions.FightLights], ColourEffectColours.OldMotherBrainBackgroundLights,
                        IntroSheetInks.FightLights, paletteFx)]),
            },
        };
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
