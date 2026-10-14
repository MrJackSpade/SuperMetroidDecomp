using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable file-select pages, dynamic field artwork, actor compositions and layout.
/// SRAM validation, menu navigation, copy/clear operations and fades remain compiled.
/// </summary>
public sealed class FileSelectPresentation
{
    /// <summary>Named, compiled 32-by-32 BG2 tilemaps for the menu pages.</summary>
    private readonly Dictionary<string, ushort[]> pages;
    /// <summary>ENERGY label cells placed beside populated save slots.</summary>
    internal readonly FileSelectCompiledPatch energyPatch;
    /// <summary>Empty-slot label cells placed where no valid save exists.</summary>
    internal readonly FileSelectCompiledPatch noDataPatch;
    /// <summary>Colon cell inserted between the hour and minute digits.</summary>
    internal readonly FileSelectCompiledPatch timeColonPatch;
    /// <summary>Edited digit tile words, or null when the stock consecutive tiles suffice.</summary>
    internal readonly ushort[]? digits;
    /// <summary>Edited slot-letter tile words, or null for the stock consecutive tiles.</summary>
    internal readonly ushort[]? slotLetters;
    /// <summary>Named border, cursor, and helmet compositions used by the menu renderer.</summary>
    internal readonly Dictionary<string, SpriteComposition> sprites;
    /// <summary>Validated anchors and frame durations from the selected JSON presentation.</summary>
    private readonly FileSelectPresentationDocument document;

    /// <summary>Stores compiled artwork and collapses unedited glyph sequences to stock tile bases.</summary>
    private FileSelectPresentation(
        Dictionary<string, ushort[]> pages,
        Dictionary<string, FileSelectCompiledPatch> patches,
        ushort[] digits,
        ushort[] slotLetters,
        Dictionary<string, SpriteComposition> sprites,
        FileSelectPresentationDocument document,
        string contentIdentity)
    {
        this.pages = pages;
        energyPatch = patches[FileSelectPresentationDefinitions.EnergyPatch];
        noDataPatch = patches[FileSelectPresentationDefinitions.NoDataPatch];
        timeColonPatch = patches[FileSelectPresentationDefinitions.TimeColonPatch];
        this.digits = PreserveEditedGlyphs(digits, FileSelectLayout.DigitTileBase);
        this.slotLetters = PreserveEditedGlyphs(slotLetters, FileSelectLayout.SamusLetterTileBase);
        this.sprites = sprites;
        this.document = document;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Returns null when every glyph matches its stock consecutive tile word.</summary>
    private static ushort[]? PreserveEditedGlyphs(ushort[] values, ushort first)
    {
        for (int index = 0; index < values.Length; index++)
            if (values[index] != first + index)
                return values;
        return null;
    }
    /// <summary>Uppercase SHA-256 digest of the loaded JSON bytes, identifying the selected presentation including its encoding.</summary>
    public string ContentIdentity { get; }
    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public int CursorFrameDuration => document.CursorFrameDuration;
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public int HelmetFrameDuration => document.HelmetFrameDuration;

    /// <summary>Copies the background page into the menu's BG2 VRAM tilemap.</summary>
    internal void LoadBackground(SnesVram vram) =>
        new MenuVram(vram).Bg2Tilemap.Write(pages[FileSelectPresentationDefinitions.BackgroundPage]);

    /// <summary>Copies one named full page into a 1024-cell destination tilemap.</summary>
    internal void CopyPage(string name, Span<ushort> destination)
    {
        if (!pages.TryGetValue(name, out ushort[]? source))
            throw new InvalidDataException($"Unknown file-select page {name}.");
        Ensure.LengthEqual(destination, FileSelectPresentationDefinitions.CellCount);
        source.CopyTo(destination);
    }

    /// <summary>Selects the main or data-management field anchors for one save slot.</summary>
    internal FileSelectSlotFieldDocument Slot(bool dataManagement, int slot)
    {
        FileSelectSlotFieldDocument[] layouts = dataManagement
            ? document.DataSlots
            : document.MainSlots;
        return (uint)slot < layouts.Length
            ? layouts[slot]
            : throw new ArgumentOutOfRangeException(nameof(slot));
    }

    /// <summary>Places an ENERGY, NO DATA, or time-colon patch relative to its tile anchor.</summary>
    internal void ApplyPatch(Span<ushort> tilemap, string name, Position16<TilemapCells> anchor)
    {
        FileSelectCompiledPatch patch = name switch
        {
            FileSelectPresentationDefinitions.EnergyPatch => energyPatch,
            FileSelectPresentationDefinitions.NoDataPatch => noDataPatch,
            FileSelectPresentationDefinitions.TimeColonPatch => timeColonPatch,
            _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
        };
        for (int index = 0; index < patch.Count; index++)
        {
            FileSelectCompiledPatchCell cell = patch.Cell(index);
            tilemap[FileSelectPresentationDefinitions.PageSize.RowMajorIndex(anchor + cell.Offset)] = cell.Word;
        }
    }

    /// <summary>Writes one decimal digit using edited glyph art when supplied.</summary>
    internal void WriteDigit(Span<ushort> tilemap, Position16<TilemapCells> anchor, int offset, int digit)
    {
        if ((uint)digit >= FileSelectPresentationDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException(nameof(digit));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X + offset] =
            digits is null ? (ushort)(FileSelectLayout.DigitTileBase + digit) : digits[digit];
    }

    /// <summary>Writes the selected slot's letter at its dynamic tile anchor.</summary>
    internal void WriteSlotLetter(Span<ushort> tilemap, Position16<TilemapCells> anchor, int slot)
    {
        if ((uint)slot >= FileSelectPresentationDefinitions.SlotLetterCount)
            throw new ArgumentOutOfRangeException(nameof(slot));
        tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X] =
            slotLetters is null ? (ushort)(FileSelectLayout.SamusLetterTileBase + slot) : slotLetters[slot];
    }

