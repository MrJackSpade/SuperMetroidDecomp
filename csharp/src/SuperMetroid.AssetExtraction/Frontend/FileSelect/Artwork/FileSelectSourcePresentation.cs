using SuperMetroid.AssetExtraction.Frontend.Map;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The file-select presentation as the import pipeline reads it (#1463, #1638): the layout the game binds
/// (<see cref="Presentation"/>), read from the installed layout document, and the pages, fields and the border, cursor and
/// helmet compositions the file-select sheets are composed from, read from the import-data artwork document, which the game
/// never receives (#1616).
/// </summary>
public sealed class FileSelectSourcePresentation
{
    /// <summary>Each bordered heading's sprite.</summary>
    internal readonly Dictionary<FileSelectBorder, SpriteComposition> borderSprites;
    /// <summary>The missile cursor's sprite by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;
    /// <summary>The selected slot's helmet sprite by animation frame.</summary>
    internal readonly SpriteComposition[] helmetSprites;

    /// <summary>Stores the compiled presentation and its sprite compositions.</summary>
    private FileSelectSourcePresentation(FileSelectPresentation presentation, int objectPalette, FileSelectPages pages,
        FileSelectGlyphs glyphs, FileSelectSprites sprites, string contentIdentity)
    {
        Presentation = presentation;
        ContentIdentity = contentIdentity;
        ObjectPalette = objectPalette;
        this.pages = pages.Foreground;
        background = pages.Background;
        patches = pages.Patches;
        digits = glyphs.Digits;
        slotLetters = glyphs.SlotLetters;
        borderSprites = sprites.Borders;
        cursorSprites = sprites.Cursors;
        helmetSprites = sprites.Helmets;
    }
    /// <summary>The slot fields, anchors and durations the game places its actors by.</summary>
    public FileSelectPresentation Presentation { get; }

    /// <summary>Uppercase SHA-256 digest of the loaded layout and artwork JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }

    /// <summary>The OBJ palette the borders, cursor and helmets are composed in.</summary>
    internal int ObjectPalette { get; }

    /// <summary>The slot fields, anchors and durations of a validated document, which the game places its actors by.</summary>
    private static FileSelectPlacements Placements(FileSelectLayoutDocument document) => new()
    {
        MainSlots = document.MainSlots,
        DataSlots = document.DataSlots,
        BorderAnchors = FileSelectPresentationFormat.Borders.ToDictionary(border => border,
            border => document.BorderAnchors[FileSelectPresentationFormat.BorderName(border)]),
        MainCursorAnchors = document.MainCursorAnchors,
        DataCursorAnchors = document.DataCursorAnchors,
        ConfirmationCursorAnchors = document.ConfirmationCursorAnchors,
        HelmetAnchors = document.HelmetAnchors,
        DynamicAnchors = FileSelectPresentationFormat.Anchors.ToDictionary(anchor => anchor,
            anchor => document.DynamicAnchors[FileSelectPresentationFormat.AnchorName(anchor)]),
        CursorFrameDuration = document.CursorFrameDuration,
        HelmetFrameDuration = document.HelmetFrameDuration,
    };

    /// <summary>Named, compiled 32-by-32 tilemaps for the menu pages.</summary>
    internal readonly Dictionary<FileSelectPage, BackgroundTile[]> pages;
    /// <summary>The common backdrop tilemap.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>Each slot-field patch.</summary>
    internal readonly Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches;
    /// <summary>The installed digit glyphs, zero to nine.</summary>
    private readonly BackgroundTile[] digits;
    /// <summary>The installed slot-letter glyphs, one per save slot.</summary>
    private readonly BackgroundTile[] slotLetters;

    /// <summary>The installed tile of decimal digit <paramref name="digit"/>.</summary>
    internal BackgroundTile Digit(int digit) => (uint)digit < FileSelectPresentationDefinitions.DigitCount
        ? digits[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>The installed tile of save slot <paramref name="slot"/>'s letter.</summary>
    internal BackgroundTile SlotLetter(int slot) => (uint)slot < FileSelectPresentationDefinitions.SlotLetterCount
        ? slotLetters[slot]
        : throw new ArgumentOutOfRangeException(nameof(slot));

    /// <summary>The energy tank's tile, filled or empty, in the colour variant of health tens digit <paramref name="tensDigit"/>.</summary>
    private BackgroundTile Tank(bool filled, int tensDigit) => new(filled ? FileSelectFieldTileDefinitions.FilledTankTile
        : FileSelectFieldTileDefinitions.EmptyTankTile, Digit(tensDigit).ColourVariant, highPriority: false);

    /// <summary>
    /// Every picture of file select the menus' sheet holds (#1467): the backdrop and each page, then each field picture
    /// the game places at a field's anchor.
    /// </summary>
    internal IEnumerable<ScreenPictureSubject> Subjects()
    {
        yield return ScreenPictureSubject.Page(FileSelectParts.BackgroundFrame, background, FileSelectPresentationFormat.PageColumns);
        foreach (FileSelectPage page in FileSelectPresentationFormat.Pages)
            yield return ScreenPictureSubject.Page(FileSelectParts.PageFrame(page), pages[page], FileSelectPresentationFormat.PageColumns);
        foreach (ScreenPictureSubject field in FieldSubjects())
            yield return field;
    }

    /// <summary>
    /// The field pictures, each placed at its anchor: each patch with its cells where they lie from the anchor, each digit,
    /// each slot letter, and each tank filled or empty in each health tens digit's colours.
    /// </summary>
    internal IEnumerable<ScreenPictureSubject> FieldSubjects()
    {
        Extent16<CharacterCells> cell = new(1, 1);
        foreach (FileSelectPatch name in FileSelectPresentationFormat.Patches)
        {
            FileSelectCompiledPatch patch = patches[name];
            FileSelectCompiledPatchCell[] cells = [.. Enumerable.Range(0, patch.Count).Select(patch.Cell)];
            int left = cells.Min(each => each.Offset.X), top = cells.Min(each => each.Offset.Y);
            int right = cells.Max(each => each.Offset.X), bottom = cells.Max(each => each.Offset.Y);
            var size = new Extent16<CharacterCells>(checked((ushort)(right - left + 1)), checked((ushort)(bottom - top + 1)));
            var tiles = new BackgroundTile?[size.X * size.Y];
            foreach (FileSelectCompiledPatchCell each in cells)
                tiles[(each.Offset.Y - top) * size.X + each.Offset.X - left] = each.Tile;
            yield return new ScreenPictureSubject(FileSelectParts.PatchFrame(name), new(checked((short)left), checked((short)top)), size, tiles);
        }
        for (int digit = 0; digit < FileSelectPresentationDefinitions.DigitCount; digit++)
        {
            yield return ScreenPictureSubject.Block(FileSelectParts.DigitFrame(digit), default, cell, new[] { Digit(digit) });
            foreach (bool filled in new[] { false, true })
                yield return ScreenPictureSubject.Block(FileSelectParts.TankFrame(filled, digit), default, cell, new[] { Tank(filled, digit) });
        }
        for (int slot = 0; slot < FileSelectPresentationDefinitions.SlotLetterCount; slot++)
            yield return ScreenPictureSubject.Block(FileSelectParts.LetterFrame(slot), default, cell, new[] { SlotLetter(slot) });
    }

    /// <summary>
    /// The name of every picture of file select the menus' sheet holds (#1467), in <see cref="Subjects"/> order: the names
    /// alone, which the game needs to check the installed sheet and which no installed document holds (#1638).
    /// </summary>
    internal static IEnumerable<string> SubjectNames()
    {
        yield return FileSelectParts.BackgroundFrame;
        foreach (FileSelectPage page in FileSelectPresentationFormat.Pages)
            yield return FileSelectParts.PageFrame(page);
        foreach (FileSelectPatch patch in FileSelectPresentationFormat.Patches)
            yield return FileSelectParts.PatchFrame(patch);
        for (int digit = 0; digit < FileSelectPresentationDefinitions.DigitCount; digit++)
        {
            yield return FileSelectParts.DigitFrame(digit);
            foreach (bool filled in new[] { false, true })
                yield return FileSelectParts.TankFrame(filled, digit);
        }
        for (int slot = 0; slot < FileSelectPresentationDefinitions.SlotLetterCount; slot++)
            yield return FileSelectParts.LetterFrame(slot);
    }

    /// <summary>Reads and validates both extracted file-select documents.</summary>
    /// <param name="extraction">The extraction's layout and artwork documents.</param>
    /// <exception cref="InvalidDataException">Either document is invalid, or a patch placement escapes its page.</exception>
    public static FileSelectSourcePresentation Load(FileSelectExtraction extraction)
    {
        Ensure.NotNull(extraction);
        return Load(new MemoryStream(extraction.Layout, writable: false), new MemoryStream(extraction.Artwork, writable: false));
    }

    /// <summary>
    /// Reads and validates the installed layout document and the import-data artwork document, compiling the layout, the
    /// tilemaps, patches, glyphs and sprite compositions.
    /// </summary>
    /// <param name="layoutSource">Readable layout JSON stream, consumed to the end and left open.</param>
    /// <param name="artworkSource">Readable artwork JSON stream, consumed to the end and left open.</param>
    /// <returns>A presentation owning the parsed layout and compiled artwork, independent of the source streams.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema versions, required names, artwork, anchors, durations, or patch placements are invalid.</exception>
    public static FileSelectSourcePresentation Load(Stream layoutSource, Stream artworkSource)
    {
        byte[] layoutBytes = ReadAll(layoutSource);
        byte[] artworkBytes = ReadAll(artworkSource);
        FileSelectLayoutDocument layout = ReadLayoutDocument(layoutBytes);
        FileSelectArtworkDocument document;
        try
        {
            document = JsonAssetDocument.Read<FileSelectArtworkDocument>(artworkBytes, ArtworkJson.Options) ??
                throw new InvalidDataException("File-select artwork document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid file-select artwork JSON.", error);
        }

        if (document.Version != FileSelectPresentationFormat.ArtworkVersion)
            throw new InvalidDataException(
                $"File-select artwork version must be {FileSelectPresentationFormat.ArtworkVersion}.");
        ValidateExactKeys(document.Pages,
            [FileSelectPresentationFormat.BackgroundPageName, .. FileSelectPresentationFormat.Pages.Select(FileSelectParts.PageName)],
            "file-select pages");
        ValidateExactKeys(document.Patches, FileSelectPresentationFormat.Patches.Select(FileSelectParts.PatchName),
            "file-select patches");
        ValidateExactKeys(document.Sprites, FileSelectPresentationFormat.SpriteNames,
            "file-select sprites");
        if (document.Digits is null || document.Digits.Length != 10)
            throw new InvalidDataException("File select requires ten digit cells.");
        if (document.SlotLetters is null || document.SlotLetters.Length != 3)
            throw new InvalidDataException("File select requires three slot-letter cells.");
        if ((uint)document.ObjectPalette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException("File-select object palette must be 0..7.");

        var pages = FileSelectPresentationFormat.Pages.ToDictionary(page => page,
            page => CompilePage(document, FileSelectParts.PageName(page)));
        BackgroundTile[] background = CompilePage(document, FileSelectPresentationFormat.BackgroundPageName);
        var patches = new Dictionary<FileSelectPatch, FileSelectCompiledPatch>();
        foreach (FileSelectPatch kind in FileSelectPresentationFormat.Patches)
        {
            string name = FileSelectParts.PatchName(kind);
            FileSelectPatchDocument patch = document.Patches[name] ??
                throw new InvalidDataException($"File-select patch {name} is null.");
            if (patch.Cells is null || patch.Cells.Length == 0)
                throw new InvalidDataException($"File-select patch {name} is empty.");
            var claimed = new HashSet<Position16<CharacterCells>>();
            var compiled = new FileSelectCompiledPatchCell[patch.Cells.Length];
            for (int index = 0; index < compiled.Length; index++)
            {
                FileSelectPatchCellDocument cell = patch.Cells[index] ??
                    throw new InvalidDataException($"File-select patch {name} cell {index} is null.");
                if (cell.X < 0 || cell.Y < 0 ||
                    !claimed.Add(cell.Offset))
                    throw new InvalidDataException(
                        $"File-select patch {name} contains an invalid or duplicate coordinate.");
                compiled[index] = new(cell.Offset - Position16<CharacterCells>.Zero,
                    CompileCell(cell.Cell, $"file-select patch {name} cell {index}"));
            }
            patches.Add(kind, new(compiled));
        }

        BackgroundTile[] digits = document.Digits.Select((cell, index) =>
            CompileCell(cell, $"file-select digit {index}")).ToArray();
        BackgroundTile[] letters = document.SlotLetters.Select((cell, index) =>
            CompileCell(cell, $"file-select slot letter {index}")).ToArray();
        var borderSprites = FileSelectPresentationFormat.Borders.ToDictionary(border => border,
            border => CompileSprite(document, FileSelectParts.BorderSpriteName(border)));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, FileSelectPresentationFormat.CursorFrameCount).Select(frame =>
            CompileSprite(document, FileSelectParts.CursorSpriteName(frame)))];
        SpriteComposition[] helmetSprites = [.. Enumerable.Range(0, FileSelectPresentationFormat.HelmetFrameCount).Select(frame =>
            CompileSprite(document, FileSelectParts.HelmetSpriteName(frame)))];

        ValidatePatchPlacements(layout, patches);
        return new(new FileSelectPresentation(Placements(layout)), document.ObjectPalette, new(pages, background, patches),
            new(digits, letters), new(borderSprites, cursorSprites, helmetSprites),
            Convert.ToHexString(SHA256.HashData(layoutBytes.Concat(artworkBytes).ToArray())));
    }

    /// <summary>
    /// Reads and validates the installed layout document alone: the slot fields, anchors and durations the game places its
    /// actors by (#1616, #1638).
    /// </summary>
    /// <param name="source">Readable layout JSON stream, consumed to the end and left open.</param>
    /// <exception cref="InvalidDataException">The JSON, schema version, required names, anchors or durations are invalid.</exception>
    public static FileSelectPresentation ReadLayout(Stream source) =>
        new(Placements(ReadLayoutDocument(ReadAll(source))));

    /// <summary>The whole of <paramref name="source"/>, from its current position.</summary>
    private static byte[] ReadAll(Stream source)
    {
        using var copy = new MemoryStream();
        source.CopyTo(copy);
        return copy.ToArray();
    }

    /// <summary>Parses and validates the layout document in <paramref name="bytes"/>.</summary>
    /// <exception cref="InvalidDataException">The JSON, schema version, required names, anchors or durations are invalid.</exception>
    private static FileSelectLayoutDocument ReadLayoutDocument(byte[] bytes)
    {
        FileSelectLayoutDocument document;
        try
        {
            document = JsonAssetDocument.Read<FileSelectLayoutDocument>(bytes, ArtworkJson.Options) ??
                throw new InvalidDataException("File-select presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid file-select presentation JSON.", error);
        }

        if (document.Version != FileSelectPresentationFormat.Version)
            throw new InvalidDataException(
                $"File-select presentation version must be {FileSelectPresentationFormat.Version}.");
        ValidateExactKeys(document.BorderAnchors,
            FileSelectPresentationFormat.Borders.Select(FileSelectPresentationFormat.BorderName), "file-select borders");
        ValidateExactKeys(document.DynamicAnchors,
            FileSelectPresentationFormat.Anchors.Select(FileSelectPresentationFormat.AnchorName),
            "file-select dynamic anchors");
        if (document.MainSlots is null || document.MainSlots.Length != 3 ||
            document.DataSlots is null || document.DataSlots.Length != 3)
            throw new InvalidDataException("File select requires three main and three data slot layouts.");
        ValidatePoints(document.MainCursorAnchors, 6, "main cursor");
        ValidatePoints(document.DataCursorAnchors, 4, "data cursor");
        ValidatePoints(document.ConfirmationCursorAnchors, 2, "confirmation cursor");
        ValidatePoints(document.HelmetAnchors, 3, "helmet");
        foreach (string name in FileSelectPresentationFormat.Borders.Select(FileSelectPresentationFormat.BorderName))
            ValidatePoint(document.BorderAnchors[name], $"{name} border");
        foreach (string name in FileSelectPresentationFormat.Anchors.Select(FileSelectPresentationFormat.AnchorName))
            ValidatePagePoint(document.DynamicAnchors[name], $"{name} dynamic anchor");
        if (document.CursorFrameDuration < 1 || document.HelmetFrameDuration < 1)
            throw new InvalidDataException("File-select actor durations must be at least one update.");
        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
            ValidateSlot(slot);
        return document;
    }

    /// <summary>Serializes an authored layout document to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored layout to serialize; its collections are not modified.</param>
    public static void Write(Stream output, FileSelectLayoutDocument document) =>
        output.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));

    /// <summary>Serializes an authored artwork document to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored artwork to serialize; its collections are not modified.</param>
    public static void WriteArtwork(Stream output, FileSelectArtworkDocument document) =>
        output.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));

    /// <summary>Rejects authored field patches or time digits that would escape the page.</summary>
    private static void ValidatePatchPlacements(
        FileSelectLayoutDocument document,
        Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches)
    {
        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
        {
            ValidatePatch(slot.EnergyAnchor, patches[FileSelectPatch.Energy]);
            ValidatePatch(slot.NoDataAnchor, patches[FileSelectPatch.NoData]);
            const int glyph = FileSelectFieldDefinitions.GlyphWidthPixels;
            ValidatePatch(slot.TimeValueAnchor + new Offset16<PlanePixels>(FileSelectFieldDefinitions.ColonColumn * glyph, 0),
                patches[FileSelectPatch.TimeColon]);
            if (slot.TimeValueAnchor.X + TimeGlyphs * glyph > FileSelectPresentationDefinitions.PageWidthPixels)
                throw new InvalidDataException("File-select time field escapes its page.");
        }

        static void ValidatePatch(Position16<PlanePixels> anchor, FileSelectCompiledPatch patch)
        {
            for (int index = 0; index < patch.Count; index++)
            {
                Position16<PlanePixels> placed = anchor + patch.Cell(index).Offset.PixelOffset<PlanePixels>();
                if (placed.X >= FileSelectPresentationDefinitions.PageWidthPixels ||
                    placed.Y >= FileSelectPresentationDefinitions.PageHeightPixels)
                    throw new InvalidDataException("File-select patch placement escapes its page.");
            }
        }
    }
    /// <summary>Compiles the 1024-cell page the document names <paramref name="name"/>.</summary>
    private static BackgroundTile[] CompilePage(FileSelectArtworkDocument document, string name)
    {
        MapPresentationCell[] cells = document.Pages[name];
        if (cells is null || cells.Length != FileSelectPresentationFormat.CellCount)
            throw new InvalidDataException($"File-select page {name} requires 1024 cells.");
        return CompileCells(cells, $"file-select page {name}");
    }

    /// <summary>Compiles the sprite the document names <paramref name="name"/>.</summary>
    private static SpriteComposition CompileSprite(FileSelectArtworkDocument document, string name) =>
        MenuSpriteCompiler.Compile(document.Sprites[name], $"file-select {name}");

    /// <summary>Compiles each authored cell of <paramref name="owner"/> to the tile it places.</summary>
    private static BackgroundTile[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var tiles = new BackgroundTile[cells.Length];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = CompileCell(cells[index], $"{owner} cell {index}");
        return tiles;
    }

    /// <summary>Validates an authored cell's tile coordinates and palette and places its tile.</summary>
    private static BackgroundTile CompileCell(MapPresentationCell? cell, string owner)
    {
        if (cell is null ||
            (uint)cell.TileColumn >= FileSelectPresentationFormat.CharacterColumns ||
            (uint)cell.TileRow >= FileSelectPresentationFormat.CharacterRows ||
            (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"{owner} has an invalid character or palette.");
        return new BackgroundTile(cell.TileRow * FileSelectPresentationFormat.CharacterColumns + cell.TileColumn,
            cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
    }

    /// <summary>Requires every save-slot field anchor to remain within a menu page.</summary>
    private static void ValidateSlot([NotNull] FileSelectSlotFieldDocument? slot)
    {
        if (slot is null)
            throw new InvalidDataException("File-select slot layout is null.");
        ValidatePagePoint(slot.EnergyAnchor, "slot energy");
        ValidatePagePoint(slot.HealthAnchor, "slot health");
        ValidatePagePoint(slot.NoDataAnchor, "slot no-data");
        ValidatePagePoint(slot.TimeValueAnchor, "slot time");
    }

    /// <summary>Requires an exact number of valid OAM pixel anchors for a menu layout.</summary>
    private static void ValidatePoints(Position16<SpritePixels>[]? points, int count, string owner)
    {
        if (points is null || points.Length != count)
            throw new InvalidDataException($"File-select {owner} requires {count} points.");
        foreach (Position16<SpritePixels> point in points)
            ValidatePoint(point, owner);
    }

    /// <summary>Restricts a sprite anchor to the sprite plane: X 0..511 and Y 0..255.</summary>
    private static void ValidatePoint(Position16<SpritePixels> point, string owner)
    {
        if (point.X < 0 || point.X > SpritePart.PlaneWidth - 1 ||
            (uint)point.Y > byte.MaxValue)
            throw new InvalidDataException(
                $"File-select {owner} requires X=0..511 and Y=0..255.");
    }

    /// <summary>The glyphs of a slot's HH:MM time: two hour digits, the colon and two minute digits.</summary>
    private const int TimeGlyphs = 5;

    /// <summary>Restricts a dynamic field anchor to a pixel of the page.</summary>
    private static void ValidatePagePoint(Position16<PlanePixels> point, string owner)
    {
        if (point.X >= FileSelectPresentationDefinitions.PageWidthPixels || point.Y >= FileSelectPresentationDefinitions.PageHeightPixels)
            throw new InvalidDataException(
                $"File-select {owner} requires a pixel inside the {FileSelectPresentationDefinitions.PageWidthPixels}x{FileSelectPresentationDefinitions.PageHeightPixels} page.");
    }

    /// <summary>Requires the authored dictionary to contain precisely the schema's named entries.</summary>
    private static void ValidateExactKeys<T>(Dictionary<string, T>? values,
        IEnumerable<string> expected, string owner)
    {
        int count = expected.Count();
        if (values is null || values.Count != count)
            throw new InvalidDataException($"{owner} requires exactly {count} entries.");
        foreach (string name in expected)
            if (!values.ContainsKey(name))
                throw new InvalidDataException($"{owner} is missing {name}.");
    }

    /// <summary>The compiled foreground menu pages, the common background page and the slot-field patches.</summary>
    /// <param name="Foreground">Each foreground page's tiles.</param>
    /// <param name="Background">The common BG2 background's tiles.</param>
    /// <param name="Patches">Each slot-field patch.</param>
    private sealed record FileSelectPages(Dictionary<FileSelectPage, BackgroundTile[]> Foreground, BackgroundTile[] Background,
        Dictionary<FileSelectPatch, FileSelectCompiledPatch> Patches);

    /// <summary>The compiled digit and slot-letter glyphs.</summary>
    /// <param name="Digits">The ten digit glyphs.</param>
    /// <param name="SlotLetters">The three slot-letter glyphs.</param>
    private sealed record FileSelectGlyphs(BackgroundTile[] Digits, BackgroundTile[] SlotLetters);

    /// <summary>The compiled bordered headings, cursor frames and helmet frames.</summary>
    /// <param name="Borders">Each bordered heading's composition.</param>
    /// <param name="Cursors">The missile cursor's composition by animation frame.</param>
    /// <param name="Helmets">The selected slot's helmet composition by animation frame.</param>
    private sealed record FileSelectSprites(
        Dictionary<FileSelectBorder, SpriteComposition> Borders,
        SpriteComposition[] Cursors,
        SpriteComposition[] Helmets);
}
