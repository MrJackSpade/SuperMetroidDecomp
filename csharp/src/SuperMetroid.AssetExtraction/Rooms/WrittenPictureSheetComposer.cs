using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Pre-assembles and pre-colours the pictures written over each tileset's rooms (#1392, #1466): each picture a writer writes
/// over BG1 or BG2 (a Ceres warning page, a typed glyph) or BG3 shows (an effect's page, the cleared cell), composed from
/// the tile words the cartridge would write for it, coloured with the tileset's standard palette over the characters the
/// room has loaded, and pictured again for each value of the appearance inputs that reach any of its characters, named by
/// <see cref="AppearancePictureNames"/>; with the inputs each picture depends on. BG3's sheet also pictures each of the
/// HUD's images as its own frame.
/// </summary>
internal static class WrittenPictureSheetComposer
{
    /// <summary>Tiles across the tilemaps pictures are written into.</summary>
    private const int SheetTiles = 32;

    /// <summary>
    /// Tiles across the widest sheet the installation can hold, which pictures are packed into: a BG3 sheet pictures each
    /// room effect's whole page again for each frame its effects animate, more than a page-wide sheet could hold.
    /// </summary>
    private const int PackedTiles = RgbaPng.MaximumDimension / RoomTileCanvas.CharacterPixels;