    /// <summary>Resolves a named dynamic field location in the menu tilemap.</summary>
    internal Position16<TilemapCells> DynamicAnchor(string name) =>
        document.DynamicAnchors.TryGetValue(name, out Position16<TilemapCells> anchor)
            ? anchor
            : throw new InvalidDataException($"Unknown file-select dynamic anchor {name}.");

    /// <summary>Selects the cursor anchor from main, confirmation, or data-management layouts.</summary>
    internal Position16<OamPixels> CursorPosition(bool main, bool confirmation, int selected) =>
        Point(main
            ? document.MainCursorAnchors
            : confirmation
                ? document.ConfirmationCursorAnchors
                : document.DataCursorAnchors,
            selected, "cursor");

    /// <summary>Draws the page's border composition at its authored OAM anchor.</summary>
    internal void DrawBorder(OamBuffer oam, string page)
    {
        Position16<OamPixels> anchor = document.BorderAnchors[page];
        sprites[FileSelectPresentationDefinitions.BorderFrameName(page)].DrawOnScreen(
            oam, anchor,
            PaletteBits(document.ObjectPalette));
    }

    /// <summary>Draws one missile-cursor frame with the menu object palette.</summary>
    internal void DrawCursor(OamBuffer oam, int frame, Position16<OamPixels> anchor) =>
        sprites[FileSelectPresentationDefinitions.CursorFrameName(frame)].DrawOnScreen(
            oam, anchor,
            PaletteBits(document.ObjectPalette));

    /// <summary>Draws a selected-slot helmet frame at that slot's OAM anchor.</summary>
    internal void DrawHelmet(OamBuffer oam, int frame, int slot)
    {
        Position16<OamPixels> anchor = Point(document.HelmetAnchors, slot, "helmet");
        sprites[FileSelectPresentationDefinitions.HelmetFrameName(frame)].DrawOnScreen(
            oam, anchor,
            PaletteBits(document.ObjectPalette));
    }

