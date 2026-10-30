using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The installed file-select presentation as the import pipeline reads it (#1463): beside the pages, fields and layout
/// the game binds, the border, cursor and helmet compositions the file-select sheet is composed from.
/// </summary>
public sealed class FileSelectSourcePresentation : FileSelectPresentation
{
    /// <summary>Each bordered heading's sprite.</summary>
    internal readonly Dictionary<FileSelectBorder, SpriteComposition> borderSprites;
    /// <summary>The missile cursor's sprite by animation frame.</summary>
    internal readonly SpriteComposition[] cursorSprites;
    /// <summary>The selected slot's helmet sprite by animation frame.</summary>
    internal readonly SpriteComposition[] helmetSprites;

    /// <summary>Stores the compiled presentation and its sprite compositions.</summary>
    private FileSelectSourcePresentation(FileSelectPresentationDocument document, FileSelectPages pages,
        FileSelectGlyphs glyphs, FileSelectSprites sprites, string contentIdentity)
        : base(document, contentIdentity)
    {
        this.pages = pages.Foreground;
        background = pages.Background;
        patches = pages.Patches;
        digits = PreserveEditedGlyphs(glyphs.Digits, FileSelectFieldTileDefinitions.DigitTileBase);
        slotLetters = PreserveEditedGlyphs(glyphs.SlotLetters, FileSelectFieldTileDefinitions.SamusLetterTileBase);
        borderSprites = sprites.Borders;
        cursorSprites = sprites.Cursors;
        helmetSprites = sprites.Helmets;
    }

    /// <summary>Named, compiled 32-by-32 tilemaps for the menu pages.</summary>
    internal readonly Dictionary<FileSelectPage, BackgroundTile[]> pages;
    /// <summary>The common backdrop tilemap.</summary>
    private readonly BackgroundTile[] background;
    /// <summary>Each slot-field patch.</summary>
    internal readonly Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches;
    /// <summary>Edited digit glyphs, or null when every digit is the native consecutive glyph.</summary>
    internal readonly BackgroundTile[]? digits;
    /// <summary>Edited slot-letter glyphs, or null when every letter is the native consecutive glyph.</summary>
    internal readonly BackgroundTile[]? slotLetters;

    /// <summary>Returns null when every glyph matches its stock consecutive tile.</summary>
    private static BackgroundTile[]? PreserveEditedGlyphs(BackgroundTile[] values, BackgroundTile first)
    {
        for (int index = 0; index < values.Length; index++)
            if (values[index] != first with { Tile = first.Tile + index })
                return values;
        return null;
    }

    /// <summary>The tile of decimal digit <paramref name="digit"/>, using edited glyph art when supplied.</summary>
    internal BackgroundTile Digit(int digit) => (uint)digit < FileSelectPresentationDefinitions.DigitCount
        ? digits is null ? FileSelectFieldTiles.Digit(digit) : digits[digit]
        : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>The tile of save slot <paramref name="slot"/>'s letter, using edited glyph art when supplied.</summary>
    internal BackgroundTile SlotLetter(int slot) => (uint)slot < FileSelectPresentationDefinitions.SlotLetterCount
        ? slotLetters is null ? FileSelectFieldTiles.SlotLetter(slot) : slotLetters[slot]
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
        yield return ScreenPictureSubject.Page(FileSelectParts.BackgroundFrame, background, FileSelectPresentationDefinitions.Width);
        foreach (FileSelectPage page in FileSelectPresentationDefinitions.Pages)
            yield return ScreenPictureSubject.Page(FileSelectParts.PageFrame(page), pages[page], FileSelectPresentationDefinitions.Width);
        foreach (ScreenPictureSubject field in FieldSubjects())
            yield return field;
    }

    /// <summary>
    /// The field pictures, each placed at its anchor: each patch with its cells where they lie from the anchor, each digit,
    /// each slot letter, and each tank filled or empty in each health tens digit's colours.
    /// </summary>
    internal IEnumerable<ScreenPictureSubject> FieldSubjects()
    {
        Extent16<TilemapCells> cell = new(1, 1);
        foreach (FileSelectPatch name in FileSelectPresentationDefinitions.Patches)
        {
            FileSelectCompiledPatch patch = patches[name];
            FileSelectCompiledPatchCell[] cells = [.. Enumerable.Range(0, patch.Count).Select(patch.Cell)];
            int left = cells.Min(each => each.Offset.X), top = cells.Min(each => each.Offset.Y);
            int right = cells.Max(each => each.Offset.X), bottom = cells.Max(each => each.Offset.Y);
            var size = new Extent16<TilemapCells>(checked((ushort)(right - left + 1)), checked((ushort)(bottom - top + 1)));
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

    /// <summary>Reads and validates a complete file-select JSON document, compiling its tilemaps, patches, glyphs, and sprite compositions.</summary>
    /// <param name="source">Readable JSON stream, consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning the parsed layout and compiled artwork, independent of the source stream.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, required names, artwork, anchors, durations, or patch placements are invalid.</exception>
    public static FileSelectSourcePresentation Load(Stream source)
    {
        byte[] bytes;
        using (var copy = new MemoryStream())
        {
            source.CopyTo(copy);
            bytes = copy.ToArray();
        }
        FileSelectPresentationDocument document;
        try
        {
            document = JsonAssetDocument.Read<FileSelectPresentationDocument>(
                bytes, ArtworkJson.Options) ??
                throw new InvalidDataException("File-select presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid file-select presentation JSON.", error);
        }

        if (document.Version != FileSelectPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"File-select presentation version must be {FileSelectPresentationDefinitions.Version}.");
        ValidateExactKeys(document.Pages,
            [FileSelectPresentationDefinitions.BackgroundPageName, .. FileSelectPresentationDefinitions.Pages.Select(FileSelectParts.PageName)],
            "file-select pages");
        ValidateExactKeys(document.Patches, FileSelectPresentationDefinitions.Patches.Select(FileSelectParts.PatchName),
            "file-select patches");
        ValidateExactKeys(document.Sprites, FileSelectParts.SpriteNames,
            "file-select sprites");
        ValidateExactKeys(document.BorderAnchors,
            FileSelectPresentationDefinitions.Borders.Select(FileSelectParts.BorderName), "file-select borders");
        ValidateExactKeys(document.DynamicAnchors,
            FileSelectPresentationDefinitions.Anchors.Select(FileSelectParts.AnchorName),
            "file-select dynamic anchors");
        if (document.Digits is null || document.Digits.Length != 10)
            throw new InvalidDataException("File select requires ten digit cells.");
        if (document.SlotLetters is null || document.SlotLetters.Length != 3)
            throw new InvalidDataException("File select requires three slot-letter cells.");
        if (document.MainSlots is null || document.MainSlots.Length != 3 ||
            document.DataSlots is null || document.DataSlots.Length != 3)
            throw new InvalidDataException("File select requires three main and three data slot layouts.");
        ValidatePoints(document.MainCursorAnchors, 6, "main cursor");
        ValidatePoints(document.DataCursorAnchors, 4, "data cursor");
        ValidatePoints(document.ConfirmationCursorAnchors, 2, "confirmation cursor");
        ValidatePoints(document.HelmetAnchors, 3, "helmet");
        foreach (string name in FileSelectPresentationDefinitions.Borders.Select(FileSelectParts.BorderName))
            ValidatePoint(document.BorderAnchors[name], $"{name} border");
        foreach (string name in FileSelectPresentationDefinitions.Anchors.Select(FileSelectParts.AnchorName))
            ValidateTilePoint(document.DynamicAnchors[name], $"{name} dynamic anchor");
        if ((uint)document.ObjectPalette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException("File-select object palette must be 0..7.");
        if (document.CursorFrameDuration is < 1 or > ushort.MaxValue ||
            document.HelmetFrameDuration is < 1 or > ushort.MaxValue)
            throw new InvalidDataException("File-select actor durations must be 1..65535.");

        var pages = FileSelectPresentationDefinitions.Pages.ToDictionary(page => page,
            page => CompilePage(document, FileSelectParts.PageName(page)));
        BackgroundTile[] background = CompilePage(document, FileSelectPresentationDefinitions.BackgroundPageName);
        var patches = new Dictionary<FileSelectPatch, FileSelectCompiledPatch>();
        foreach (FileSelectPatch kind in FileSelectPresentationDefinitions.Patches)
        {
            string name = FileSelectParts.PatchName(kind);
            FileSelectPatchDocument patch = document.Patches[name] ??
                throw new InvalidDataException($"File-select patch {name} is null.");
            if (patch.Cells is null || patch.Cells.Length == 0)
                throw new InvalidDataException($"File-select patch {name} is empty.");
            var claimed = new HashSet<Position16<TilemapCells>>();
            var compiled = new FileSelectCompiledPatchCell[patch.Cells.Length];
            for (int index = 0; index < compiled.Length; index++)
            {
                FileSelectPatchCellDocument cell = patch.Cells[index] ??
                    throw new InvalidDataException($"File-select patch {name} cell {index} is null.");
                if (cell.X < 0 || cell.Y < 0 ||
                    !claimed.Add(cell.Offset))
                    throw new InvalidDataException(
                        $"File-select patch {name} contains an invalid or duplicate coordinate.");
                compiled[index] = new(cell.Offset - Position16<TilemapCells>.Zero,
                    CompileCell(cell.Cell, $"file-select patch {name} cell {index}"));
            }
            patches.Add(kind, new(kind, compiled));
        }

        BackgroundTile[] digits = document.Digits.Select((cell, index) =>
            CompileCell(cell, $"file-select digit {index}")).ToArray();
        BackgroundTile[] letters = document.SlotLetters.Select((cell, index) =>
            CompileCell(cell, $"file-select slot letter {index}")).ToArray();
        var borderSprites = FileSelectPresentationDefinitions.Borders.ToDictionary(border => border,
            border => MenuHeadingBorder.CalculateIfMatching(Ensure.Listed(FileSelectSpriteDefinitions.Headings, border),
                MenuBorderParts.CalculateIfMatching(CompileSprite(document, FileSelectParts.BorderSpriteName(border)))));
        SpriteComposition[] cursorSprites = [.. Enumerable.Range(0, FileSelectPresentationDefinitions.CursorFrameCount).Select(frame =>
            MenuCursorParts.CalculateIfMatching(frame, CompileSprite(document, FileSelectParts.CursorSpriteName(frame))))];
        SpriteComposition[] helmetSprites = [.. Enumerable.Range(0, FileSelectPresentationDefinitions.HelmetFrameCount).Select(frame =>
            FileSelectHelmetParts.CalculateIfMatching(frame, CompileSprite(document, FileSelectParts.HelmetSpriteName(frame))))];

        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
            ValidateSlot(slot);
        ValidatePatchPlacements(document, patches);
        return new(document, new(pages, background, patches), new(digits, letters),
            new(borderSprites, cursorSprites, helmetSprites), Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document and validates the serialized presentation before writing any bytes to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored layout and artwork to validate and serialize; its collections are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document fails file-select presentation validation.</exception>
    public static void Write(Stream output, FileSelectPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>The import-side presentation <paramref name="fileSelect"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The file-select presentation was not read from installed artwork.</exception>
    public static FileSelectSourcePresentation Of(FileSelectPresentation fileSelect) => fileSelect as FileSelectSourcePresentation
        ?? throw new InvalidDataException("The file-select presentation was not read from installed artwork.");

    /// <summary>Rejects authored field patches or time digits that would escape the 32-by-32 page.</summary>
    private static void ValidatePatchPlacements(
        FileSelectPresentationDocument document,
        Dictionary<FileSelectPatch, FileSelectCompiledPatch> patches)
    {
        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
        {
            ValidatePatch(slot.EnergyAnchor, patches[FileSelectPatch.Energy]);
            ValidatePatch(slot.NoDataAnchor, patches[FileSelectPatch.NoData]);
            ValidatePatch(slot.TimeValueAnchor + new Offset16<TilemapCells>(2, 0),
                patches[FileSelectPatch.TimeColon]);
            if (slot.TimeValueAnchor.X + 5 > FileSelectPresentationDefinitions.Width)
                throw new InvalidDataException("File-select time field escapes its page.");
        }

        static void ValidatePatch(Position16<TilemapCells> anchor, FileSelectCompiledPatch patch)
        {
            for (int index = 0; index < patch.Count; index++)
            {
                Position16<TilemapCells> placed = anchor + patch.Cell(index).Offset;
                if (placed.X >= FileSelectPresentationDefinitions.Width ||
                    placed.Y >= FileSelectPresentationDefinitions.Height)
                    throw new InvalidDataException("File-select patch placement escapes its page.");
            }
        }
    }

    /// <summary>Compiles the 1024-cell page the document names <paramref name="name"/>.</summary>
    private static BackgroundTile[] CompilePage(FileSelectPresentationDocument document, string name)
    {
        MapPresentationCell[] cells = document.Pages[name];
        if (cells is null || cells.Length != FileSelectPresentationDefinitions.CellCount)
            throw new InvalidDataException($"File-select page {name} requires 1024 cells.");
        return CompileCells(cells, $"file-select page {name}");
    }

    /// <summary>Compiles the sprite the document names <paramref name="name"/>.</summary>
    private static SpriteComposition CompileSprite(FileSelectPresentationDocument document, string name) =>
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
            (uint)cell.TileColumn >= FileSelectPresentationDefinitions.CharacterColumns ||
            (uint)cell.TileRow >= FileSelectPresentationDefinitions.CharacterRows ||
            (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"{owner} has an invalid character or palette.");
        return new BackgroundTile(cell.TileRow * FileSelectPresentationDefinitions.CharacterColumns + cell.TileColumn,
            cell.Palette, cell.FlipX, cell.FlipY, cell.Priority);
    }

    /// <summary>Requires every save-slot field anchor to remain within a menu page.</summary>
    private static void ValidateSlot([NotNull] FileSelectSlotFieldDocument? slot)
    {
        if (slot is null)
            throw new InvalidDataException("File-select slot layout is null.");
        ValidateTilePoint(slot.EnergyAnchor, "slot energy");
        ValidateTilePoint(slot.HealthAnchor, "slot health");
        ValidateTilePoint(slot.NoDataAnchor, "slot no-data");
        ValidateTilePoint(slot.TimeValueAnchor, "slot time");
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

    /// <summary>Restricts a dynamic field anchor to the 32-by-32 tilemap.</summary>
    private static void ValidateTilePoint(Position16<TilemapCells> point, string owner)
    {
        if (!FileSelectPresentationDefinitions.PageSize.Contains(point))
            throw new InvalidDataException(
                $"File-select {owner} requires a coordinate inside the 32x32 page.");
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
