using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks a picture drawn over characters an encounter uploads (#1392): it shows exactly the inks its frame's parts show
/// over the room's live OBJ characters, the path such frames drew through before pre-coloured sheets.
/// </summary>
internal static class UploadedPictureChecks
{
    /// <summary>
    /// Asserts <paramref name="pictures"/> show exactly the inks <paramref name="parts"/> show over their characters: at
    /// every on-screen pixel the same palette entry, which decides the colour and whether it blends (#1405), and the same
    /// priority, with at least one pixel inked where <paramref name="requirePixels"/> asks for it.
    /// </summary>
    /// <returns>The on-screen pixels the parts ink.</returns>
    internal static int AssertPaintsLikeParts(ImmutableArray<SpriteDraw> pictures, ImmutableArray<CharacterSpriteDraw> parts,
        string message, bool requirePixels = true)
    {
        AssertTrue(pictures.Length > 0 && pictures.All(draw => draw is ImageSpriteDraw), $"{message}: draws from a sheet");
        return AssertSameInks([.. pictures.Cast<ImageSpriteDraw>()], parts, message, requirePixels);
    }

    /// <summary>
    /// Asserts <paramref name="images"/> show exactly the inks <paramref name="parts"/> show over their characters at every
    /// on-screen pixel: the frontmost inked pixel's palette entry and priority, with at least one pixel inked where
    /// <paramref name="requirePixels"/> asks for it (a sheet may hold a frame that draws nothing).
    /// </summary>
    /// <returns>The on-screen pixels the parts ink.</returns>
    internal static int AssertSameInks(IReadOnlyList<ImageSpriteDraw> images, IReadOnlyList<CharacterSpriteDraw> parts, string message,
        bool requirePixels = true)
    {
        Dictionary<(int X, int Y), (int Entry, int Priority)> expected = [], actual = [];
        foreach (CharacterSpriteDraw draw in parts)
        {
            for (int part = 0; part < draw.Frame.Parts.Length; part++)
            {
                SpriteScreenPart placed = draw.Place(part);
                for (ushort y = 0; y < placed.Size.Y; y++)
                for (ushort x = 0; x < placed.Size.X; x++)
                {
                    int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> within);
                    int colour = draw.Artwork.ColorIndex(character, within);
                    if (colour != 0)
                        Ink(expected, placed.Position.X + x, placed.Position.Y + y, placed.Palette * 16 + colour, placed.Priority);
                }
            }
        }
        foreach (ImageSpriteDraw image in images)
        {
            // A shaded draw's selection shade ORs its row into each part's own, as the cartridge does (#1512): each pixel's
            // row is the one the draw's composition places it in under that selection.
            Dictionary<(int X, int Y), int>? selectedRows = image.Shading is { } shading && SpriteColouringAccess.SelectedRow(shading) is not null
                ? RowsOf(SheetFrameSources.CharacterDraw(image)) : null;
            for (ushort y = 0; y < image.Image.Size.Y; y++)
            for (ushort x = 0; x < image.Image.Size.X; x++)
            {
                var cell = new Position16<ScreenPixels>(x, y);
                int entry = image.PaletteIndexAt(cell);
                int screenX = image.Corner.X + x, screenY = image.Corner.Y + y;
                if (entry != 0 && selectedRows is not null)
                {
                    int row = selectedRows.TryGetValue((screenX, screenY), out int placed) ? placed
                        : throw new InvalidOperationException($"{message}: {image} inks screen pixel ({screenX},{screenY}), which its composition leaves blank.");
                    entry = row * 16 + entry % 16;
                }
                else if (entry != 0)
                {
                    // A sheet's ink is its part's own row and colour; the draw ORs the sheet's palette bits into that row (#1512).
                    entry = InstalledPictureColoursConstants.CartridgeColourOf(image.Image.Sheet, entry) - GameplayColourPositionDefinitions.SpriteSetsFirst;
                }
                if (entry != 0)
                    Ink(actual, screenX, screenY, entry, image.PriorityAt(cell));
            }
        }
        foreach ((int X, int Y) pixel in expected.Keys.Union(actual.Keys))
        {
            (int Entry, int Priority)? want = expected.TryGetValue(pixel, out var e) ? e : null, got = actual.TryGetValue(pixel, out var g) ? g : null;
            if (want != got)
                throw new InvalidOperationException($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                    $"({pixel.X},{pixel.Y}): ink {got?.ToString() ?? "none"} for {want?.ToString() ?? "none"}.");
        }
        if (requirePixels)
            AssertTrue(expected.Count > 0, $"{message}: paints pixels");
        return expected.Count;

        // The palette row of the frontmost inked part at each screen pixel the draw covers.
        static Dictionary<(int X, int Y), int> RowsOf(CharacterSpriteDraw draw)
        {
            Dictionary<(int X, int Y), int> rows = [];
            for (int part = 0; part < draw.Frame.Parts.Length; part++)
            {
                SpriteScreenPart placed = draw.Place(part);
                for (ushort y = 0; y < placed.Size.Y; y++)
                for (ushort x = 0; x < placed.Size.X; x++)
                {
                    int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> within);
                    if (draw.Artwork.ColorIndex(character, within) != 0)
                        rows.TryAdd((placed.Position.X + x, placed.Position.Y + y), placed.Palette);
                }
            }
            return rows;
        }

        // The frontmost ink at an on-screen pixel; a later, further-back draw does not replace it.
        static void Ink(Dictionary<(int X, int Y), (int Entry, int Priority)> inks, int x, int y, int entry, int priority)
        {
            if (x is >= 0 and < ScreenGeometry.ScreenWidthPixels && y is >= 0 and < ScreenGeometry.ScreenHeightPixels)
                inks.TryAdd((x, y), (entry, priority));
        }
    }

    /// <summary>
    /// Asserts each of <paramref name="pictures"/> paints exactly what its frame's parts paint over the characters graphics
    /// set <paramref name="set"/> leaves once its encounters have played each sequence <paramref name="reached"/> names
    /// through the step it names (#1513): its own composition drawn over those characters.
    /// </summary>
    internal static void AssertPicturesPaintOverSteps(RoomEnemyGraphicsSetDefinition set, IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)> reached,
        ImmutableArray<SpriteDraw> pictures, string message)
    {
        SpriteArtwork live = EnemySheetFixtures.LivePage(set, reached);
        AssertPaintsLikeParts(pictures,
            [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"{message}: draws from a sheet"), live))],
            message);
    }

    /// <summary>The sequences the runtime's room encounters have reached and the step each reached (#1513).</summary>
    internal static IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)> Reached(SuperMetroidRuntime runtime) =>
        [.. Enum.GetValues<EncounterArtworkSequence>().Select(sequence => (sequence, runtime.EnemySession.ArtworkProgress.StepOf(sequence)))
            .Where(entry => entry.Item2 is not null).Select(entry => (entry.sequence, entry.Item2 ?? 0))];

    /// <summary>
    /// The picture <paramref name="frame"/> draws as once the room's encounters have played each sequence
    /// <paramref name="reached"/> names through the step it names: the frame itself, or its picture of the step each of its
    /// <paramref name="variations"/> shows then (#1513).
    /// </summary>
    internal static string ExpectedPicture(string frame, IReadOnlyList<FrameArtworkVariation> variations,
        IReadOnlyList<(EncounterArtworkSequence Sequence, int Step)> reached)
    {
        var shown = new List<(EncounterArtworkSequence Sequence, int Step)>();
        foreach (FrameArtworkVariation variation in variations)
        {
            foreach ((EncounterArtworkSequence sequence, int step) in reached)
            {
                if (sequence == variation.Sequence && variation.PictureAt(step) is int picture)
                    shown.Add((sequence, picture));
            }
        }
        return FrameArtworkVariation.PictureName(frame, shown);
    }

    /// <summary>Asserts <paramref name="drawn"/> is exactly <paramref name="sheet"/>'s picture named <paramref name="expected"/>.</summary>
    internal static void AssertDrawnPicture(SpriteSheet sheet, string expected, ImmutableArray<SpriteDraw> drawn, string message)
    {
        AssertEqual(1, drawn.Length, $"{message}: draws one image");
        AssertTrue(drawn[0] is ImageSpriteDraw image && ReferenceEquals(sheet.Frame(expected), image.Image), $"{message}: shows {expected}");
    }

    /// <summary>
    /// Asserts that at each of the <paramref name="steps"/> steps of <paramref name="sequence"/>, every frame of the installed
    /// sheets of graphics set <paramref name="set"/> the sequence changes shows a picture that paints exactly what the frame's
    /// parts paint over the characters the set leaves at that step (#1513): the step's own copies and every earlier one, the
    /// picture an earlier step shares included.
    /// </summary>
    /// <param name="set">The graphics set whose sheets are checked.</param>
    /// <param name="sequence">The sequence the set's encounter plays.</param>
    /// <param name="steps">The sequence's steps.</param>
    /// <param name="message">Names the check in failures.</param>
    /// <param name="requireFrames">Whether the sequence must change some frame of the set's sheets; the escape pages change backgrounds alone.</param>
    /// <returns>The frames the sequence changes.</returns>
    internal static int AssertFramesFollowSequence(RoomEnemyGraphicsSetDefinition set, EncounterArtworkSequence sequence, int steps, string message,
        bool requireFrames = true)
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        var origin = new Position16<ScreenPixels>((ushort)(ScreenGeometry.ScreenWidthPixels / 2), (ushort)(ScreenGeometry.ScreenHeightPixels / 2));
        int frames = 0;
        foreach ((EnemySheetKey key, IReadOnlyDictionary<string, FrameArtworkVariation[]> followed) in sheets.variations.Where(entry => entry.Key.Set == set.Name))
        {
            SpriteSheet sheet = sheets.For(set, key.Enemy);
            foreach ((string frame, FrameArtworkVariation[] variations) in followed.Where(entry => entry.Value.Any(variation => variation.Sequence == sequence)))
            {
                frames++;
                for (int step = 0; step < steps; step++)
                {
                    string picture = ExpectedPicture(frame, variations, [(sequence, step)]);
                    var draw = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy, 0), sheet.Frame(picture), origin, SpriteMirror.None);
                    AssertSameInks([draw], [SheetFrameSources.CharacterDraw(draw, EnemySheetFixtures.LivePage(set, [(sequence, step)]))],
                        $"{message}: {key.Enemy} frame {frame} at step {step} of {sequence}", requirePixels: false);
                }
            }
        }
        if (requireFrames)
            AssertTrue(frames > 0, $"{message}: {sequence} changes frames of {set.Name}'s sheets");
        return frames;
    }
}