    /// <summary>
    /// Composes the sheet of <paramref name="writers"/>' BG1 and BG2 pictures over <paramref name="content"/>, the room as it
    /// has loaded and, where its backgrounds read the OBJ character table, as its encounters upload there.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture reads a character nothing the room loads is bound over.</exception>
    internal static ComposedWrittenPictureSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads,
        IEnumerable<TileWordWriter> writers, BgArtworkBindings content)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        List<WrittenSubject> subjects = [.. Ensure.NotNull(writers).Order().SelectMany(writer => Subjects(inputs, writer))];
        // Pictures over the sprites' artwork show only what its load and encounters place there, never the tileset's.
        return Compose(inputs, tileset, WrittenPictureLayer.Bg12, new WrittenSubjects(subjects, [], []), content, new WrittenOptions(
            new AppearanceOptions(uploads.WordArtwork == RoomBlockArtwork.Sprites ? [] : uploads.Options), BgCharacterSet.Tiles, uploads.WordUploads));
    }

    /// <summary>
    /// Composes the sheet of the BG3 pictures a room of <paramref name="tileset"/> shows: each of the HUD's images as its own
    /// frame (#1466), the cleared cell, and the page of each room effect its rooms run, over the HUD's characters, and again
    /// for each frame its rooms' effects animate over BG3's characters (#1406).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A picture reads a character nothing the room loads is bound over, or a HUD image reads a character an appearance
    /// input can show other artwork at.
    /// </exception>
    internal static ComposedWrittenPictureSheet ComposeBg3(RoomBlockSheetInputs inputs, TilesetDefinition tileset, RoomBlockUploads.TilesetUploads uploads)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(uploads);
        HudSheetLayout hud = HudImageSheet.Layout(GameplayHudArtwork.Of(inputs.Maps.GameplayHud), HudImageSheet.Maps(inputs.Maps));
        List<WrittenSubject> subjects = [new WrittenSubject(Layer3Plane.ClearedPicture, [new SubjectWord(default, new SnesBgTilemapWord(BackgroundTileWords.ToWord(Layer3ClearedCellDefinitions.ClearedCell)))]),
            .. uploads.FxTypes.Order().Select(type => new WrittenSubject(Layer3Plane.EffectPagePicture(type),
                Rows(BackgroundTileWords.ToWords(inputs.Maps.RoomFxLayer3Tilemaps.Resolve(type).Span))))];
        return Compose(inputs, tileset, WrittenPictureLayer.Bg3, new WrittenSubjects(subjects, [.. HudShelves(hud)], [.. HudFrames(hud)]),
            uploads.Bg3Content ?? throw new InvalidDataException($"No room of {tileset} records what BG3 shows over."),
            new WrittenOptions(new AppearanceOptions(uploads.Bg3Options), BgCharacterSet.Layer3, []));
    }

    /// <summary>The name of the part holding shelf <paramref name="shelf"/> of the HUD's images.</summary>
    private static string HudShelfName(int shelf) => $"HudShelf{shelf}";

    /// <summary>Each shelf of <paramref name="hud"/>'s images as a fixed part: each image's tiles at its place on the shelf.</summary>
    private static IEnumerable<WrittenSubject> HudShelves(HudSheetLayout hud)
    {
        for (int shelf = 0; shelf < hud.ShelfRows.Length; shelf++)
        {
            var words = new List<SubjectWord>();
            foreach (HudSheetPlacement placed in hud.Placements.Where(placed => placed.Shelf == shelf))
            {
                HudImageTiles image = placed.Image;
                for (int row = 0; row < image.Size.Y; row++)
                for (int column = 0; column < image.Size.X; column++)
                {
                    words.Add(new SubjectWord(new((ushort)(placed.Cell.X + column), (ushort)(placed.Cell.Y + row)),
                        new SnesBgTilemapWord(BackgroundTileWords.ToWord(image.Tiles[row * image.Size.X + column]))));
                }
            }
            yield return new WrittenSubject(HudShelfName(shelf), [.. words]);
        }
    }

    /// <summary>The frame of each of <paramref name="hud"/>'s images, within its shelf's part.</summary>
    private static IEnumerable<PartFrame> HudFrames(HudSheetLayout hud) =>
        hud.Placements.Select(placed => new PartFrame(placed.Image.Image.FrameName, HudShelfName(placed.Shelf), placed.Cell, placed.Image.Size));

    /// <summary>
    /// Composes <paramref name="layer"/>'s sheet: <paramref name="written"/>'s fixed parts as the room loads them, each of
    /// its part frames cut from within its part, and each of its subjects pictured once for each
    /// value of the inputs whose options reach any of its characters, over <paramref name="content"/>, which binds the
    /// options' bound artwork beyond the room's load, each word's character read in the options' set.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A picture reads a character nothing the room loads is bound over, or a fixed part reads a character an option shows
    /// other artwork at.
    /// </exception>
    private static ComposedWrittenPictureSheet Compose(RoomBlockSheetInputs inputs, TilesetDefinition tileset, WrittenPictureLayer layer,
        WrittenSubjects written, BgArtworkBindings content, WrittenOptions options)
    {
        Ensure.NotNull(content);
        (List<WrittenSubject> subjects, List<WrittenSubject> fixedParts, IReadOnlyList<PartFrame> partFrames) = written;
        string sheetName = WrittenPictureSheet.NameOf(layer, tileset);
        int characterWords = CharacterWordsOf(layer);
        IReadOnlyList<BgCharacterUpload> load = RoomBlockUploads.LoadUploads(inputs, tileset);
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> bytes = load.Append(RoomBlockUploads.HudCharacters(inputs))
            .Concat(options.Options.All.Select(option => option.Upload)).Concat(options.Bound)
            .GroupBy(upload => upload.Identity).ToDictionary(group => group.Key, group => group.First().Bytes);
        RgbColor[] colours = RoomStaticPaletteImage.Colours(inputs.Palettes.Get(tileset.Palette));

        var pictures = new List<ShownPicture>();
        // A fixed part, such as a shelf of the HUD's images, is one picture no appearance input may show otherwise.
        foreach (WrittenSubject part in fixedParts)
        {
            foreach (SubjectWord word in part.Words)
            {
                int character = word.Word.CharacterIndex;
                if (!RoomAppearanceComposition.InputsOver(options.Set, [character], options.Options).IsEmpty)
                    throw new InvalidDataException($"{sheetName} part {part.Name} reads {options.Set} character {character}, which a room can show other artwork at.");
            }
            pictures.Add(Shown(part, part.Name, [], []) ?? throw new InvalidDataException($"{sheetName} part {part.Name} reads a character its room loads nothing over."));
        }
        var inputsOf = new Dictionary<string, ImmutableArray<AppearanceInput>>(StringComparer.Ordinal);
        foreach (WrittenSubject subject in subjects)
        {
            ImmutableArray<AppearanceInput> depends = RoomAppearanceComposition.InputsOver(options.Set,
                subject.Words.Select(word => word.Word.CharacterIndex).Distinct(), options.Options);
            if (!inputsOf.TryAdd(subject.Name, depends))
                throw new InvalidDataException($"{sheetName} pictures {subject.Name} twice.");
            foreach (string[] values in RoomAppearanceComposition.Assignments(depends, options.Options, withLoaded: true))
            {
                bool loaded = values.All(value => value == RoomAppearanceValues.Loaded);
                ShownPicture? shown = Shown(subject, AppearancePictureNames.Of(subject.Name, depends, values), depends, values);
                if (shown is not null)
                    pictures.Add(shown);
                else if (loaded)
                    throw new InvalidDataException($"{sheetName} picture {subject.Name} reads a character its room loads nothing over.");
            }
        }

        // Pictures are packed onto shelves across the sheet, each shelf as tall as its tallest picture.
        var corners = new Position16<TilemapCells>[pictures.Count];
        int x = 0, y = 0, shelfRows = 0, widest = 1;
        for (int index = 0; index < pictures.Count; index++)
        {
            Extent16<TilemapCells> size = pictures[index].Size;
            if (x + size.X > PackedTiles)
            {
                y += shelfRows;
                x = 0;
                shelfRows = 0;
            }
            corners[index] = new((ushort)x, (ushort)y);
            x += size.X;
            widest = Math.Max(widest, x);
            shelfRows = Math.Max(shelfRows, size.Y);
        }
        int rows = Math.Max(1, y + shelfRows);
        if (rows > PackedTiles)
            throw new InvalidDataException($"{sheetName}'s pictures need more than one {RgbaPng.MaximumDimension}-pixel sheet.");
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(checked((ushort)(widest * RoomTileCanvas.CharacterPixels)),
            checked((ushort)(rows * RoomTileCanvas.CharacterPixels))));
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        var partCorners = new Dictionary<string, Position16<TilemapCells>>(StringComparer.Ordinal);
        for (int index = 0; index < pictures.Count; index++)
        {
            ShownPicture picture = pictures[index];
            Position16<TilemapCells> corner = corners[index];
            foreach (PictureWord word in picture.Words)
                Paint(corner + word.Offset, word.Word);
            partCorners.Add(picture.Name, corner);
            frames.Add(picture.Name, Frame(canvas, Corner(corner), new Extent16<ImagePixels>(
                checked((ushort)(picture.Size.X * RoomTileCanvas.CharacterPixels)), checked((ushort)(picture.Size.Y * RoomTileCanvas.CharacterPixels))), layer));
        }
        foreach (PartFrame frame in partFrames)
        {
            Position16<TilemapCells> corner = partCorners[frame.Part] + (frame.Cell - Position16<TilemapCells>.Zero);
            frames.Add(frame.Name, Frame(canvas, HudImageSheet.Pixels(corner), HudImageSheet.Pixels(frame.Size), layer));
        }
        return new ComposedWrittenPictureSheet(layer, tileset, canvas.ToRoomSheet(colours, frames, WrittenPictureSheet.ColoursOf(layer)), inputsOf);

        // `subject`'s words as they show while `depends` take `values`, named `name`, laid from its top-left written cell;
        // null where a word's character shows nothing for them.
        ShownPicture? Shown(WrittenSubject subject, string name, ImmutableArray<AppearanceInput> depends, string[] values)
        {
            int left = subject.Words.Min(word => word.Cell.X), top = subject.Words.Min(word => word.Cell.Y);
            int right = subject.Words.Max(word => word.Cell.X), bottom = subject.Words.Max(word => word.Cell.Y);
            var origin = new Position16<TilemapCells>((ushort)left, (ushort)top);
            var words = new List<PictureWord>();
            foreach (SubjectWord written in subject.Words)
            {
                SnesBgTilemapWord word = written.Word;
                if (!RoomAppearanceComposition.TryShowing(new CharacterSpan(options.Set, word.CharacterIndex, 1), depends, values,
                        options.Options, content, out BgArtworkShown shown))
                    return null;
                words.Add(new PictureWord(written.Cell - origin, new ShownWord(word, shown)));
            }
            return new ShownPicture(name, new((ushort)(right - left + 1), (ushort)(bottom - top + 1)), words);
        }

        void Paint(Position16<TilemapCells> cell, ShownWord word)
        {
            ReadOnlySpan<byte> character = bytes[word.Shown.Artwork].Span.Slice(word.Shown.WordOffset * sizeof(ushort), characterWords * sizeof(ushort));
            int left = cell.X * RoomTileCanvas.CharacterPixels, top = cell.Y * RoomTileCanvas.CharacterPixels;
            if (layer == WrittenPictureLayer.Bg3)
                canvas.PaintTwoBppTile(left, top, word.Word, character, colours);
            else
                canvas.PaintTile(left, top, word.Word, character, colours);
        }
    }

    /// <summary>VRAM words in one of <paramref name="layer"/>'s characters: BG3's are two-bit, BG1's and BG2's four-bit.</summary>
    private static int CharacterWordsOf(WrittenPictureLayer layer) =>
        (layer == WrittenPictureLayer.Bg3 ? BgCharacterSet.Layer3 : BgCharacterSet.Tiles).CharacterBytes() / sizeof(ushort);

    /// <summary>The pixel corner of the tile at <paramref name="cell"/>.</summary>
    private static Position16<ImagePixels> Corner(Position16<TilemapCells> cell) =>
        new((ushort)(cell.X * RoomTileCanvas.CharacterPixels), (ushort)(cell.Y * RoomTileCanvas.CharacterPixels));

    /// <summary>The frame of <paramref name="rows"/> tile rows from row <paramref name="top"/>, across the sheet.</summary>
    internal static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, int top, int rows, WrittenPictureLayer layer) =>
        Frame(canvas, new Position16<ImagePixels>(0, (ushort)(top * RoomTileCanvas.CharacterPixels)),
            new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)(rows * RoomTileCanvas.CharacterPixels)), layer);

    /// <summary>The frame of the <paramref name="size"/> pixels from <paramref name="corner"/>, coloured from the inks it shows.</summary>
    internal static SpriteSheetFrameDocument Frame(RoomTileCanvas canvas, Position16<ImagePixels> corner, Extent16<ImagePixels> size, WrittenPictureLayer layer)
    {
        Ensure.NotNull(canvas);
        int rowColours = layer == WrittenPictureLayer.Bg3 ? 4 : 16;
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

    /// <summary>The pictures <paramref name="writer"/> writes, each named as gameplay writes it, in a stable order.</summary>
    /// <exception cref="InvalidDataException">The writer's installed artwork is missing.</exception>
    private static IEnumerable<WrittenSubject> Subjects(RoomBlockSheetInputs inputs, TileWordWriter writer) => writer switch
    {
        TileWordWriter.ZebesEscapeText => Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase),
        TileWordWriter.CeresEscapeText => [.. CeresWarningPages(inputs),
            .. Typed(inputs.EscapeTypewriter.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase)],
        _ => throw new InvalidDataException($"Undefined tile word writer {writer}."),
    };

    /// <summary>
    /// The pages of the warning Ceres Ridley's getaway writes over BG1: its EMERGENCY title and the Japanese subtitle's rows,
    /// each a row of words.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed area artwork has no Ceres warning pages, or a page runs past its row.</exception>
    private static IEnumerable<WrittenSubject> CeresWarningPages(RoomBlockSheetInputs inputs)
    {
        CeresEscapeOverlays overlays = inputs.Objects.Areas.CeresEscapeOverlayTilemaps
            ?? throw new InvalidDataException("The installed area artwork has no Ceres warning tilemaps.");
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (page.Cell % SheetTiles + page.WordCount > SheetTiles)
                throw new InvalidDataException($"The Ceres warning page {page.Name} runs past the end of its row.");
            if (!overlays.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles))
                throw new InvalidDataException($"The installed Ceres warning tilemaps hold no {page.Name} page.");
            yield return new WrittenSubject(page.Picture, Rows(BackgroundTileWords.ToWords(tiles.Span)));
        }
    }

    /// <summary>Each glyph <paramref name="program"/> types from <paramref name="tileBase"/>, once, named as the typewriter names it.</summary>
    private static IEnumerable<WrittenSubject> Typed(EscapeTypewriterProgram program, BackgroundTile tileBase)
    {
        var typed = new HashSet<char>();
        foreach (EscapeTypewriterLine line in program.Lines)
        {
            // The typewriter types each character's low byte, and names its picture by that.
            foreach (char character in line.Text.Select(text => (char)(byte)text))
            {
                if (typed.Add(character) && EscapeTypewriterState.GlyphOf(character, tileBase) is { } glyph)
                    yield return new WrittenSubject(EscapeTypewriterGlyph.PictureOf(program.Id, character), [new SubjectWord(default, new SnesBgTilemapWord(BackgroundTileWords.ToWord(glyph)))]);
            }
        }
    }

    /// <summary><paramref name="words"/> laid out from the top-left, 32 to a row.</summary>
    private static SubjectWord[] Rows(ushort[] words) =>
        [.. words.Select((word, index) => new SubjectWord(new((ushort)(index % SheetTiles), (ushort)(index / SheetTiles)), new SnesBgTilemapWord(word)))];

    /// <summary>One tile word a picture writes, at its cell.</summary>
    /// <param name="Cell">The cell, from the picture's own tilemap origin.</param>
    /// <param name="Word">The tile word.</param>
    private readonly record struct SubjectWord(Position16<TilemapCells> Cell, SnesBgTilemapWord Word);

    /// <summary>One picture something writes: the name gameplay writes it by and the tile words it is composed from.</summary>
    /// <param name="Name">The name.</param>
    /// <param name="Words">The words, each at its cell.</param>
    private sealed record WrittenSubject(string Name, SubjectWord[] Words);

    /// <summary>One frame of the sheet: a picture's words as they show for one set of values of its inputs.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Size">Its size in tiles.</param>
    /// <param name="Words">Each word as it shows, at its cell within the frame.</param>
    private sealed record ShownPicture(string Name, Extent16<TilemapCells> Size, List<PictureWord> Words);

    /// <summary>One word of a frame as it shows, at its offset from the frame's top-left cell.</summary>
    /// <param name="Offset">The offset from the frame's top-left cell.</param>
    /// <param name="Word">The word as it shows.</param>
    private readonly record struct PictureWord(Offset16<TilemapCells> Offset, ShownWord Word);

    /// <summary>What a layer's sheet pictures.</summary>
    /// <param name="Subjects">The pictures gameplay writes, each pictured for every value of the inputs that reach it.</param>
    /// <param name="FixedParts">Parts pictured as the room loads them, such as the shelves of the HUD's images.</param>
    /// <param name="PartFrames">Frames cut from within the fixed parts.</param>
    private sealed record WrittenSubjects(List<WrittenSubject> Subjects, List<WrittenSubject> FixedParts, IReadOnlyList<PartFrame> PartFrames);

    /// <summary>A frame cut from within a fixed part (#1466): one of the HUD's images on its shelf.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Part">The part it is cut from.</param>
    /// <param name="Cell">Its upper-left cell within the part.</param>
    /// <param name="Size">Its size in cells.</param>
    private sealed record PartFrame(string Name, string Part, Position16<TilemapCells> Cell, Extent16<TilemapCells> Size);

    /// <summary>A tile word as it shows over the artwork bound at its character.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Shown">The artwork it shows.</param>
    private readonly record struct ShownWord(SnesBgTilemapWord Word, BgArtworkShown Shown);

    /// <summary>What a layer's pictures are shown over beyond the room's load.</summary>
    /// <param name="Options">What the rooms show over the layer's characters after loading, each picture pictured again for.</param>
    /// <param name="Set">The character set the layer's words read.</param>
    /// <param name="Bound">Artwork the room binds beyond its load, which its pictures show over.</param>
    private sealed record WrittenOptions(AppearanceOptions Options, BgCharacterSet Set, IReadOnlyList<BgCharacterUpload> Bound);
}
