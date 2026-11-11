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
        // Each worker thread reuses its two screen buffers, so a comparison allocates nothing per pixel.
        ScreenInks expected = expectedInks ??= new(), actual = actualInks ??= new();
        expected.Clear();
        actual.Clear();
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
                        expected.Ink(placed.Position.X + x, placed.Position.Y + y, placed.Palette * 16 + colour);
                }
            }
        }
        foreach (ImageSpriteDraw image in images)
        {
            // A shaded draw's selection shade ORs its row into each part's own, as the cartridge does (#1512): each pixel's
            // row is the one the draw's composition places it in under that selection.
            Dictionary<(int X, int Y), int>? selectedRows = image.Shading is { } shading && SpriteColouringAccess.SelectedRow(shading) is not null
                ? RowsOf(SheetFrameSources.CharacterDraw(image)) : null;
            Offset16<ScreenPixels> corner = image.Corner;
            for (ushort y = 0; y < image.Image.Size.Y; y++)
            for (ushort x = 0; x < image.Image.Size.X; x++)
            {
                var cell = new Position16<ScreenPixels>(x, y);
                int entry = image.PaletteIndexAt(cell);
                int screenX = corner.X + x, screenY = corner.Y + y;
                if (entry != 0 && selectedRows is not null)
                {
                    int row = selectedRows.TryGetValue((screenX, screenY), out int placed) ? placed
                        : throw new InvalidOperationException($"{message}: {image} inks screen pixel ({screenX},{screenY}), which its composition leaves blank.");
                    entry = row * 16 + entry % 16;
                }
                else if (entry != 0)
                {
                    // A sheet's palette index is its part's own row and colour; the draw ORs the sheet's palette bits into that row (#1512).
                    if (InstalledPictureColoursConstants.KeyComposedAs(image.Image.Sheet, entry) is null)
                        throw new InvalidDataException($"{message}: {image} paints palette index {entry}, none of its sheet's colours.");
                    entry = InstalledPictureColoursConstants.CartridgeColourOfIndex(image.Image.Sheet, entry) - GameplayColourPositionDefinitions.SpriteSetsFirst;
                }
                if (entry != 0)
                    actual.Ink(screenX, screenY, entry);
            }
        }
        foreach (int pixel in expected.Inked)
            if (actual.At(pixel) != expected.At(pixel))
                throw Differs(pixel);
        foreach (int pixel in actual.Inked)
            if (expected.At(pixel) is null)
                throw Differs(pixel);
        if (requirePixels)
            AssertTrue(expected.Count > 0, $"{message}: paints pixels");
        return expected.Count;

        InvalidOperationException Differs(int pixel) =>
            new($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                $"({ScreenInks.XOf(pixel)},{ScreenInks.YOf(pixel)}): ink {actual.At(pixel)?.ToString() ?? "none"} for {expected.At(pixel)?.ToString() ?? "none"}.");

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
    }

    /// <summary>This thread's buffers of the inks a comparison expects and the inks its pictures show.</summary>
    [ThreadStatic]
    private static ScreenInks? expectedInks, actualInks;

    /// <summary>The palette entry inked at each on-screen pixel, the first ink at a pixel staying over later ones.</summary>
    private sealed class ScreenInks
    {
        /// <summary>Each screen pixel's entry plus one, or zero while nothing inks it.</summary>
        private readonly int[] entries = new int[ScreenGeometry.ScreenWidthPixels * ScreenGeometry.ScreenHeightPixels];

        /// <summary>The pixels inked, in the order they were first inked.</summary>
        private readonly List<int> inked = [];

        /// <summary>How many pixels are inked.</summary>
        internal int Count => inked.Count;

        /// <summary>The pixels inked, by screen index.</summary>
        internal IReadOnlyList<int> Inked => inked;

        /// <summary>The screen column of a pixel index.</summary>
        internal static int XOf(int pixel) => pixel % ScreenGeometry.ScreenWidthPixels;

        /// <summary>The screen row of a pixel index.</summary>
        internal static int YOf(int pixel) => pixel / ScreenGeometry.ScreenWidthPixels;

        /// <summary>Forgets every ink.</summary>
        internal void Clear()
        {
            foreach (int pixel in inked)
                entries[pixel] = 0;
            inked.Clear();
        }

        /// <summary>Inks the pixel at (<paramref name="x"/>, <paramref name="y"/>) unless it is off screen or already inked.</summary>
        internal void Ink(int x, int y, int entry)
        {
            if (x is < 0 or >= ScreenGeometry.ScreenWidthPixels || y is < 0 or >= ScreenGeometry.ScreenHeightPixels)
                return;
            int pixel = (y * ScreenGeometry.ScreenWidthPixels) + x;
            if (entries[pixel] != 0)
                return;
            entries[pixel] = entry + 1;
            inked.Add(pixel);
        }

        /// <summary>The entry inked at a pixel index, or null where nothing inks it.</summary>
        internal int? At(int pixel) => entries[pixel] == 0 ? null : entries[pixel] - 1;
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
        PopulationArtwork artwork = InstalledFixtures.EnemySheets.Under(set);
        int frames = 0;
        foreach ((EnemyDefinitionId enemy, EnemyArtworkVariant<SpriteSheet> variant) in artwork.enemies.Where(entry => entry.Value.changingEncounters.Count > 0))
        {
            SpriteSheet sheet = variant.Sheet;
            frames += AssertSheetFollowsLooks(set, encounter, sheet.Frame, variant.changingEncounters,
                picture => sheet.FrameNames.Contains(picture) || sheet.BlankFrameNames.Contains(picture), sheet.ShowsNothing, $"{message}: {enemy}");
        }
        var checkedSheets = new HashSet<SpawnedObjectSheet>(ReferenceEqualityComparer.Instance);
        foreach ((SpawnedObjectOwner owner, EnemyArtworkVariant<SpawnedObjectSheet> variant) in artwork.spawned.Where(entry => entry.Value.changingEncounters.Count > 0))
        {
            // Owners sharing one variant's spawned-object pictures are checked once.
            SpawnedObjectSheet sheet = variant.Sheet;
            if (checkedSheets.Add(sheet))
                frames += AssertSheetFollowsLooks(set, encounter, sheet.Frame, variant.changingEncounters,
                    picture => sheet.frames.ContainsKey(picture) || sheet.empty.Contains(picture), sheet.ShowsNothing, $"{message}: {owner}'s spawned objects");
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
        // Each look's live characters are the same for every frame, so each is composed once.
        Dictionary<string, SpriteArtwork> pages = EncounterLookUploads.Looks(composed)
            .ToDictionary(name => name, name => EnemySheetFixtures.LivePage(set, [new EncounterLook(encounter, name)]), StringComparer.Ordinal);
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
                AssertSameInks([draw], [SheetFrameSources.CharacterDraw(draw, pages[name])],
                    $"{message} frame {frame} in {look}", requirePixels: false);
            }
            AssertTrue(drawnLooks > 1, $"{message} frame {frame} has a picture of more than one of {encounter}'s looks");
        }
        return frames;
    }
}
