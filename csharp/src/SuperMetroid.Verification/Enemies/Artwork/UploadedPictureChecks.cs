using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;
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
    /// every on-screen pixel the same palette entry, which decides the colour and whether it blends (#1405), with at least
    /// one pixel inked where <paramref name="requirePixels"/> asks for it. A picture is drawn whole at its frame's depth
    /// (#1514), so no pixel keeps its part's priority.
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
    /// on-screen pixel: the frontmost inked pixel's palette entry, with at least one pixel inked where
    /// <paramref name="requirePixels"/> asks for it (a sheet may hold a frame that draws nothing).
    /// </summary>
    /// <returns>The on-screen pixels the parts ink.</returns>
    internal static int AssertSameInks(IReadOnlyList<ImageSpriteDraw> images, IReadOnlyList<CharacterSpriteDraw> parts, string message,
        bool requirePixels = true)
    {
        Dictionary<(int X, int Y), int> expected = [], actual = [];
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
                        Ink(expected, placed.Position.X + x, placed.Position.Y + y, placed.Palette * 16 + colour);
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
                    entry = InstalledPictureColoursConstants.CartridgeColourOf(image.Image.Sheet,
                        InstalledPictureColoursConstants.ColourComposedAs(image.Image.Sheet, entry) ?? throw new InvalidDataException(
                            $"{message}: {image} inks palette index {entry}, none of its sheet's colours.")) - GameplayColourPositionDefinitions.SpriteSetsFirst;
                }
                if (entry != 0)
                    Ink(actual, screenX, screenY, entry);
            }
        }
        foreach ((int X, int Y) pixel in expected.Keys.Union(actual.Keys))
        {
            int? want = expected.TryGetValue(pixel, out int e) ? e : null, got = actual.TryGetValue(pixel, out int g) ? g : null;
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
        static void Ink(Dictionary<(int X, int Y), int> inks, int x, int y, int entry)
        {
            if (x is >= 0 and < ScreenGeometry.ScreenWidthPixels && y is >= 0 and < ScreenGeometry.ScreenHeightPixels)
                inks.TryAdd((x, y), entry);
        }
    }

    /// <summary>
    /// Asserts each of <paramref name="pictures"/> paints exactly what its frame's parts paint over the characters graphics
    /// set <paramref name="set"/> leaves once its encounters show each look of <paramref name="shown"/> (#1538): its own
    /// composition drawn over those characters.
    /// </summary>
    internal static void AssertPicturesPaintOverLooks(RoomEnemyGraphicsSetDefinition set, IReadOnlyList<EncounterLook> shown,
        ImmutableArray<SpriteDraw> pictures, string message)
    {
        SpriteArtwork live = EnemySheetFixtures.LivePage(set, shown);
        AssertPaintsLikeParts(pictures,
            [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"{message}: draws from a sheet"), live))],
            message);
    }

    /// <summary>The looks the runtime's room encounters show other than their room's load (#1538).</summary>
    internal static IReadOnlyList<EncounterLook> Shown(SuperMetroidRuntime runtime) =>
        [.. Enum.GetValues<EncounterArtwork>().Select(runtime.EnemySession.Looks.Current).Where(look => look.Name != EncounterLook.LoadedName)];

    /// <summary>
    /// The picture <paramref name="frame"/> draws as while the room's encounters show the looks of <paramref name="shown"/>:
    /// its picture of the look each of the <paramref name="changing"/> encounters shows then, the room's load for one
    /// <paramref name="shown"/> names no look of (#1538).
    /// </summary>
    internal static string ExpectedPicture(string frame, IReadOnlyList<EncounterArtwork> changing, IReadOnlyList<EncounterLook> shown) =>
        EncounterLook.PictureName(frame, changing.Select(encounter =>
            shown.Where(look => look.Encounter == encounter).DefaultIfEmpty(EncounterLook.Loaded(encounter)).Single()));

    /// <summary>
    /// Asserts <paramref name="drawn"/> is exactly the picture named <paramref name="expected"/> that <paramref name="pictureOf"/>
    /// finds on the sheets it reads.
    /// </summary>
    internal static void AssertDrawnPicture(Func<string, SpriteSheetFrame> pictureOf, string expected, ImmutableArray<SpriteDraw> drawn, string message)
    {
        AssertEqual(1, drawn.Length, $"{message}: draws one image");
        AssertTrue(drawn[0] is ImageSpriteDraw image && ReferenceEquals(pictureOf(expected), image.Image), $"{message}: shows {expected}");
    }

    /// <summary>
    /// Asserts that in each look of <paramref name="encounter"/> it is drawn in, every frame of the installed enemy and
    /// spawned-object sheets of graphics set <paramref name="set"/> the encounter's looks change shows a finished picture
    /// that paints exactly what the frame's parts paint over the characters the set leaves in that look (#1538): the look's
    /// own copies and every earlier look's. In a look the frame is never drawn in, its sheet holds no picture and asking
    /// for one fails loudly, naming the picture.
    /// </summary>
    /// <param name="set">The graphics set whose sheets are checked.</param>
    /// <param name="encounter">The encounter the set's members play.</param>
    /// <param name="message">Names the check in failures.</param>
    /// <param name="requireFrames">Whether the encounter's looks must change some frame of the set's sheets; the escape warning changes backgrounds alone.</param>
    /// <returns>The frames the encounter's looks change.</returns>
    internal static int AssertFramesFollowLooks(RoomEnemyGraphicsSetDefinition set, EncounterArtwork encounter, string message, bool requireFrames = true)
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        int frames = 0;
        foreach ((EnemySheetKey key, IReadOnlyDictionary<string, EncounterArtwork[]> changed) in sheets.changingEncounters.Where(entry => entry.Key.Set == set.Name))
        {
            SpriteSheet sheet = sheets.For(set, key.Enemy);
            frames += AssertSheetFollowsLooks(set, encounter, sheet.Frame, changed,
                picture => sheet.FrameNames.Contains(picture) || sheet.BlankFrameNames.Contains(picture), sheet.ShowsNothing, $"{message}: {key.Enemy}");
        }
        var checkedSheets = new HashSet<SpawnedObjectSheet>(ReferenceEqualityComparer.Instance);
        foreach ((SpawnedObjectSheetKey key, IReadOnlyDictionary<string, EncounterArtwork[]> changed) in sheets.spawnedChangingEncounters.Where(entry => entry.Key.Set == set.Name))
        {
            // Owners sharing one set's spawned-object pictures are checked once.
            SpawnedObjectSheet sheet = sheets.Spawned(set, key.Owner);
            if (checkedSheets.Add(sheet))
                frames += AssertSheetFollowsLooks(set, encounter, sheet.Frame, changed,
                    picture => sheet.frames.ContainsKey(picture) || sheet.empty.Contains(picture), sheet.ShowsNothing, $"{message}: {key.Owner}'s spawned objects");
        }
        if (requireFrames)
            AssertTrue(frames > 0, $"{message}: {encounter}'s looks change frames of {set.Name}'s sheets");
        return frames;
    }

    /// <summary>
    /// Asserts the frames whose pictures <paramref name="pictureOf"/> finds that <paramref name="changed"/> says
    /// <paramref name="encounter"/>'s looks change follow its looks, as <see cref="AssertFramesFollowLooks"/> describes.
    /// </summary>
    /// <returns>The frames the encounter's looks change.</returns>
    private static int AssertSheetFollowsLooks(RoomEnemyGraphicsSetDefinition set, EncounterArtwork encounter, Func<string, SpriteSheetFrame> pictureOf,
        IReadOnlyDictionary<string, EncounterArtwork[]> changed, Func<string, bool> holds, Func<string, bool> showsNothing, string message)
    {
        var origin = new Position16<ScreenPixels>(ScreenGeometry.ScreenWidthPixels / 2, ScreenGeometry.ScreenHeightPixels / 2);
        ComposedEncounter composed = Enum.GetValues<ComposedEncounter>().Single(candidate => EncounterLookUploads.ShownAs(candidate) == encounter);
        int frames = 0;
        foreach ((string frame, EncounterArtwork[] encounters) in changed.Where(entry => entry.Value.Contains(encounter)))
        {
            frames++;
            int drawnLooks = 0;
            foreach (string name in EncounterLookUploads.Looks(composed))
            {
                var look = new EncounterLook(encounter, name);
                string picture = ExpectedPicture(frame, encounters, [look]);
                if (!holds(picture))
                {
                    InvalidDataException failure = AssertThrows<InvalidDataException>(() => pictureOf(picture),
                        $"{message} frame {frame} drawn in {look}, a look it is never drawn in, fails");
                    AssertTrue(failure.Message.Contains(picture, StringComparison.Ordinal), $"{message} frame {frame}'s failure in {look} names the picture");
                    continue;
                }
                drawnLooks++;
                // A finished picture that shows nothing is on no sheet and draws nothing (#1538).
                if (showsNothing(picture))
                    continue;
                var draw = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy, 0), pictureOf(picture), origin, SpriteMirror.None);
                AssertSameInks([draw], [SheetFrameSources.CharacterDraw(draw, EnemySheetFixtures.LivePage(set, [look]))],
                    $"{message} frame {frame} in {look}", requirePixels: false);
            }
            AssertTrue(drawnLooks > 1, $"{message} frame {frame} has a picture of more than one of {encounter}'s looks");
        }
        return frames;
    }
}
