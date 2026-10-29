using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours the tile words written over each tileset's rooms (#1392): one image of each part each
/// writer writes over BG1 or BG2 (typed text) or BG3 (an effect's page), coloured with the tileset's standard palette
/// over the characters the room has loaded, and each tile word pictured again over each artwork the room binds after
/// loading at its character. BG3's sheet also pictures each of the HUD's images as its own frame (#1466).
/// </summary>
internal static class TileWordSheetComposer
{
    /// <summary>Tiles across a part: the width of the tilemaps the words are written into.</summary>
    private const int PartTiles = 32;

    /// <summary>Cells in reading order: by row, then by column.</summary>
    private static readonly IComparer<Position16<TilemapCells>> RowMajor =
        Comparer<Position16<TilemapCells>>.Create((a, b) => a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

    /// <summary>
    /// Composes the sheet of <paramref name="writers"/>' BG1 and BG2 words over <paramref name="content"/>, the room as it
    /// has loaded and, where its backgrounds read the OBJ character table, as its encounters upload there.
    /// </summary>
    /// <exception cref="InvalidDataException">A part reads a character nothing the room loads is bound over.</exception>
    internal static ComposedTileWordSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IEnumerable<TileWordWriter> writers, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        List<NamedPart> parts = [.. Ensure.NotNull(writers).Order()
            .SelectMany(writer => Parts(inputs, writer).Select(part => new NamedPart($"{writer}.{part.Name}", part.Part)))];
        // Words over the OBJ character table show only what its load and encounters place there, never the tileset's.
        return Compose(inputs, tileset, TileWordLayer.Bg12, parts, content, new TileWordBindings(
            uploads.WordCharacters == RoomBackgroundCharacters.ObjectCharacterPage ? [] : uploads.Uploads, [BgCharacterSet.Tiles], uploads.WordUploads));
    }