    /// <summary>Reads and validates a complete file-select JSON document, compiling its tilemaps, patches, glyphs, and sprite compositions.</summary>
    /// <param name="source">Readable JSON stream, consumed from its current position to the end and left open.</param>
    /// <returns>A presentation owning the parsed layout and compiled artwork, independent of the source stream.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, required names, artwork, anchors, durations, or patch placements are invalid.</exception>
    public static FileSelectPresentation Load(Stream source)
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
                bytes, MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("File-select presentation document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid file-select presentation JSON.", error);
        }

        if (document.Version != FileSelectPresentationDefinitions.Version)
            throw new InvalidDataException(
                $"File-select presentation version must be {FileSelectPresentationDefinitions.Version}.");
        ValidateExactKeys(document.Pages, FileSelectPresentationDefinitions.PageNames,
            "file-select pages");
        ValidateExactKeys(document.Patches, FileSelectPresentationDefinitions.PatchNames,
            "file-select patches");
        ValidateExactKeys(document.Sprites, FileSelectPresentationDefinitions.SpriteNames,
            "file-select sprites");
        ValidateExactKeys(document.BorderAnchors,
            FileSelectPresentationDefinitions.BorderNames, "file-select borders");
        ValidateExactKeys(document.DynamicAnchors,
            FileSelectPresentationDefinitions.DynamicAnchorNames,
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
        foreach (string name in FileSelectPresentationDefinitions.BorderNames)
            ValidatePoint(document.BorderAnchors[name], $"{name} border");
        foreach (string name in FileSelectPresentationDefinitions.DynamicAnchorNames)
            ValidateTilePoint(document.DynamicAnchors[name], $"{name} dynamic anchor");
        if ((uint)document.ObjectPalette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException("File-select object palette must be 0..7.");
        if (document.CursorFrameDuration is < 1 or > ushort.MaxValue ||
            document.HelmetFrameDuration is < 1 or > ushort.MaxValue)
            throw new InvalidDataException("File-select actor durations must be 1..65535.");

        var pages = new Dictionary<string, ushort[]>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.PageNames)
        {
            MapPresentationCell[] cells = document.Pages[name];
            if (cells is null || cells.Length != FileSelectPresentationDefinitions.CellCount)
                throw new InvalidDataException($"File-select page {name} requires 1024 cells.");
            pages.Add(name, CompileCells(cells, $"file-select page {name}"));
        }
        var patches = new Dictionary<string, FileSelectCompiledPatch>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.PatchNames)
        {
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
            patches.Add(name, new(name, compiled));
        }

        ushort[] digits = document.Digits.Select((cell, index) =>
            CompileCell(cell, $"file-select digit {index}")).ToArray();
        ushort[] letters = document.SlotLetters.Select((cell, index) =>
            CompileCell(cell, $"file-select slot letter {index}")).ToArray();
        var sprites = new Dictionary<string, SpriteComposition>(StringComparer.Ordinal);
        foreach (string name in FileSelectPresentationDefinitions.SpriteNames)
            sprites.Add(name, MenuHeadingBorderDefinitions.CalculateIfMatching(name, FileSelectHelmetParts.CalculateIfMatching(name, MenuBorderParts.CalculateIfMatching(name, MenuCursorParts.CalculateIfMatching(name, MenuSpriteCompiler.Compile(document.Sprites[name],
                $"file-select {name}"))))));

        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
            ValidateSlot(slot);
        ValidatePatchPlacements(document, patches);
        return new(pages, patches, digits, letters, sprites, document,
            Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Serializes an authored document and validates the serialized presentation before writing any bytes to the output.</summary>
    /// <param name="output">Writable stream receiving UTF-8 JSON at its current position; left open.</param>
    /// <param name="document">Authored layout and artwork to validate and serialize; its collections are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document fails file-select presentation validation.</exception>
    public static void Write(Stream output, FileSelectPresentationDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Rejects authored field patches or time digits that would escape the 32-by-32 page.</summary>
    private static void ValidatePatchPlacements(
        FileSelectPresentationDocument document,
        Dictionary<string, FileSelectCompiledPatch> patches)
    {
        foreach (FileSelectSlotFieldDocument slot in document.MainSlots.Concat(document.DataSlots))
        {
            ValidatePatch(slot.EnergyAnchor, patches[FileSelectPresentationDefinitions.EnergyPatch]);
            ValidatePatch(slot.NoDataAnchor, patches[FileSelectPresentationDefinitions.NoDataPatch]);
            ValidatePatch(slot.TimeValueAnchor + new Offset16<TilemapCells>(2, 0),
                patches[FileSelectPresentationDefinitions.TimeColonPatch]);
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

    /// <summary>Converts authored page cells into native BG tilemap words.</summary>
    private static ushort[] CompileCells(MapPresentationCell[] cells, string owner)
    {
        var words = new ushort[cells.Length];
        for (int index = 0; index < words.Length; index++)
            words[index] = CompileCell(cells[index], $"{owner} cell {index}");
        return words;
    }

    /// <summary>Validates tile coordinates and packs palette, priority, and flip flags into one word.</summary>
    private static ushort CompileCell(MapPresentationCell? cell, string owner)
    {
        if (cell is null ||
            (uint)cell.TileColumn >= FileSelectPresentationDefinitions.CharacterColumns ||
            (uint)cell.TileRow >= FileSelectPresentationDefinitions.CharacterRows ||
            (uint)cell.Palette >= MapPresentationFormat.PaletteCount)
            throw new InvalidDataException($"{owner} has an invalid character or palette.");
        return checked((ushort)(
            cell.TileRow * FileSelectPresentationDefinitions.CharacterColumns + cell.TileColumn |
            cell.Palette << MapPresentationFormat.PaletteShift |
            (cell.Priority ? MapPresentationFormat.PriorityBit : 0) |
            (cell.FlipX ? MapPresentationFormat.FlipXBit : 0) |
            (cell.FlipY ? MapPresentationFormat.FlipYBit : 0)));
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
    private static void ValidatePoints(Position16<OamPixels>[]? points, int count, string owner)
    {
        if (points is null || points.Length != count)
            throw new InvalidDataException($"File-select {owner} requires {count} points.");
        foreach (Position16<OamPixels> point in points)
            ValidatePoint(point, owner);
    }

    /// <summary>Restricts a sprite anchor to the SNES nine-bit X and eight-bit Y range.</summary>
    private static void ValidatePoint(Position16<OamPixels> point, string owner)
    {
        if (point.X < 0 || point.X > 0x1ff ||
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

    /// <summary>Reads an indexed OAM anchor, rejecting selections outside its layout.</summary>
    private static Position16<OamPixels> Point(Position16<OamPixels>[] points, int index, string owner) =>
        (uint)index < points.Length ? points[index] :
            throw new ArgumentOutOfRangeException(owner);

    /// <summary>Encodes the chosen OBJ palette into a sprite attribute word.</summary>
    private static ushort PaletteBits(int index) =>
        SnesObjAttributeWord.Create(0, index, 0).PaletteBits;
}