    /// <summary>
    /// Composes the sheet of the BG3 words a room of <paramref name="tileset"/> shows: each of the HUD's images as its own
    /// frame (#1466) and the pages of the room effects its rooms run, over the HUD's and effects' characters in each set
    /// BG3 reads, and over each of <paramref name="bg3Uploads"/>, what any room binds over BG3's characters.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A part reads a character nothing the room loads is bound over, or a HUD image reads a character a room can show
    /// other artwork at.
    /// </exception>
    internal static ComposedTileWordSheet ComposeBg3(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IReadOnlyList<BgCharacterUpload> bg3Uploads)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        HudSheetLayout hud = HudImageSheet.Layout(GameplayHudArtwork.Of(inputs.Maps.GameplayHud), HudImageSheet.Maps(inputs.Maps));
        List<NamedPart> parts = [.. HudShelves(hud),
            .. uploads.FxTypes.Order().Select(type => new NamedPart($"Fx{type}", Page(inputs.Maps.RoomFxLayer3Tilemaps.Resolve(type).Span))),
            Fill(Layer3Plane.ClearedCell)];
        return Compose(inputs, tileset, TileWordLayer.Bg3, parts,
            uploads.Bg3Content ?? throw new InvalidDataException($"No room of {tileset} records what BG3 shows over."),
            new TileWordBindings([RoomBlockUploads.HudCharacters(inputs), .. bg3Uploads, .. uploads.Uploads], [BgCharacterSet.Layer3], []),
            [.. HudFrames(hud)]);
    }

    /// <summary>The name of the part holding shelf <paramref name="shelf"/> of the HUD's images.</summary>
    private static string HudShelfName(int shelf) => $"HudShelf{shelf}";

    /// <summary>Each shelf of <paramref name="hud"/>'s images as a part: each image's tiles at its place on the shelf.</summary>
    private static IEnumerable<NamedPart> HudShelves(HudSheetLayout hud)
    {
        for (int shelf = 0; shelf < hud.ShelfRows.Length; shelf++)
        {
            var runs = new List<BodyRun>();
            foreach (HudSheetPlacement placed in hud.Placements.Where(placed => placed.Shelf == shelf))
            {
                HudImageTiles image = placed.Image;
                for (int row = 0; row < image.Size.Y; row++)
                    runs.Add(new BodyRun(new(placed.Cell.X, (ushort)(placed.Cell.Y + row)),
                        BackgroundTileWords.ToWords(image.Tiles.AsSpan().Slice(row * image.Size.X, image.Size.X))));
            }
            yield return new NamedPart(HudShelfName(shelf), new BodyPart([.. runs]));
        }
    }

    /// <summary>The frame of each of <paramref name="hud"/>'s images, within its shelf's part.</summary>
    private static IEnumerable<PartFrame> HudFrames(HudSheetLayout hud) =>
        hud.Placements.Select(placed => new PartFrame(placed.Image.Image.FrameName, HudShelfName(placed.Shelf), placed.Cell, placed.Image.Size));

    /// <summary>
    /// Composes <paramref name="layer"/>'s sheet of <paramref name="parts"/> over <paramref name="content"/>, which binds
    /// <paramref name="bindings"/>' bound artwork beyond the room's load, each word's character read in the first of its
    /// sets, and each word pictured again over each of its options covering its character in any of them; each of
    /// <paramref name="partFrames"/> is cut from within its part.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A part reads a character nothing the room loads is bound over, or a part picturing HUD images reads a character an
    /// option shows other artwork at.
    /// </exception>
    private static ComposedTileWordSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, TileWordLayer layer,
        List<NamedPart> parts, BgArtworkBindings content, TileWordBindings bindings, IReadOnlyList<PartFrame>? partFrames = null)
    {
        Ensure.NotNull(content);
        int characterWords = TileWordSheet.CharacterWordsOf(layer);
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Append(RoomBlockUploads.HudCharacters(inputs)).Concat(bindings.Options).Concat(bindings.Bound)
            .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));

        var tiles = new List<TileWordSheetTile>();
        // The parts frames are cut from picture fixed images, whose characters no room may show otherwise.
        HashSet<string> imageParts = [.. (partFrames ?? []).Select(frame => frame.Part)];
        var pictured = new HashSet<ShownWord>();
        var placed = new List<PlacedWord>();
        int top = 0;
        foreach (NamedPart part in parts)
        {
            // A part shows the last word it writes at each position.
            var written = new SortedDictionary<Position16<TilemapCells>, ushort>(RowMajor);
            foreach (BodyRun run in part.Part.Runs)
            {
                for (int index = 0; index < run.Words.Length; index++)
                    written[new((ushort)(run.Start.X + index), (ushort)(run.Start.Y - part.Part.Top))] = run.Words[index];
            }
            foreach (KeyValuePair<Position16<TilemapCells>, ushort> entry in written)
            {
                Position16<TilemapCells> cell = entry.Key;
                var word = new SnesBgTilemapWord(entry.Value);
                int character = word.CharacterIndex;
                if (!content.TryShowing(bindings.Sets[0], character, 1, out BgArtworkShown shown))
                    throw new InvalidDataException($"{TileWordSheet.NameOf(layer, tileset)} part {part.Name} reads {bindings.Sets[0]} character {character}, which its room loads nothing over.");
                // A HUD image is one fixed picture (#1466): no room may show other artwork at a character it reads.
                if (imageParts.Contains(part.Name))
                    RequireFixedCharacter(bindings, shown, bytes, character, characterWords, $"{TileWordSheet.NameOf(layer, tileset)} part {part.Name}");
                var shownWord = new ShownWord(word, shown);
                placed.Add(new PlacedWord(top, shownWord, cell));
                if (pictured.Add(shownWord))
                    tiles.Add(new TileWordSheetTile(new BgTileArtwork(BackgroundTileWords.ToTile(word.Raw), shown), part.Name, Corner(cell)));
            }
            top += part.Part.Rows;
        }
        // A word the room shows over artwork it binds after loading is pictured once more over that artwork.
        var rebound = new List<ShownWord>();
        foreach (SnesBgTilemapWord word in placed.Select(tile => tile.Shown.Word).Distinct())
        {
            int character = word.CharacterIndex;
            foreach (BgCharacterSet set in bindings.Sets)
            {
                foreach (BgCharacterUpload placement in bindings.Options.Where(placement => placement.Covers(set, character)))
                {
                    var again = new ShownWord(word, placement.ShownAt(character));
                    if (pictured.Add(again))
                        rebound.Add(again);
                }
            }
        }
        int reboundRows = (rebound.Count + PartTiles - 1) / PartTiles;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(PartTiles * RoomTileCanvas.CharacterPixels, checked((ushort)((top + reboundRows) * RoomTileCanvas.CharacterPixels))));
        foreach (PlacedWord tile in placed)
            Paint(new(tile.Cell.X, (ushort)(tile.PartTop + tile.Cell.Y)), tile.Shown);
        for (int index = 0; index < rebound.Count; index++)
        {
            var cell = new Position16<TilemapCells>((ushort)(index % PartTiles), (ushort)(index / PartTiles));
            Paint(new(cell.X, (ushort)(top + cell.Y)), rebound[index]);
            tiles.Add(new TileWordSheetTile(new BgTileArtwork(BackgroundTileWords.ToTile(rebound[index].Word.Raw), rebound[index].Shown),
                TileWordSheet.ReboundFrameName, Corner(cell)));
        }

        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var partTops = new Dictionary<string, int>(StringComparer.Ordinal);
        int frameTop = 0;
        foreach (NamedPart part in parts)
        {
            partTops.Add(part.Name, frameTop);
            frames.Add(part.Name, Frame(canvas, frameTop, part.Part.Rows, layer));
            frameTop += part.Part.Rows;
        }
        if (reboundRows > 0)
            frames.Add(TileWordSheet.ReboundFrameName, Frame(canvas, top, reboundRows, layer));
        foreach (PartFrame frame in partFrames ?? [])
        {
            var corner = new Position16<TilemapCells>(frame.Cell.X, checked((ushort)(partTops[frame.Part] + frame.Cell.Y)));
            frames.Add(frame.Name, Frame(canvas, HudImageSheet.Pixels(corner), HudImageSheet.Pixels(frame.Size), layer));
        }
        return new ComposedTileWordSheet(layer, tileset, canvas.ToRoomSheet(colours, frames, TileWordSheet.ColoursOf(layer)), tiles);

        void Paint(Position16<TilemapCells> cell, ShownWord word)
        {
            ReadOnlySpan<byte> character = bytes[word.Shown.Artwork].Span.Slice(word.Shown.WordOffset * sizeof(ushort), characterWords * sizeof(ushort));
            int x = cell.X * RoomTileCanvas.CharacterPixels, y = cell.Y * RoomTileCanvas.CharacterPixels;
            if (layer == TileWordLayer.Bg3)
                canvas.PaintTwoBppTile(x, y, word.Word, character, colours);
            else
                canvas.PaintTile(x, y, word.Word, character, colours);
        }
    }

    /// <summary>The pixel corner of the tile at <paramref name="cell"/> within its frame.</summary>
    private static Position16<ImagePixels> Corner(Position16<TilemapCells> cell) =>
        new((ushort)(cell.X * RoomTileCanvas.CharacterPixels), (ushort)(cell.Y * RoomTileCanvas.CharacterPixels));

    /// <summary>The frame of <paramref name="rows"/> tile rows from row <paramref name="top"/>, across the sheet.</summary>
    internal static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, int top, int rows, TileWordLayer layer) =>
        Frame(canvas, new Position16<ImagePixels>(0, (ushort)(top * RoomTileCanvas.CharacterPixels)),
            new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)(rows * RoomTileCanvas.CharacterPixels)), layer);

    /// <summary>The frame of the <paramref name="size"/> pixels from <paramref name="corner"/>, coloured from the inks it shows.</summary>
    internal static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, Position16<ImagePixels> corner, Extent16<ImagePixels> size, TileWordLayer layer)
    {
        Ensure.NotNull(canvas);
        int rowColours = layer == TileWordLayer.Bg3 ? 4 : 16;
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int y = 0; y < size.Y; y++)
        for (int x = 0; x < size.X; x++)
        {
            int index = canvas.PaletteIndices[(corner.Y + y) * canvas.Width + corner.X + x];
            if (index % rowColours != 0)
                coloured = coloured.With(ImageColourGroup.OfInk(index));
        }
        return new SpriteSheetFrameDocument { Corner = corner, Size = size, Origin = default, ColourGroups = coloured.ManifestMembers, Parts = [] };
    }

    /// <summary>The parts <paramref name="writer"/> writes, each named, in a stable order.</summary>
    /// <exception cref="InvalidDataException">The writer's installed artwork is missing.</exception>
    private static IEnumerable<NamedPart> Parts(RoomBlockSheetInputs inputs, TileWordWriter writer)
    {
        switch (writer)
        {
            case TileWordWriter.ZebesEscapeText:
                yield return new NamedPart("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase));
                break;
            case TileWordWriter.CeresEscapeText:
                foreach (NamedPart part in CeresWarningParts(inputs))
                    yield return part;
                break;
            default:
                throw new InvalidDataException($"Undefined tile word writer {writer}.");
        }
    }

    /// <summary>
    /// The warning Ceres Ridley's getaway writes over BG1: its EMERGENCY title, the Japanese subtitle's rows and its typed
    /// text, each word at the BG1 overlay cell it is written to.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed area artwork has no Ceres warning tilemaps.</exception>
    private static IEnumerable<NamedPart> CeresWarningParts(RoomBlockSheetInputs inputs)
    {
        CeresEscapeOverlays overlays = inputs.Objects.Areas.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The installed area artwork has no Ceres warning tilemaps.");
        CeresEscapeOverlayTilemapDefinition emergency = CeresEscapeOverlayTilemapDefinitions.Emergency;
        yield return new NamedPart("Emergency", new BodyPart([.. Runs(emergency.Cell, OverlayWords(overlays, emergency))]));
        var subtitle = new List<BodyRun>();
        foreach (CeresEscapeOverlayTilemapDefinition row in CeresEscapeOverlayTilemapDefinitions.Japanese)
            subtitle.AddRange(Runs(row.Cell, OverlayWords(overlays, row)));
        yield return new NamedPart("Subtitle", new BodyPart([.. subtitle]));
        yield return new NamedPart("Typed", Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase));
    }

    /// <summary>The words of the installed Ceres warning tilemap <paramref name="page"/> the getaway writes.</summary>
    /// <exception cref="InvalidDataException">No installed warning tilemap holds them.</exception>
    private static ushort[] OverlayWords(CeresEscapeOverlays overlays, CeresEscapeOverlayTilemapDefinition page)
    {
        if (!overlays.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles))
            throw new InvalidDataException($"The installed Ceres warning tilemaps hold no {page.Name} page.");
        return BackgroundTileWords.ToWords(tiles.Span);
    }

    /// <summary>The text <paramref name="program"/> types from <paramref name="tileBase"/>, each glyph at the overlay cell it is typed over.</summary>
    private static BodyPart Typed(EscapeTypewriterProgram program, BackgroundTile tileBase)
    {
        var runs = new List<BodyRun>();
        foreach (EscapeTypewriterLine line in program.Lines)
        {
            int cell = line.Cell;
            foreach (char character in line.Text)
            {
                if (EscapeTypewriterState.GlyphOf((byte)character, tileBase) is { } glyph)
                    runs.AddRange(Runs(cell, [BackgroundTileWords.ToWord(glyph)]));
                cell++;
            }
        }
        return new BodyPart([.. runs]);
    }

    /// <summary>
    /// Requires that no option shows other artwork at <paramref name="character"/> than <paramref name="shown"/>, the
    /// artwork a fixed image cut from <paramref name="part"/> pictures there (#1466): a HUD image is one picture, which no
    /// room's bindings re-resolve.
    /// </summary>
    /// <exception cref="InvalidDataException">An option shows other bytes at the character.</exception>
    private static void RequireFixedCharacter(TileWordBindings bindings, BgArtworkShown shown,
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes, int character, int characterWords, string part)
    {
        int length = characterWords * sizeof(ushort);
        ReadOnlySpan<byte> pictured = bytes[shown.Artwork].Span.Slice(shown.WordOffset * sizeof(ushort), length);
        foreach (BgCharacterUpload option in bindings.Options)
        {
            if (!option.Covers(bindings.Sets[0], character))
                continue;
            BgArtworkShown other = option.ShownAt(character);
            if (!option.Bytes.Span.Slice(other.WordOffset * sizeof(ushort), length).SequenceEqual(pictured))
                throw new InvalidDataException($"{part} pictures {bindings.Sets[0]} character {character} as {shown.Artwork.Name} shows it, but {option.Identity.Name} can show other artwork there.");
        }
    }

    /// <summary>A room effect's page, as the tilemap words of its tiles lay it out.</summary>
    private static BodyPart Page(ReadOnlySpan<BackgroundTile> page) => Rows(BackgroundTileWords.ToWords(page));

    /// <summary>The tile a page is cleared with, as one part.</summary>
    private static NamedPart Fill(BackgroundTile tile) => new("Fill", new BodyPart([new BodyRun(default, [BackgroundTileWords.ToWord(tile)])]));

    /// <summary><paramref name="words"/> laid out from the top-left, 32 to a row.</summary>
    private static BodyPart Rows(ushort[] words) => new([.. Runs(0, words)]);

    /// <summary><paramref name="words"/> written from tilemap word <paramref name="firstWord"/>, split where they reach a row's end.</summary>
    private static IEnumerable<BodyRun> Runs(int firstWord, ushort[] words)
    {
        for (int start = 0; start < words.Length;)
        {
            int word = firstWord + start, count = Math.Min(words.Length - start, PartTiles - word % PartTiles);
            yield return new BodyRun(new((ushort)(word % PartTiles), (ushort)(word / PartTiles)), words[start..(start + count)]);
            start += count;
        }
    }


    /// <summary>One part: the runs of words it writes, laid out at their tilemap positions.</summary>
    /// <param name="Runs">The runs.</param>
    private sealed record BodyPart(BodyRun[] Runs)
    {
        /// <summary>The first tilemap row a run writes.</summary>
        internal int Top => Runs.Min(run => run.Start.Y);

        /// <summary>The tilemap rows from the first a run writes to the last.</summary>
        internal int Rows => Runs.Max(run => run.Start.Y) - Top + 1;
    }

    /// <summary>A run of words written left to right from one tilemap position.</summary>
    /// <param name="Start">The first word's cell.</param>
    /// <param name="Words">The words.</param>
    private sealed record BodyRun(Position16<TilemapCells> Start, ushort[] Words);

    /// <summary>One part a writer writes, by the name its frame takes.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Part">The words it writes.</param>
    private sealed record NamedPart(string Name, BodyPart Part);

    /// <summary>A frame cut from within a part (#1466): one of the HUD's images on its shelf.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Part">The part it is cut from.</param>
    /// <param name="Cell">Its upper-left cell within the part.</param>
    /// <param name="Size">Its size in cells.</param>
    private sealed record PartFrame(string Name, string Part, Position16<TilemapCells> Cell, Extent16<TilemapCells> Size);

    /// <summary>A tile word as it shows over the artwork bound at its character.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Shown">The artwork it shows.</param>
    private readonly record struct ShownWord(SnesBgTilemapWord Word, BgArtworkShown Shown);

    /// <summary>A word placed in its part.</summary>
    /// <param name="PartTop">The sheet row its part starts at.</param>
    /// <param name="Shown">The word as it shows.</param>
    /// <param name="Cell">Its cell, from its part's first row.</param>
    private readonly record struct PlacedWord(int PartTop, ShownWord Shown, Position16<TilemapCells> Cell);

    /// <summary>What a layer's words are shown over beyond the room's load.</summary>
    /// <param name="Options">Artwork any room binds over the layer's characters, each word pictured again over.</param>
    /// <param name="Sets">The character sets the layer reads, the first the one its words are read in.</param>
    /// <param name="Bound">Artwork the room binds beyond its load.</param>
    private sealed record TileWordBindings(IReadOnlyList<BgCharacterUpload> Options, BgCharacterSet[] Sets, IReadOnlyList<BgCharacterUpload> Bound);
}

